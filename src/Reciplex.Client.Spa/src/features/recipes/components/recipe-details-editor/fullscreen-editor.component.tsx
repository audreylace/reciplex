import { RecipeDetailsMenuBar } from "../recipe-details-menu-bar/recipe-details-menu-bar.component";
import { useMarkdownEditor } from "../../../core/hooks/useMarkdownEditor.hook";
import { useRecipeDetailsMenuBarCommandHandler } from "../recipe-details-menu-bar/useRecipeDetailsMenuBarCommandHandler.hook";
import Stack from "@mui/material/Stack";
import cssStyles from "./recipe-details-editor.module.css";
import { useEffect, useRef, useState } from "preact/hooks";
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
  const elRef = useRef<HTMLTextAreaElement>();

  useEffect(() => {
    if (disabled) {
      onExit(value);
    }
  }, [disabled, onExit, value]);

  return (
    <Stack direction="column" height={"100%"}>
      <RecipeDetailsMenuBar
        commandHandler={menuCommandHandler}
        isFullscreen={true}
        onSizeToggle={() => onExit(value)}
      />
      <InputBase
        inputRef={(element) => {
          textAreaRef(element);
          elRef.current = element;
        }}
        sx={{
          flex: "1 1 auto",
          p: "5px",
        }}
        onChange={() => setValue(elRef.current?.value ?? "")}
        value={value}
        onKeyDown={onKeyDown}
        // @ts-expect-error bad typing does not pickup `textarea` as an element. Instead thinks it is a string
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
