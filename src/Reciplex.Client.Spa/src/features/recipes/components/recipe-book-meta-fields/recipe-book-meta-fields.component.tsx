import type { FieldErrors, UseFormRegister } from "react-hook-form";
import {
  RecipeBookNameMaxLength,
  RecipeBookShortDescriptionMaxLength,
} from "../../services/recipe-types";
import fieldStyles from "../../../core/form-common/form-common.module.css";
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
  disabled?: boolean;
  legend: string;
  register: UseFormRegister<TFormModel>;
  errors: FieldErrors<TFormModel>;
}) {
  return (
    <Fieldset className={fieldStyles.fieldSet} disabled={disabled}>
      <Legend className={fieldStyles.formLegend}>{legend}</Legend>
      <Field className={fieldStyles.inputGroup}>
        <Label className={fieldStyles.label}>Name of Book</Label>
        <Input
          className={fieldStyles.fieldControl}
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
        {errors.bookName?.type === "required" && <span>Name is Required</span>}
        {errors.bookName?.type === "maxLength" && (
          <span>Name has a max length of {RecipeBookNameMaxLength}</span>
        )}
      </Field>
      <Field className={fieldStyles.inputGroup}>
        <Label className={fieldStyles.label}>Book Description</Label>
        <Textarea
          className={fieldStyles.fieldControl}
          maxLength={RecipeBookShortDescriptionMaxLength}
          {...(register as unknown as UseFormRegister<RecipeBookMetaFormModel>)(
            "bookDescription",
            {
              required: false,
              maxLength: RecipeBookShortDescriptionMaxLength,
            },
          )}
        ></Textarea>
        {errors.bookDescription?.type === "maxLength" && (
          <span>
            Description has a max length of{" "}
            {RecipeBookShortDescriptionMaxLength}
          </span>
        )}
      </Field>
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
