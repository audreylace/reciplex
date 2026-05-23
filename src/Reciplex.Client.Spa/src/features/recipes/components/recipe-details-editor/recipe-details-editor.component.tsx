import { RecipeDetailsMenuBar } from "../recipe-details-menu-bar/recipe-details-menu-bar.component";
import { useMarkdownEditor } from "../../../core/hooks/useMarkdownEditor.hook";
import { useRecipeDetailsMenuBarCommandHandler } from "../recipe-details-menu-bar/useRecipeDetailsMenuBarCommandHandler.hook";
import Stack from "@mui/material/Stack";
import TextField from "@mui/material/TextField";
import { useRef, useState } from "react";
import Dialog from "@mui/material/Dialog";
import { FullScreenEditor } from "./fullscreen-editor.component";

export interface RecipeDetailsEditorProps {
  value: string;
  onChange: React.ChangeEventHandler<
    HTMLInputElement | HTMLTextAreaElement,
    Element
  >;
  onBlur?: React.FocusEventHandler<HTMLInputElement | HTMLTextAreaElement>;
  disabled?: boolean;
  syntheticChange: (s: string) => void;
}
export function RecipeDetailsEditor({
  value,
  onChange,
  disabled,
  onBlur,
  syntheticChange,
}: RecipeDetailsEditorProps) {
  const { textAreaRef, onKeyDown, orchestratorRef } = useMarkdownEditor();
  const menuCommandHandler =
    useRecipeDetailsMenuBarCommandHandler(orchestratorRef);
  const [fullScreen, setFullScreen] = useState(false);
  const [key, setKey] = useState(0);
  const elRef = useRef<HTMLTextAreaElement>(null);

  return (
    <Stack direction="column">
      <RecipeDetailsMenuBar
        disabled={disabled}
        commandHandler={menuCommandHandler}
        isFullscreen={false}
        onSizeToggle={() => {
          setKey((k) => k + 1);
          setFullScreen(true);
        }}
      />
      <TextField
        hiddenLabel
        multiline
        fullWidth
        variant="outlined"
        aria-label={"markdown content describing the recipe"}
        inputRef={(element) => {
          textAreaRef(element);
          elRef.current = element;
        }}
        value={value}
        rows={8}
        onChange={onChange}
        disabled={disabled}
        onBlur={onBlur}
        slotProps={{
          input: {
            onKeyDown: onKeyDown,
          },
        }}
      />

      {fullScreen && (
        <Dialog open fullScreen key={key}>
          <FullScreenEditor
            disabled={disabled}
            value={value}
            onExit={(value) => {
              setFullScreen(false);
              setKey((k) => k + 1);
              syntheticChange(value);
            }}
          />
        </Dialog>
      )}
    </Stack>
  );
}
