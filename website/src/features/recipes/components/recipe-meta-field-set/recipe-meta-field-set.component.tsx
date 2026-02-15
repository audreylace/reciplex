import type { FieldErrors, UseFormRegister } from "react-hook-form";
import {
  RecipeNameMaxLength,
  RecipeShortDescriptionMaxLength,
} from "../../../../services/recipe-store";
import commonFormStyles from "../../../core/form-common/form-common.module.css";
import {
  Field,
  Fieldset,
  Input,
  Label,
  Legend,
  Textarea,
} from "@headlessui/react";

/**
 * Renders a form component for modifying the name and short description of a recipe.
 * Expects calling component to manage the form hook.
 * @param param0 react args
 * @returns JSX tree for rendering
 */
export function RecipeMetaFieldSet<TFormModel extends RecipeMetaFormModel>({
  disabled,
  legendText,
  register,
  errors,
}: {
  disabled: boolean;
  legendText: string;
  register: UseFormRegister<TFormModel>;
  errors: FieldErrors<TFormModel>;
}) {
  return (
    <Fieldset className={commonFormStyles.fieldSet} disabled={disabled}>
      <Legend className={commonFormStyles.formLegend}>{legendText}</Legend>
      <Field className={commonFormStyles.inputGroup}>
        <Label className={commonFormStyles.label}>Recipe Name</Label>
        <Input
          type="text"
          className={commonFormStyles.fieldControl}
          required
          maxLength={RecipeNameMaxLength}
          {...(register as unknown as UseFormRegister<RecipeMetaFormModel>)(
            "recipeName",
            {
              required: true,
              maxLength: RecipeNameMaxLength,
            },
          )}
        ></Input>
        {errors.recipeName?.type === "required" && (
          <span>Name is Required</span>
        )}
        {errors.recipeName?.type === "maxLength" && (
          <span>Name has a max length of {RecipeNameMaxLength}</span>
        )}
      </Field>
      <Field className={commonFormStyles.inputGroup}>
        <Label className={commonFormStyles.label}>
          Short Recipe Description
        </Label>
        <Textarea
          className={commonFormStyles.fieldControl}
          rows={3}
          maxLength={RecipeShortDescriptionMaxLength}
          {...(register as unknown as UseFormRegister<RecipeMetaFormModel>)(
            "recipeDescription",
            {
              required: false,
              maxLength: RecipeShortDescriptionMaxLength,
            },
          )}
        ></Textarea>
        {errors.recipeDescription?.type === "maxLength" && (
          <span>
            Short description has a max length of{" "}
            {RecipeShortDescriptionMaxLength}
          </span>
        )}
      </Field>
    </Fieldset>
  );
}

/**
 * Fields in the form
 */
export type RecipeMetaFormModel = {
  /** name of the recipe */
  recipeName: string;
  /** recipe short plain text description */
  recipeDescription: string;
};
