import type { FieldErrors, UseFormRegister } from "react-hook-form";
import {
  RecipeNameMaxLength,
  RecipeShortDescriptionMaxLength,
} from "../../../../services/recipe-store";
import style from "./recipe-meta-fields.module.css";

/**
 * Renders a form component for modifying the name and short description of a recipe.
 * Expects calling component to manage the form hook.
 * @param param0 react args
 * @returns JSX tree for rendering
 */
export function RecipeMetaFields<TFormModel extends RecipeMetaFormModel>({
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
    <fieldset disabled={disabled}>
      <legend>{legendText}</legend>
      <label className={style.inputGroup}>
        <span>Recipe Name</span>
        <input
          type="text"
          required
          maxLength={RecipeNameMaxLength}
          {...(register as unknown as UseFormRegister<RecipeMetaFormModel>)(
            "recipeName",
            {
              required: true,
              maxLength: RecipeNameMaxLength,
            },
          )}
        ></input>
        {errors.recipeName?.type === "required" && (
          <span>Name is Required</span>
        )}
        {errors.recipeName?.type === "maxLength" && (
          <span>Name has a max length of {RecipeNameMaxLength}</span>
        )}
      </label>
      <label className={style.inputGroup}>
        <span>Short Recipe Description</span>
        <textarea
          maxLength={RecipeShortDescriptionMaxLength}
          {...(register as unknown as UseFormRegister<RecipeMetaFormModel>)(
            "recipeDescription",
            {
              required: false,
              maxLength: RecipeShortDescriptionMaxLength,
            },
          )}
        ></textarea>
      </label>
      {errors.recipeDescription?.type === "maxLength" && (
        <span>
          Short description has a max length of{" "}
          {RecipeShortDescriptionMaxLength}
        </span>
      )}
    </fieldset>
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
