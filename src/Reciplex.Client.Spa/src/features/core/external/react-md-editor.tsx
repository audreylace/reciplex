/** code lifted from @uiw/react-md-editor to avoid the extra css and other dependencies */

/*
MIT License

Copyright (c) 2020 uiw

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

/* eslint-disable prefer-const */
/* eslint-disable @typescript-eslint/no-explicit-any */
import type { TextAreaCommandOrchestrator } from "@uiw/react-md-editor/commands";
import {
  getCommands,
  TextAreaTextApi,
  type TextRange,
} from "@uiw/react-md-editor/commands";

/**
 * - `13` - `Enter`
 * - `9` - `Tab`
 */
function stopPropagation(
  e: KeyboardEvent | React.KeyboardEvent<HTMLTextAreaElement>,
) {
  e.stopPropagation();
  e.preventDefault();
}

function handleLineMove(
  e: KeyboardEvent | React.KeyboardEvent<HTMLTextAreaElement>,
  direction: number,
) {
  stopPropagation(e);
  const target = e.target as HTMLTextAreaElement;
  const textArea = new TextAreaTextApi(target);
  let selection = { start: target.selectionStart, end: target.selectionEnd };
  selection = selectLine({ text: target.value, selection });
  if (
    (direction < 0 && selection.start <= 0) ||
    (direction > 0 && selection.end >= target.value.length)
  ) {
    return;
  }

  const blockText = target.value.slice(selection.start, selection.end);
  if (direction < 0) {
    const prevLineSelection = selectLine({
      text: target.value,
      selection: { start: selection.start - 1, end: selection.start - 1 },
    });
    const prevLineText = target.value.slice(
      prevLineSelection.start,
      prevLineSelection.end,
    );
    textArea.setSelectionRange({
      start: prevLineSelection.start,
      end: selection.end,
    });
    insertTextAtPosition(target, `${blockText}\n${prevLineText}`);
    textArea.setSelectionRange({
      start: prevLineSelection.start,
      end: prevLineSelection.start + blockText.length,
    });
  } else {
    const nextLineSelection = selectLine({
      text: target.value,
      selection: { start: selection.end + 1, end: selection.end + 1 },
    });
    const nextLineText = target.value.slice(
      nextLineSelection.start,
      nextLineSelection.end,
    );
    textArea.setSelectionRange({
      start: selection.start,
      end: nextLineSelection.end,
    });
    insertTextAtPosition(target, `${nextLineText}\n${blockText}`);
    textArea.setSelectionRange({
      start: nextLineSelection.end - blockText.length,
      end: nextLineSelection.end,
    });
  }
}

