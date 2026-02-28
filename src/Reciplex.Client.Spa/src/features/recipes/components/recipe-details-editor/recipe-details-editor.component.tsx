import { Textarea } from "@headlessui/react";
import type { RefCallBack } from "react-hook-form";
import { RecipeDetailsMenuBar } from "../recipe-details-menu-bar/recipe-details-menu-bar.component";
import { useMarkdownEditor } from "../../../core/hooks/useMarkdownEditor.hook";
import { useRecipeDetailsMenuBarCommandHandler } from "../recipe-details-menu-bar/useRecipeDetailsMenuBarCommandHandler.hook";
import styles from "./recipe-details-editor.module.css";

export interface RecipeDetailsEditorProps {
  maxLength?: number;
  value: string;
  onChange: (s: string) => void;
  onSizeToggle: () => void;
  className?: string;
  textAreaClassName?: string;
  isFullscreen?: boolean;
  rows?: number;
}
export function RecipeDetailsEditor({
  value,
  maxLength,
  onChange,
  onSizeToggle,
  className,
  textAreaClassName,
  isFullscreen,
  rows,
}: RecipeDetailsEditorProps) {
  const { textAreaRef, onKeyDown, orchestratorRef } = useMarkdownEditor();
  const menuCommandHandler =
    useRecipeDetailsMenuBarCommandHandler(orchestratorRef);

  return (
    <div className={`${styles.recipeDetailsEditor} ${className ?? ""}`}>
      <RecipeDetailsMenuBar
        commandHandler={menuCommandHandler}
        isFullscreen={isFullscreen ?? false}
        onSizeToggle={onSizeToggle}
      />
      <Textarea
        aria-label={"markdown content describing the recipe"}
        className={`${styles.textArea} ${textAreaClassName ?? ""}`}
        ref={textAreaRef as unknown as RefCallBack}
        onKeyDown={onKeyDown}
        maxlength={maxLength}
        value={value}
        rows={rows}
        onChange={(e) =>
          onChange(
            (e.target as unknown as HTMLTextAreaElement | undefined)?.value ??
              "",
          )
        }
      />
    </div>
  );
}
