import { type Control, useController } from "react-hook-form";
import { RecipeDetailsMaxLength } from "../../services/recipe-types";
import { RecipeDetailsEditor } from "../recipe-details-editor/recipe-details-editor.component";
import type { IFormModel } from "./form-model";

/** props for `DetailsInput` */
export interface IDetailsInput {
  /** disables the field */
  disabled?: boolean;
  /** form control */
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  control: Control<IFormModel, any, IFormModel>;
}

/** input field for recipe details */
export function DetailsInput({ disabled, control }: IDetailsInput) {
  const {
    field: { onChange, onBlur, value },
  } = useController({
    name: "details",
    control,
    rules: {
      maxLength: {
        value: RecipeDetailsMaxLength,
        message: `A recipe can be at most ${RecipeDetailsMaxLength} characters`,
      },
    },
  });

  return (
    <RecipeDetailsEditor
      value={value}
      onChange={onChange}
      onBlur={onBlur}
      minRows={8}
      maxRows={20}
      disabled={disabled}
    />
  );
}
