import type { FieldErrors, UseFormRegister } from "react-hook-form";
import {
  RecipeBookNameMaxLength,
  RecipeBookShortDescriptionMaxLength,
} from "../../../../services/recipe-store";
import style from "./recipe-book-meta-fields.module.css";

export function RecipeBookMetaFields<
  TFormModel extends RecipeBookMetaFormModel,
>({
  disabled,
  legend,
  register,
  errors,
}: {
  disabled: boolean;
  legend: string;
  register: UseFormRegister<TFormModel>;
  errors: FieldErrors<TFormModel>;
}) {
  return (
    <fieldset disabled={disabled}>
      <legend className={style.formLegend}>{legend}</legend>
      <label className={style.inputGroup}>
        <span>Name of Book</span>
        <input
          type="text"
          required
          maxLength={RecipeBookNameMaxLength}
          {...(register as unknown as UseFormRegister<RecipeBookMetaFormModel>)(
            "bookName",
            {
              required: true,
              maxLength: RecipeBookNameMaxLength,
            },
          )}
        ></input>
        {errors.bookName?.type === "required" && <span>Name is Required</span>}
        {errors.bookName?.type === "maxLength" && (
          <span>Name has a max length of {RecipeBookNameMaxLength}</span>
        )}
      </label>
      <label className={style.inputGroup}>
        <span>Book Description</span>
        <textarea
          maxLength={RecipeBookShortDescriptionMaxLength}
          {...(register as unknown as UseFormRegister<RecipeBookMetaFormModel>)(
            "bookDescription",
            {
              required: false,
              maxLength: RecipeBookShortDescriptionMaxLength,
            },
          )}
        ></textarea>
      </label>
      {errors.bookDescription?.type === "maxLength" && (
        <span>
          Description has a max length of {RecipeBookShortDescriptionMaxLength}
        </span>
      )}
    </fieldset>
  );
}

/**
 * Fields in the form
 */
export type RecipeBookMetaFormModel = {
  /**
   * name of the book; max length is 127 characters
   * @see RecipeBookNameMaxLength
   */
  bookName: string;
  /**
   * description of the book; max length is 255 characters
   * @see RecipeBookShortDescriptionMaxLength
   */
  bookDescription: string;
};
