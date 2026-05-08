import { RecipeDetailsMenuBar } from "../recipe-details-menu-bar/recipe-details-menu-bar.component";
import { useMarkdownEditor } from "../../../core/hooks/useMarkdownEditor.hook";
import { useRecipeDetailsMenuBarCommandHandler } from "../recipe-details-menu-bar/useRecipeDetailsMenuBarCommandHandler.hook";
import Stack from "@mui/material/Stack";
import TextField from "@mui/material/TextField";
import { useRef, useState } from "preact/hooks";
import Dialog from "@mui/material/Dialog";
import { FullScreenEditor } from "./fullscreen-editor.component";

export interface RecipeDetailsEditorProps {
  value: string;
  onChange: (e: Event | string) => void;
  onBlur?: (e: Event) => void;
  disabled?: boolean;
}
export function RecipeDetailsEditor({
  value,
  onChange,
  disabled,
  onBlur,
}: RecipeDetailsEditorProps) {
  const { textAreaRef, onKeyDown, orchestratorRef } = useMarkdownEditor();
  const menuCommandHandler =
    useRecipeDetailsMenuBarCommandHandler(orchestratorRef);
  const [fullScreen, setIsFullscreen] = useState(false);
  const [key, setKey] = useState(0);
  const elRef = useRef<HTMLTextAreaElement>();

  return (
    <Stack direction="column">
      <RecipeDetailsMenuBar
        disabled={disabled}
        commandHandler={menuCommandHandler}
        isFullscreen={false}
        onSizeToggle={() => {
          setKey((k) => k + 1);
          setIsFullscreen(true);
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
        onKeyDown={onKeyDown}
        value={value}
        rows={8}
        onChange={onChange}
        disabled={disabled}
        onBlur={onBlur}
      />

      {fullScreen && (
        <Dialog open fullScreen key={key}>
          <FullScreenEditor
            disabled={disabled}
            value={value}
            onExit={(value) => {
              setIsFullscreen(false);
              setKey((k) => k + 1);
              onChange(value);
            }}
          />
        </Dialog>
      )}
    </Stack>
  );
}
