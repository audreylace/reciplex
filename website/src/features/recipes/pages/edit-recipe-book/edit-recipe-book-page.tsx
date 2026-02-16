import { RecipeBookNotFoundBanner } from "../../components/recipe-book-not-found-banner/recipe-book-not-found-banner.component";
import { FetchingRecipeBookBanner } from "../../components/fetching-recipe-book-banner/fetching-recipe-book-banner.component";
import { FetchingRecipeBookFailedBanner } from "../../components/fetching-recipe-book-failed-banner/fetching-recipe-book-failed-banner.component";
import { BadPathBanner } from "../../components/bad-path-banner/bad-path-banner.component";
import { RecipeBookMetaFields } from "../../components/recipe-book-meta-fields/recipe-book-meta-fields.component";
import { useEditRecipeBookPage } from "./useEditRecipeBookPage.hook";
import { BookIsReadonlyBanner } from "../../components/book-is-readonly-banner/book-is-readonly-banner.component";
import { OfflineBanner } from "../../components/offline-banner/offline-banner.component";
import { ActionFailedTryAgainCancel } from "../../components/action-failed-try-again-cancel/action-failed-try-again-cancel.component";
import { SuccessButton } from "../../../core/components/success-button/success-button.component";
import { DangerButton } from "../../../core/components/danger-button/danger-button.component";
import { FormButtons } from "../../../core/components/form-buttons/form-buttons.component";
import formStyles from "../../../core/form-common/form-common.module.css";

/**
 * Entry point for editing a recipe book
 */
export function EditRecipeBookPage() {
  const { onSubmit, bookName, state, bookId, onCancel, register, errors } =
    useEditRecipeBookPage();
  return (
    <main className={formStyles.formMain}>
      {state === "bad-path" && <BadPathBanner />}
      {state === "loading" && <FetchingRecipeBookBanner />}
      {state === "error" && <FetchingRecipeBookFailedBanner />}
      {state === "not-found" && <RecipeBookNotFoundBanner />}
      {state === "read-only" && <BookIsReadonlyBanner bookId={bookId ?? ""} />}
      {state === "offline" && <OfflineBanner />}
      {state === "save-in-progress" && <p>Saving...</p>}
      {state === "save-failed" && (
        <ActionFailedTryAgainCancel
          message="Something went wrong while saving."
          cancelCaption="View Recipe"
          cancelAction={onCancel}
        />
      )}
      {state === "conflict" && (
        <ActionFailedTryAgainCancel
          message="Someone else changed the recipe book."
          tryAgainCaption="Reload and try again?"
          cancelCaption="View Recipe Book"
          cancelAction={onCancel}
        />
      )}
      {state === "loaded" && (
        <form onSubmit={onSubmit}>
          <RecipeBookMetaFields
            register={register}
            legend={`Editing Recipe Book ${bookName}`}
            errors={errors}
          />
          <FormButtons>
            <SuccessButton type="submit">Save</SuccessButton>
            <DangerButton onClick={onCancel}>Cancel</DangerButton>
          </FormButtons>
        </form>
      )}
    </main>
  );
}