export default function handleKeyDown(
  e: KeyboardEvent | React.KeyboardEvent<HTMLTextAreaElement>,
  tabSize: number = 2,
  defaultTabEnable: boolean = false,
) {
  const target = e.target as HTMLTextAreaElement;
  const starVal = target.value.substr(0, target.selectionStart);
  const valArr = starVal.split("\n");
  const currentLineStr = valArr[valArr.length - 1];
  const textArea = new TextAreaTextApi(target);

  /**
   * `9` - `Tab`
   */
  if (!defaultTabEnable && e.code && e.code.toLowerCase() === "tab") {
    stopPropagation(e);
    const space = new Array(tabSize + 1).join("  ");
    if (target.selectionStart !== target.selectionEnd) {
      const _star = target.value
        .substring(0, target.selectionStart)
        .split("\n");
      const _end = target.value.substring(0, target.selectionEnd).split("\n");
      const modifiedTextLine: string[] = [];
      _end.forEach((item, idx) => {
        if (item !== _star[idx]) {
          modifiedTextLine.push(item);
        }
      });
      const modifiedText = modifiedTextLine.join("\n");
      const oldSelectText = target.value.substring(
        target.selectionStart,
        target.selectionEnd,
      );
      const newStarNum = target.value.substring(
        0,
        target.selectionStart,
      ).length;

      textArea.setSelectionRange({
        start: target.value.indexOf(modifiedText),
        end: target.selectionEnd,
      });

      const modifiedTextObj = insertBeforeEachLine(
        modifiedText,
        e.shiftKey ? "" : space,
      );

      let text = modifiedTextObj.modifiedText;
      if (e.shiftKey) {
        text = text
          .split("\n")
          .map((item) => item.replace(new RegExp(`^${space}`), ""))
          .join("\n");
      }
      textArea.replaceSelection(text);

      const startTabSize = e.shiftKey ? -tabSize : tabSize;
      const endTabSize = e.shiftKey
        ? -modifiedTextLine.length * tabSize
        : modifiedTextLine.length * tabSize;

      textArea.setSelectionRange({
        start: newStarNum + startTabSize,
        end: newStarNum + oldSelectText.length + endTabSize,
      });
    } else {
      return insertTextAtPosition(target, space);
    }
  } else if (
    e.keyCode === 13 &&
    e.code.toLowerCase() === "enter" &&
    (/^(-|\*)\s/.test(currentLineStr) || /^\d+.\s/.test(currentLineStr)) &&
    !e.shiftKey
  ) {
    /**
     * `13` - `Enter`
     */
    stopPropagation(e);
    let startStr = "\n- ";

    if (currentLineStr.startsWith("*")) {
      startStr = "\n* ";
    }

    if (
      currentLineStr.startsWith("- [ ]") ||
      currentLineStr.startsWith("- [X]") ||
      currentLineStr.startsWith("- [x]")
    ) {
      startStr = "\n- [ ] ";
    }

    if (/^\d+.\s/.test(currentLineStr)) {
      startStr = `\n${parseInt(currentLineStr) + 1}. `;
    }
    return insertTextAtPosition(target, startStr);
  } else if (e.code && e.code.toLowerCase() === "keyd" && e.ctrlKey) {
    // Duplicate lines
    stopPropagation(e);
    let selection = { start: target.selectionStart, end: target.selectionEnd };
    const savedSelection = selection;
    selection = selectLine({ text: target.value, selection });
    const textToDuplicate = target.value.slice(selection.start, selection.end);
    textArea.setSelectionRange({ start: selection.end, end: selection.end });
    insertTextAtPosition(target, `\n${textToDuplicate}`);
    textArea.setSelectionRange({
      start: savedSelection.start,
      end: savedSelection.end,
    });
  } else if (e.code && e.code.toLowerCase() === "arrowup" && e.altKey) {
    handleLineMove(e, -1);
  } else if (e.code && e.code.toLowerCase() === "arrowdown" && e.altKey) {
    handleLineMove(e, 1);
  }
}

/**
 * The MIT License
 * Copyright (c) 2018 Dmitriy Kubyshkin
 * Copied from https://github.com/grassator/insert-text-at-cursor
 */

let browserSupportsTextareaTextNodes: any;

/**
 * @param {HTMLElement} input
 * @return {boolean}
 */
function canManipulateViaTextNodes(
  input: HTMLTextAreaElement | HTMLInputElement,
): boolean {
  if (input.nodeName !== "TEXTAREA") {
    return false;
  }
  if (typeof browserSupportsTextareaTextNodes === "undefined") {
    const textarea: HTMLTextAreaElement = document.createElement("textarea");
    textarea.value = "1";
    browserSupportsTextareaTextNodes = !!textarea.firstChild;
  }
  return browserSupportsTextareaTextNodes;
}

/**
 * @param {HTMLTextAreaElement|HTMLInputElement} input
 * @param {string} text
 * @returns {void}
 */
export function insertTextAtPosition(
  input: HTMLTextAreaElement | HTMLInputElement,
  text: string,
): void {
  // Most of the used APIs only work with the field selected
  input.focus();

  // IE 8-10
  if ((document as any).selection) {
    const ieRange = (document as any).selection.createRange();
    ieRange.text = text;

    // Move cursor after the inserted text
    ieRange.collapse(false /* to the end */);
    ieRange.select();

    return;
  }

  // Webkit + Edge
  let isSuccess;
  if (text !== "") {
    isSuccess =
      document.execCommand && document.execCommand("insertText", false, text);
  } else {
    isSuccess = document.execCommand && document.execCommand("delete", false);
  }

  if (!isSuccess) {
    const start = input.selectionStart!;
    const end = input.selectionEnd!;
    // Firefox (non-standard method)
    if (typeof input.setRangeText === "function") {
      input.setRangeText(text);
    } else {
      // To make a change we just need a Range, not a Selection
      const range = document.createRange();
      const textNode = document.createTextNode(text);

      if (canManipulateViaTextNodes(input)) {
        let node = input.firstChild;

        // If textarea is empty, just insert the text
        if (!node) {
          input.appendChild(textNode);
        } else {
          // Otherwise we need to find a nodes for start and end
          let offset = 0;
          let startNode = null;
          let endNode = null;

          while (node && (startNode === null || endNode === null)) {
            const nodeLength = node.nodeValue!.length;

            // if start of the selection falls into current node
            if (start >= offset && start <= offset + nodeLength) {
              range.setStart((startNode = node), start - offset);
            }

            // if end of the selection falls into current node
            if (end >= offset && end <= offset + nodeLength) {
              range.setEnd((endNode = node), end - offset);
            }

            offset += nodeLength;
            node = node.nextSibling;
          }

          // If there is some text selected, remove it as we should replace it
          if (start !== end) {
            range.deleteContents();
          }
        }
      }

      // If the node is a textarea and the range doesn't span outside the element
      //
      // Get the commonAncestorContainer of the selected range and test its type
      // If the node is of type `#text` it means that we're still working with text nodes within our textarea element
      // otherwise, if it's of type `#document` for example it means our selection spans outside the textarea.
      if (
        canManipulateViaTextNodes(input) &&
        range.commonAncestorContainer.nodeName === "#text"
      ) {
        // Finally insert a new node. The browser will automatically split start and end nodes into two if necessary
        range.insertNode(textNode);
      } else {
        // If the node is not a textarea or the range spans outside a textarea the only way is to replace the whole value
        const value = input.value;
        input.value = value.slice(0, start) + text + value.slice(end);
      }
    }

    // Correct the cursor position to be at the end of the insertion
    input.setSelectionRange(start + text.length, start + text.length);

    // Notify any possible listeners of the change
    const e = document.createEvent("UIEvent");
    e.initEvent("input", true, false);
    input.dispatchEvent(e);
  }
}

