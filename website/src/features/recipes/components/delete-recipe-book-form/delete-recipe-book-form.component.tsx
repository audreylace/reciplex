import { Fieldset, Legend, Field, Label, Input } from "@headlessui/react";
import { useForm } from "react-hook-form";
import { DangerButton } from "../../../core/components/danger-button/danger-button.component";
import { FormButtons } from "../../../core/components/form-buttons/form-buttons.component";
import { PrimaryButton } from "../../../core/components/primary-button/primary-button.component";
import type { IRecipeBookModel } from "../../services/recipe-types";
import { useState } from "preact/hooks";
import { useDeleteRecipeBookMutation } from "../../hooks/useDeleteRecipeBookMutation.hook";

import formStyles from "../../../core/form-common/form-common.module.css";
import { ActionFailedTryAgainCancel } from "../action-failed-try-again-cancel/action-failed-try-again-cancel.component";
import { BookIsReadonlyBanner } from "../book-is-readonly-banner/book-is-readonly-banner.component";

/**
 * Form for deleting a recipe book
 *
 * @todo support controlling the reload action from
 * the higher component. Right now the reload action
 * just reloads the page which could be a problem
 * if this form is ever embedded in a submenu.
 */
export function DeleteRecipeBookForm({
  data,
  onCancel,
  onDeleted,
}: DeleteRecipeBookFormProps) {
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<FormFields>();

  const deleteRecipeBookMutation = useDeleteRecipeBookMutation();
  const [versionTag, setVersionTag] = useState<string | null>(null);
  const onSubmit = handleSubmit(async (formFields) => {
    if (formFields.bookTitle != data.name || !versionTag) {
      return;
    }
    await deleteRecipeBookMutation.mutateAsync({
      bookId: data.id,
      versionTag: versionTag,
    });
    onDeleted();
  });

  if (data.versionTag && !versionTag) {
    setVersionTag(data.versionTag);
    return null;
  }

  if (data.versionTag !== versionTag) {
    return (
      <ActionFailedTryAgainCancel
        message="Someone else changed the recipe book."
        tryAgainCaption="Continue Delete?"
        cancelCaption="View Recipe Book"
        cancelAction={onCancel}
      />
    );
  }

  const cancelProxy = () => {
    onCancel({
      bookId: data.id,
      name: data.name,
      shortDescription: data.shortDescription,
    });
  };

  return (
    <>
      {!data.mayDelete && <BookIsReadonlyBanner bookId={data.id} />}
      {deleteRecipeBookMutation.status === "idle" && data.mayDelete && (
        <form onSubmit={onSubmit}>
          <Fieldset className={formStyles.fieldSet}>
            <Legend className={formStyles.formLegend}>
              Delete Recipe Book {data.name}?
            </Legend>
            <Field className={formStyles.inputGroup}>
              <Label className={formStyles.label}>Type: `{data.name}`</Label>
              <Input
                className={formStyles.fieldControl}
                type="text"
                {...register("bookTitle", {
                  required: true,
                  validate: (value) => {
                    return data.name === value || `type '${data.name}'`;
                  },
                })}
              />
            </Field>
            {errors.bookTitle && <span>{errors.bookTitle.message}</span>}
            {errors.bookTitle?.type === "required" && (
              <span>Field is Required</span>
            )}
          </Fieldset>
          <FormButtons>
            <DangerButton type="submit">
              <i className="bi bi-trash"></i> Delete
            </DangerButton>
            <PrimaryButton onClick={cancelProxy}>Cancel</PrimaryButton>
          </FormButtons>
        </form>
      )}
      {deleteRecipeBookMutation.isPending && <p>Deleting...</p>}
      {deleteRecipeBookMutation.isError && (
        <ActionFailedTryAgainCancel
          message="Something went wrong while deleting."
          cancelCaption="View Recipe Book"
          cancelAction={cancelProxy}
        />
      )}
    </>
  );
}

/** props for the `DeleteRecipeBookForm` */
export interface DeleteRecipeBookFormProps {
  /** the recipe book to delete */
  data: IRecipeBookModel;
  /** invoked on cancel */
  onCancel: (info?: {
    name: string;
    shortDescription: string;
    bookId: string;
  }) => void;
  /** invoked after delete */
  onDeleted: () => void;
}

/** form fields for this component */
type FormFields = {
  /** title of the book */
  bookTitle: string;
};
