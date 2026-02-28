import { Dialog, DialogPanel } from "@headlessui/react";
import { useState, useCallback, useEffect } from "preact/hooks";
import { useFullscreenState } from "../../../core/hooks/useFullscreenState.hook";
import styles from "./recipe-details-editor-fullscreen.module.css";
import { RecipeDetailsEditor } from "../recipe-details-editor/recipe-details-editor.component";

/** JSX properties for `RecipeDetailsEditorFullscreen` */
export interface RecipeDetailsEditorFullscreenProps {
  /** the max length of the details editor */
  maxLength?: number;
  /** triggered on close passing back the final value the user typed in */
  onClose: (newValue: string) => void;
  /** the initial value */
  value: string;
}

/** Fullscreen recipe details editor */
export function RecipeDetailsEditorFullscreen({
  maxLength,
  onClose,
  value,
}: RecipeDetailsEditorFullscreenProps) {
  const [textValue, setTextValue] = useState(value);
  const [open, setIsOpen] = useState(true);
  const [enteredFullscreen, setEnteredFullscreen] = useState(false);

  const {
    fullscreen: fullscreenState,
    enterFullscreen,
    exitFullscreen,
  } = useFullscreenState();

  // callback that runs the shutdown of the component
  const triggerClose = useCallback(() => {
    setIsOpen((open) => {
      if (!open) {
        return false;
      }
      setTextValue((s) => {
        onClose(s);
        exitFullscreen();
        return s;
      });
      return false;
    });
  }, [exitFullscreen, onClose]);

  useEffect(() => {
    if (!enteredFullscreen && open) {
      enterFullscreen();
    }
  }, [enterFullscreen, enteredFullscreen, open]);

  if (fullscreenState === "active") {
    setEnteredFullscreen(true);
  }

  // trigger close when user exists fullscreen mode
  useEffect(() => {
    if (fullscreenState === "inactive" && enteredFullscreen) {
      triggerClose();
    }
  }, [fullscreenState, triggerClose, enteredFullscreen]);

  return (
    <Dialog open={open} onClose={triggerClose}>
      <DialogPanel className={styles.dialogPanel}>
        <RecipeDetailsEditor
          value={textValue}
          isFullscreen
          onChange={(s) => setTextValue(s)}
          onSizeToggle={triggerClose}
          className={`${styles.detailsEditorFullScreen}`}
          textAreaClassName={styles.textAreaFullScreen}
          maxLength={maxLength}
        />
      </DialogPanel>
    </Dialog>
  );
}
