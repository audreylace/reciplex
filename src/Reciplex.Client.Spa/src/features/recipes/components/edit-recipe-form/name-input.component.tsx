import { TextField } from "@mui/material";
import type { UseFormRegister } from "react-hook-form";
import { RecipeNameMaxLength } from "../../services/recipe-types";
import type { IFormModel } from "./form-model";

/** props for `NameInput` */
export interface INameInput {
  /** disables the field */
  disabled?: boolean;
  /** validation error. Puts input in error UI state and this message is shown. */
  errorText?: string;
  /** form hook field register method */
  register: UseFormRegister<IFormModel>;
}

/** input field for recipe name */
export function NameInput({ disabled, errorText, register }: INameInput) {
  return (
    <TextField
      disabled={disabled}
      variant="filled"
      fullWidth
      label="Title"
      error={!!errorText}
      helperText={errorText ?? "Title of the recipe"}
      {...register("name", {
        required: "Recipe must have a title",
        maxLength: {
          value: RecipeNameMaxLength,
          message: `Max length is ${RecipeNameMaxLength} characters`,
        },
      })}
    />
  );
}
