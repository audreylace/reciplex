import {
  bold as mdCommandBold,
  italic as mdCommandItalic,
  code as mdCommandCode,
  TextAreaCommandOrchestrator,
  heading1,
  heading2,
  heading3,
  heading4,
  heading5,
  heading6,
  hr as mdCommandHr,
  quote as mdBlockQuote,
  unorderedListCommand,
  orderedListCommand,
  link as mdHyperlink,
} from "@uiw/react-md-editor/nohighlight";
import type { RefObject } from "preact";
import { useCallback } from "preact/hooks";

/** support commands */
export type MenuBarCommands =
  | "bold"
  | "italic"
  | "h1"
  | "h2"
  | "h3"
  | "h4"
  | "h5"
  | "h6"
  | "hr"
  | "inline-quote"
  | "block-quote"
  | "bullet-list"
  | "number-list"
  | "hyperlink"
  | "ingredient-reference"
  | "recipe-section-reference"
  | "recipe-link"
  | "recipe-tool-reference";

/**
 * Hook that creates the callback tha handles menu button commands
 * @param orchestratorRef reference to the MD text area controller
 * @returns callback that should be passed to the menu bar component
 */
export function useMenuBarCommandHandler(
  orchestratorRef: RefObject<TextAreaCommandOrchestrator | null>,
) {
  return useCallback((command: MenuBarCommands) => {
    if (orchestratorRef.current) {
      handleCommand(command, orchestratorRef.current);
    }
  }, []);
}

/**
 * command handling logic
 * @param command the command to handle
 * @param orchestrator the md controller
 */
function handleCommand(
  command: MenuBarCommands,
  orchestrator: TextAreaCommandOrchestrator,
) {
  switch (command) {
    case "bold":
      orchestrator.executeCommand(mdCommandBold);
      break;
    case "italic":
      orchestrator.executeCommand(mdCommandItalic);
      break;
    case "h1":
      orchestrator.executeCommand(heading1);
      break;
    case "h2":
      orchestrator.executeCommand(heading2);
      break;
    case "h3":
      orchestrator.executeCommand(heading3);
      break;
    case "h4":
      orchestrator.executeCommand(heading4);
      break;
    case "h5":
      orchestrator.executeCommand(heading5);
      break;
    case "h6":
      orchestrator.executeCommand(heading6);
      break;
    case "hr":
      orchestrator.executeCommand(mdCommandHr);
      break;
    case "inline-quote":
      orchestrator.executeCommand(mdCommandCode);
      break;
    case "block-quote":
      orchestrator.executeCommand(mdBlockQuote);
      break;
    case "bullet-list":
      orchestrator.executeCommand(unorderedListCommand);
      break;
    case "number-list":
      orchestrator.executeCommand(orderedListCommand);
      break;
    case "hyperlink":
      orchestrator.executeCommand(mdHyperlink);
      break;
    case "ingredient-reference":
      textSwap(
        orchestrator,
        "`{{@ingredient ###}}`",
        middleSwap("`{{@ingredient ", "}}`"),
      );
      break;
    case "recipe-section-reference":
      textSwap(
        orchestrator,
        "`{{@section ###}}`",
        middleSwap("`{{@section ", "}}`"),
      );
      break;
    case "recipe-tool-reference":
      textSwap(orchestrator, "`{{@tool ###}}`", middleSwap("`{{@tool ", "}}`"));
      break;
    case "recipe-link":
      textSwap(orchestrator, "()[recipe://]", middleSwap("(", ")[recipe://]"));
      break;
    default:
      ((_: never) => {
        /* missing branch to handle `command` if there is an assignment error */
      })(command);
  }
}

/**
 * helper for building a command handler
 * @param orchestrator md controller
 * @param placeholder placeholder text if user has not selected anything
 * @param swap swap method if user has selected something
 */
function textSwap(
  orchestrator: TextAreaCommandOrchestrator,
  placeholder: string,
  swap: (value: string) => string,
) {
  const state = orchestrator.getState();
  if (!state) {
    return;
  }

  if (!state.selectedText) {
    orchestrator.textApi.replaceSelection(placeholder);
    return;
  }

  orchestrator.textApi.replaceSelection(swap(state.selectedText));
}

/**
 * helper for building a swap function where the selected text has a value appended before and after
 * @param start value to append before selection
 * @param end value to append after selection
 * @returns function to perform the swap
 */
function middleSwap(start: string, end: string): (value: string) => string {
  return (s) => `${start}${s}${end}`;
}
