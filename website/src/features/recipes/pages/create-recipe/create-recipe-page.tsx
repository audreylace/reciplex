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
import { PrimaryButton } from "../../../core/components/primary-button/primary-button.component";
import { useCreateRecipePage } from "./useCreateRecipePage.hook";

/**
 * Entry point for create recipe page component
 * @param param0 react props
 * @returns jsx tree for rendering by react
 */
export function CreateRecipePage() {
  const {
    bookName,
    errors,
    register,
    state,
    bookId,
    onSubmit,
    cancelAction,
    reloadAction,
  } = useCreateRecipePage();

  return (
    <main className="pageMain">
      {state === "bad-path" && <BadPathBanner />}
      {state === "loading" && <FetchingRecipeBookBanner />}
      {state === "not-found" && <RecipeBookNotFoundBanner />}
      {state === "offline" && <OfflineBanner />}
      {state === "read-only" && <BookIsReadonlyBanner bookId={bookId ?? ""} />}
      {state === "error" && <FetchingRecipeBookFailedBanner />}
      {state === "creating" && (
        <>
          <p>Creating recipe</p>
        </>
      )}
      {state === "create-failed" && (
        <>
          <p>Something went wrong while creating recipe...</p>
          <FormButtons>
            <SuccessButton onClick={reloadAction}>
              Reload and try again?
            </SuccessButton>
            <PrimaryButton onClick={cancelAction}>Return to Book</PrimaryButton>
          </FormButtons>
        </>
      )}
      {state === "loaded" && (
        <>
          <form onSubmit={onSubmit}>
            <RecipeMetaFieldSet
              disabled={false}
              register={register}
              legendText={`Add recipe to ${bookName}`}
              errors={errors}
            />
            <FormButtons>
              <SuccessButton type="submit">Create Recipe</SuccessButton>
              <DangerButton onClick={cancelAction}>Cancel</DangerButton>
            </FormButtons>
          </form>
        </>
      )}
    </main>
  );
}