export interface TextSection {
  text: string;
  selection: TextRange;
}

export function selectWord({
  text,
  selection,
  prefix,
  suffix = prefix,
}: {
  text: string;
  selection: TextRange;
  prefix: string;
  suffix?: string;
}): TextRange {
  let result = selection;
  if (text && text.length && selection.start === selection.end) {
    result = getSurroundingWord(text, selection.start);
  }
  if (
    result.start >= prefix.length &&
    result.end <= text.length - suffix.length
  ) {
    const selectedTextContext = text.slice(
      result.start - prefix.length,
      result.end + suffix.length,
    );
    if (
      selectedTextContext.startsWith(prefix) &&
      selectedTextContext.endsWith(suffix)
    ) {
      return {
        start: result.start - prefix.length,
        end: result.end + suffix.length,
      };
    }
  }
  return result;
}

export function selectLine({ text, selection }: TextSection): TextRange {
  const start = text.slice(0, selection.start).lastIndexOf("\n") + 1;
  let end = text.slice(selection.end).indexOf("\n") + selection.end;
  if (end === selection.end - 1) {
    end = text.length;
  }
  return { start, end };
}

/**
 *  Gets the number of line-breaks that would have to be inserted before the given 'startPosition'
 *  to make sure there's an empty line between 'startPosition' and the previous text
 */
export function getBreaksNeededForEmptyLineBefore(
  text = "",
  startPosition: number,
): number {
  if (startPosition === 0) return 0;

  // rules:
  // - If we're in the first line, no breaks are needed
  // - Otherwise there must be 2 breaks before the previous character. Depending on how many breaks exist already, we
  //      may need to insert 0, 1 or 2 breaks

  let neededBreaks = 2;
  let isInFirstLine = true;
  for (let i = startPosition - 1; i >= 0 && neededBreaks >= 0; i--) {
    switch (text.charCodeAt(i)) {
      case 32: // blank space
        continue;
      case 10: // line break
        neededBreaks--;
        isInFirstLine = false;
        break;
      default:
        return neededBreaks;
    }
  }
  return isInFirstLine ? 0 : neededBreaks;
}

/**
 *  Gets the number of line-breaks that would have to be inserted after the given 'startPosition'
 *  to make sure there's an empty line between 'startPosition' and the next text
 */
export function getBreaksNeededForEmptyLineAfter(
  text = "",
  startPosition: number,
): number {
  if (startPosition === text.length - 1) return 0;

  // rules:
  // - If we're in the first line, no breaks are needed
  // - Otherwise there must be 2 breaks before the previous character. Depending on how many breaks exist already, we
  //      may need to insert 0, 1 or 2 breaks

  let neededBreaks = 2;
  let isInLastLine = true;
  for (let i = startPosition; i < text.length && neededBreaks >= 0; i++) {
    switch (text.charCodeAt(i)) {
      case 32:
        continue;
      case 10: {
        neededBreaks--;
        isInLastLine = false;
        break;
      }
      default:
        return neededBreaks;
    }
  }
  return isInLastLine ? 0 : neededBreaks;
}

