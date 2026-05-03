import { TextField } from "@mui/material";
import { type Control, useController } from "react-hook-form";
import { RecipeShortDescriptionMaxLength } from "../../services/recipe-types";
import type { IFormModel } from "./form-model";

/** props for `ShortDescriptionInput` */
export interface IShortDescriptionInput {
  /** disables the field */
  disabled?: boolean;
  /** validation error. Puts input in error UI state and this message is shown. */
  errorText?: string;
  /** form control */
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  control: Control<IFormModel, any, IFormModel>;
}

/** input for the short recipe description */
export function ShortDescriptionInput({
  disabled,
  errorText,
  control,
}: IShortDescriptionInput) {
  const {
    field: { onChange, onBlur, value, ref },
  } = useController({
    name: "shortDescription",
    control,
    rules: {
      maxLength: {
        value: RecipeShortDescriptionMaxLength,
        message: `Max length for short description is ${RecipeShortDescriptionMaxLength} characters`,
      },
    },
  });

  return (
    <TextField
      disabled={disabled}
      fullWidth
      multiline
      minRows={3}
      label="Short Description"
      error={!!errorText}
      helperText={errorText ?? "Concise description of the recipe"}
      value={value}
      ref={ref}
      onBlur={onBlur}
      onChange={onChange}
    />
  );
}
