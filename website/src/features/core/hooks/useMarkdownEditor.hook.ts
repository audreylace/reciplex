import {
  TextAreaCommandOrchestrator,
  handleKeyDown,
  shortcuts,
  getCommands,
} from "@uiw/react-md-editor";
import { useRef, useCallback } from "preact/hooks";

/**
 * Hook for a markdown text editor
 */
export function useMarkdownEditor(): UseMarkdownEditorReturn {
  // Markdown editor logic
  const orchestratorRef = useRef<TextAreaCommandOrchestrator | null>(null);

  // Markdown key event handler. Supposed to be bound to textarea
  const onKeyDown = (e: KeyboardEvent) => {
    handleKeyDown(e, 2, false);
    if (orchestratorRef.current) {
      shortcuts(e, getCommands(), orchestratorRef.current);
    }
  };

  // ref function to bind the text area element to the orchestrator
  const textAreaRef = useCallback((element: HTMLTextAreaElement | null) => {
    if (element) {
      if (!orchestratorRef.current) {
        orchestratorRef.current = new TextAreaCommandOrchestrator(element);
      } else if (element !== orchestratorRef.current.textArea) {
        orchestratorRef.current.textArea = element;
      }
    } else {
      orchestratorRef.current = null;
    }
  }, []);

  const getValue = useCallback(() => {
    return orchestratorRef.current?.textArea.value ?? "";
  }, []);

  return {
    textAreaRef,
    onKeyDown,
    orchestratorRef,
    getValue,
  };
}
export interface UseMarkdownEditorReturn {
  /** ref to bind the text area */
  textAreaRef: (element: HTMLTextAreaElement | null) => void;
  /** callback invoked on each key press inside of the text area element */
  onKeyDown: (e: KeyboardEvent) => void;
  /** access to the TextAreaCommandOrchestrator */
  orchestratorRef: React.RefObject<TextAreaCommandOrchestrator | null>;
  /**
   * Use to access the current text area value outside of a render.
   */
  getValue: () => string;
}
