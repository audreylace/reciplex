import type { FieldErrors, UseFormRegister } from "react-hook-form";
import {
  RecipeBookNameMaxLength,
  RecipeBookShortDescriptionMaxLength,
} from "../../../../services/recipe-store";
import style from "./recipe-book-meta-fields.module.css";
import {
  Field,
  Fieldset,
  Input,
  Label,
  Legend,
  Textarea,
} from "@headlessui/react";

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
    <Fieldset disabled={disabled}>
      <Legend className={style.formLegend}>
        <h2>{legend}</h2>
      </Legend>
      <Field className={style.inputGroup}>
        <Label>Name of Book</Label>
        <Input
          className={style.fieldControl}
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
        ></Input>
      </Field>
      {errors.bookName?.type === "required" && <span>Name is Required</span>}
      {errors.bookName?.type === "maxLength" && (
        <span>Name has a max length of {RecipeBookNameMaxLength}</span>
      )}
      <Field className={style.inputGroup}>
        <Label>Book Description</Label>
        <Textarea
          className={style.fieldControl}
          maxLength={RecipeBookShortDescriptionMaxLength}
          {...(register as unknown as UseFormRegister<RecipeBookMetaFormModel>)(
            "bookDescription",
            {
              required: false,
              maxLength: RecipeBookShortDescriptionMaxLength,
            },
          )}
        ></Textarea>
      </Field>
      {errors.bookDescription?.type === "maxLength" && (
        <span>
          Description has a max length of {RecipeBookShortDescriptionMaxLength}
        </span>
      )}
    </Fieldset>
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
