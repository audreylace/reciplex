import { RecipeDetailsMenuBar } from "../recipe-details-menu-bar/recipe-details-menu-bar.component";
import { useMarkdownEditor } from "../../../core/hooks/useMarkdownEditor.hook";
import { useRecipeDetailsMenuBarCommandHandler } from "../recipe-details-menu-bar/useRecipeDetailsMenuBarCommandHandler.hook";
import Stack from "@mui/material/Stack";
import TextField from "@mui/material/TextField";
import { useEffect, useRef, useState } from "react";
import Dialog from "@mui/material/Dialog";
import { FullScreenEditor } from "./fullscreen-editor.component";
import { useSearchParams } from "react-router";

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
  const [key, setKey] = useState(0);
  const elRef = useRef<HTMLTextAreaElement>(null);
  const [searchParams, setSearchParams] = useSearchParams();
  const fullScreen = searchParams.get("fullScreen") === "yes";

  useEffect(() => {
    if (disabled && fullScreen) {
      setSearchParams({ fullScreen: "no" });
    }
  }, [disabled, fullScreen, setSearchParams]);

  return (
    <Stack direction="column">
      <RecipeDetailsMenuBar
        disabled={disabled}
        commandHandler={menuCommandHandler}
        isFullscreen={false}
        onSizeToggle={() => {
          setKey((k) => k + 1);
          setSearchParams({ fullScreen: "yes" });
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
            value={value}
            onChange={(s) => syntheticChange(s)}
            onExit={() => {
              setKey((k) => k + 1);
              setSearchParams({ fullScreen: "no" });
            }}
          />
        </Dialog>
      )}
    </Stack>
  );
}
