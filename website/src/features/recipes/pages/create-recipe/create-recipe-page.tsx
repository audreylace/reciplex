import { BookIsReadonlyBanner } from "../../components/book-is-readonly-banner/book-is-readonly-banner.component";
import { RecipeBookNotFoundBanner } from "../../components/recipe-book-not-found-banner/recipe-book-not-found-banner.component";
import { FetchingRecipeBookBanner } from "../../components/fetching-recipe-book-banner/fetching-recipe-book-banner.component";
import { FetchingRecipeBookFailedBanner } from "../../components/fetching-recipe-book-failed-banner/fetching-recipe-book-failed-banner.component";
import { BadPathBanner } from "../../components/bad-path-banner/bad-path-banner.component";
import { DangerButton } from "../../../core/components/danger-button/danger-button.component";
import { FormButtons } from "../../../core/components/form-buttons/form-buttons.component";
import { SuccessButton } from "../../../core/components/success-button/success-button.component";
import { RecipeMetaFieldSet } from "../../components/recipe-meta-field-set/recipe-meta-field-set.component";
import { OfflineBanner } from "../../components/offline-banner/offline-banner.component";
import { useCreateRecipePage } from "./useCreateRecipePage.hook";
import { ActionFailedTryAgainCancel } from "../../components/action-failed-try-again-cancel/action-failed-try-again-cancel.component";
import formStyles from "../../../core/form-common/form-common.module.css";
import { AssertString } from "../../../sentinel/stringUtilities";
import { BookInformationBannerWithQuery } from "../../components/book-information-banner/book-information-banner-with-query";

/**
 * Entry point for create recipe page component
 */
export function CreateRecipePage() {
  const { bookName, errors, register, state, bookId, onSubmit, cancelAction } =
    useCreateRecipePage();

  return (
    <main className={`${formStyles.formMain}`}>
      {state === "bad-path" && <BadPathBanner />}
      {state === "loading" && <FetchingRecipeBookBanner />}
      {state === "not-found" && <RecipeBookNotFoundBanner />}
      {state === "offline" && <OfflineBanner />}
      {state === "read-only" && (
        <BookIsReadonlyBanner bookId={AssertString(bookId)} />
      )}
      {state === "error" && <FetchingRecipeBookFailedBanner />}
      {state === "creating" && (
        <>
          <p>Creating recipe</p>
        </>
      )}
      {state === "create-failed" && (
        <ActionFailedTryAgainCancel
          message="Something went wrong while creating recipe..."
          cancelCaption="Return to Book"
          cancelAction={cancelAction}
        />
      )}
      {state === "loaded" && (
        <>
          <BookInformationBannerWithQuery bookId={bookId} />
          <form onSubmit={onSubmit}>
            <RecipeMetaFieldSet
              disabled={false}
              register={register}
              legendText={`Add recipe to ${bookName}`}
              errors={errors}
            />
            <FormButtons>
              <SuccessButton type="submit">
                <i className="bi bi-plus-circle-dotted"></i> Create Recipe
              </SuccessButton>
              <DangerButton onClick={cancelAction}>Cancel</DangerButton>
            </FormButtons>
          </form>
        </>
      )}
    </main>
  );
}
