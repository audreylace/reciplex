import { RecipeDetailsMenuBar } from "../recipe-details-menu-bar/recipe-details-menu-bar.component";
import { useMarkdownEditor } from "../../../core/hooks/useMarkdownEditor.hook";
import { useRecipeDetailsMenuBarCommandHandler } from "../recipe-details-menu-bar/useRecipeDetailsMenuBarCommandHandler.hook";
import Stack from "@mui/material/Stack";
import TextField from "@mui/material/TextField";

export interface RecipeDetailsEditorProps {
  value: string;
  onChange?: (e: Event) => void;
  onBlur?: (e: Event) => void;
  disabled?: boolean;
  onSizeToggle: () => void;
  isFullscreen?: boolean;
  minRows?: number;
  maxRows?: number;
}
export function RecipeDetailsEditor({
  value,
  onChange,
  onSizeToggle,
  isFullscreen,
  minRows,
  disabled,
  onBlur,
  maxRows,
}: RecipeDetailsEditorProps) {
  const { textAreaRef, onKeyDown, orchestratorRef } = useMarkdownEditor();
  const menuCommandHandler =
    useRecipeDetailsMenuBarCommandHandler(orchestratorRef);

  return (
    <Stack direction="column">
      <RecipeDetailsMenuBar
        commandHandler={menuCommandHandler}
        isFullscreen={isFullscreen ?? false}
        onSizeToggle={onSizeToggle}
      />

      <TextField
        hiddenLabel
        multiline
        fullWidth
        variant="filled"
        aria-label={"markdown content describing the recipe"}
        inputRef={textAreaRef}
        onKeyDown={onKeyDown}
        value={value}
        minRows={minRows}
        maxRows={maxRows}
        onChange={onChange}
        disabled={disabled}
        onBlur={onBlur}
      />
    </Stack>
  );
}
