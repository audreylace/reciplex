import { RecipeDetailsMenuBar } from "../recipe-details-menu-bar/recipe-details-menu-bar.component";
import { useMarkdownEditor } from "../../../core/hooks/useMarkdownEditor.hook";
import { useRecipeDetailsMenuBarCommandHandler } from "../recipe-details-menu-bar/useRecipeDetailsMenuBarCommandHandler.hook";
import Stack from "@mui/material/Stack";
import cssStyles from "./recipe-details-editor.module.css";
import { useEffect, useState } from "react";
import InputBase from "@mui/material/InputBase";

export function FullScreenEditor({
  onExit,
  value: propValue,
  disabled,
}: {
  onExit: (s: string) => void;
  value: string;
  disabled?: boolean;
}) {
  const { textAreaRef, onKeyDown, orchestratorRef } = useMarkdownEditor();
  const menuCommandHandler =
    useRecipeDetailsMenuBarCommandHandler(orchestratorRef);
  const [value, setValue] = useState(propValue);

  useEffect(() => {
    if (disabled) {
      onExit(value);
    }
  }, [disabled, onExit, value]);

  return (
    <Stack direction="column" sx={{ height: "100%" }}>
      <RecipeDetailsMenuBar
        commandHandler={menuCommandHandler}
        isFullscreen={true}
        onSizeToggle={() => onExit(value)}
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
          setValue(e.target.value);
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