export function getSurroundingWord(text: string, position: number): TextRange {
  if (!text) throw Error("Argument 'text' should be truthy");

  const isWordDelimiter = (c: string) => c === " " || c.charCodeAt(0) === 10;

  // leftIndex is initialized to 0 because if selection is 0, it won't even enter the iteration
  let start = 0;
  // rightIndex is initialized to text.length because if selection is equal to text.length it won't even enter the interation
  let end = text.length;

  // iterate to the left
  for (let i = position; i - 1 > -1; i--) {
    if (isWordDelimiter(text[i - 1])) {
      start = i;
      break;
    }
  }

  // iterate to the right
  for (let i = position; i < text.length; i++) {
    if (isWordDelimiter(text[i])) {
      end = i;
      break;
    }
  }

  return { start, end };
}

export function executeCommand({
  api,
  selectedText,
  selection,
  prefix,
  suffix = prefix,
}: {
  api: TextAreaTextApi;
  selectedText: string;
  selection: TextRange;
  prefix: string;
  suffix?: string;
}) {
  if (
    selectedText.length >= prefix.length + suffix.length &&
    selectedText.startsWith(prefix) &&
    selectedText.endsWith(suffix)
  ) {
    api.replaceSelection(
      selectedText.slice(
        prefix.length,
        suffix.length ? -suffix.length : undefined,
      ),
    );
    api.setSelectionRange({
      start: selection.start - prefix.length,
      end: selection.end - prefix.length,
    });
  } else {
    api.replaceSelection(`${prefix}${selectedText}${suffix}`);
    api.setSelectionRange({
      start: selection.start + prefix.length,
      end: selection.end + prefix.length,
    });
  }
}

export type AlterLineFunction = (line: string, index: number) => string;

/**
 * Inserts insertionString before each line
 */
export function insertBeforeEachLine(
  selectedText: string,
  insertBefore: string | AlterLineFunction,
): { modifiedText: string; insertionLength: number } {
  const lines = selectedText.split(/\n/);

  let insertionLength = 0;
  const modifiedText = lines
    .map((item, index) => {
      if (typeof insertBefore === "string") {
        if (item.startsWith(insertBefore)) {
          insertionLength -= insertBefore.length;
          return item.slice(insertBefore.length);
        }
        insertionLength += insertBefore.length;
        return insertBefore + item;
      }
      if (typeof insertBefore === "function") {
        if (item.startsWith(insertBefore(item, index))) {
          insertionLength -= insertBefore(item, index).length;
          return item.slice(insertBefore(item, index).length);
        }
        const insertionResult = insertBefore(item, index);
        insertionLength += insertionResult.length;
        return insertBefore(item, index) + item;
      }
      throw Error("insertion is expected to be either a string or a function");
    })
    .join("\n");

  return { modifiedText, insertionLength };
}

export function shortcutsHandle(
  e: KeyboardEvent | React.KeyboardEvent<HTMLTextAreaElement>,
  commandOrchestrator?: TextAreaCommandOrchestrator,
) {
  const data = getCommands();
  const shortcuts: string[] = [];
  if (e.altKey) {
    shortcuts.push("alt");
  }
  if (e.shiftKey) {
    shortcuts.push("shift");
  }
  if (e.metaKey) {
    shortcuts.push("cmd");
  }
  if (e.ctrlKey) {
    shortcuts.push("ctrl");
  }
  if (
    shortcuts.length > 0 &&
    !/(control|alt|meta|shift)/.test(e.key.toLocaleLowerCase())
  ) {
    shortcuts.push(e.key.toLocaleLowerCase());
  }
  if (/escape/.test(e.key.toLocaleLowerCase())) {
    shortcuts.push("escape");
  }
  if (shortcuts.length < 1) {
    return;
  }

  // @ts-expect-error external library; not maintained by this repo
  let equal = !!data[shortcuts.join("+")];
  // @ts-expect-error external library; not maintained by this repo
  let command = equal ? data[shortcuts.join("+")] : undefined;

  Object.keys(data).forEach((item) => {
    const isequal = item.split("+").every((v) => {
      if (/ctrlcmd/.test(v)) {
        return shortcuts.includes("ctrl") || shortcuts.includes("cmd");
      }
      return shortcuts.includes(v);
    });
    if (isequal) {
      // @ts-expect-error external library; not maintained by this repo
      command = data[item];
    }
  });
  if (command && commandOrchestrator) {
    e.stopPropagation();
    e.preventDefault();
    commandOrchestrator.executeCommand(
      command,
      undefined,
      undefined,
      shortcuts,
    );
    return;
  }
}
