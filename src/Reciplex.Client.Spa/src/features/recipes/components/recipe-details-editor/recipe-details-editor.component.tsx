import { RecipeDetailsMenuBar } from "../recipe-details-menu-bar/recipe-details-menu-bar.component";
import { useMarkdownEditor } from "../../../core/hooks/useMarkdownEditor.hook";
import { useRecipeDetailsMenuBarCommandHandler } from "../recipe-details-menu-bar/useRecipeDetailsMenuBarCommandHandler.hook";
import Stack from "@mui/material/Stack";
import TextField from "@mui/material/TextField";
import cssStyles from "./recipe-details-editor.module.css";
import FilledInput from "@mui/material/FilledInput";
import { useEffect, useRef, useState } from "preact/hooks";
import Dialog from "@mui/material/Dialog";

export interface RecipeDetailsEditorProps {
  value: string;
  onChange: (e: Event | string) => void;
  onBlur?: (e: Event) => void;
  disabled?: boolean;
  minRows?: number;
  maxRows?: number;
}
export function RecipeDetailsEditor({
  value,
  onChange,
  minRows,
  disabled,
  onBlur,
  maxRows,
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
        variant="filled"
        aria-label={"markdown content describing the recipe"}
        inputRef={(element) => {
          textAreaRef(element);
          elRef.current = element;
        }}
        onKeyDown={onKeyDown}
        value={value}
        minRows={minRows}
        maxRows={maxRows}
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

function FullScreenEditor({
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
      <FilledInput
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
