import { RecipeDetailsMenuBar } from "../recipe-details-menu-bar/recipe-details-menu-bar.component";
import { useMarkdownEditor } from "../../../core/hooks/useMarkdownEditor.hook";
import { useRecipeDetailsMenuBarCommandHandler } from "../recipe-details-menu-bar/useRecipeDetailsMenuBarCommandHandler.hook";
import Stack from "@mui/material/Stack";
import cssStyles from "./recipe-details-editor.module.css";
import InputBase from "@mui/material/InputBase";

export function FullScreenEditor({
  onExit,
  value,
  onChange,
}: {
  onExit: () => void;
  value: string;
  onChange: (s: string) => void;
}) {
  const { textAreaRef, onKeyDown, orchestratorRef } = useMarkdownEditor();
  const menuCommandHandler =
    useRecipeDetailsMenuBarCommandHandler(orchestratorRef);

  return (
    <Stack direction="column" sx={{ height: "100%" }}>
      <RecipeDetailsMenuBar
        commandHandler={menuCommandHandler}
        isFullscreen={true}
        onSizeToggle={() => onExit()}
      />
      <InputBase
        inputRef={textAreaRef}
        sx={{
          flex: "1 1 auto",
          p: "5px",
        }}
        value={value}
        onKeyDown={onKeyDown}
        onChange={(e) => {
          onChange(e.target.value);
        }}
        inputComponent="textarea"
        slotProps={{
          input: {
            className: cssStyles.fullscreenTextBox,
          },
        }}
      />
    </Stack>
  );
}
