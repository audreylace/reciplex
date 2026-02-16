import { BadPathBanner } from "../../components/bad-path-banner/bad-path-banner.component";
import { FetchingRecipeBookBanner } from "../../components/fetching-recipe-book-banner/fetching-recipe-book-banner.component";
import { FetchingRecipeBookFailedBanner } from "../../components/fetching-recipe-book-failed-banner/fetching-recipe-book-failed-banner.component";
import { RecipeBookNotFoundBanner } from "../../components/recipe-book-not-found-banner/recipe-book-not-found-banner.component";
import { Field, Fieldset, Input, Label, Legend } from "@headlessui/react";
import { DangerButton } from "../../../core/components/danger-button/danger-button.component";
import formStyles from "../../../core/form-common/form-common.module.css";
import { FormButtons } from "../../../core/components/form-buttons/form-buttons.component";
import { PrimaryButton } from "../../../core/components/primary-button/primary-button.component";
import { BookIsReadonlyBanner } from "../../components/book-is-readonly-banner/book-is-readonly-banner.component";
import { OfflineBanner } from "../../components/offline-banner/offline-banner.component";
import { ActionFailedTryAgainCancel } from "../../components/action-failed-try-again-cancel/action-failed-try-again-cancel.component";
import { useDeleteRecipeBookPage } from "./useDeleteRecipeBookPage.hook";

export function DeleteRecipeBookPage() {
  const { state, bookId, errors, register, bookName, onSubmit, onCancel } =
    useDeleteRecipeBookPage();
  return (
    <main className={`${formStyles.formMain}`}>
      {state === "bad-path" && <BadPathBanner />}
      {state === "loading" && <FetchingRecipeBookBanner />}
      {state === "error" && <FetchingRecipeBookFailedBanner />}
      {state === "not-found" && <RecipeBookNotFoundBanner />}
      {state === "read-only" && <BookIsReadonlyBanner bookId={bookId ?? ""} />}
      {state === "offline" && <OfflineBanner />}
      {state === "delete-in-progress" && <p>Deleting...</p>}
      {state === "delete-failed" && (
        <ActionFailedTryAgainCancel
          message="Something went wrong while deleting."
          cancelCaption="View Recipe"
          cancelAction={onCancel}
        />
      )}
      {state === "conflict" && (
        <ActionFailedTryAgainCancel
          message="Someone else changed the recipe book."
          tryAgainCaption="Continue Delete?"
          cancelCaption="View Recipe Book"
          cancelAction={onCancel}
        />
      )}
      {state === "loaded" && (
        <form onSubmit={onSubmit}>
          <Fieldset className={formStyles.fieldSet}>
            <Legend className={formStyles.formLegend}>
              Delete Recipe Book {bookName}?
            </Legend>
            <Field className={formStyles.inputGroup}>
              <Label className={formStyles.label}>Type: `{bookName}`</Label>
              <Input
                className={formStyles.fieldControl}
                type="text"
                {...register("bookTitle", {
                  required: true,
                  validate: (value) => {
                    return bookName === value || `type ${bookName}`;
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
            <DangerButton type="submit">Delete</DangerButton>
            <PrimaryButton onClick={onCancel}>Cancel</PrimaryButton>
          </FormButtons>
        </form>
      )}
    </main>
  );
}
