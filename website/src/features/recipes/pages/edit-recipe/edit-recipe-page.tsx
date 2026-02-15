import { FetchingRecipeBanner } from "../../components/fetching-recipe-banner/fetching-recipe-banner.component";
import { RecipeNotFoundBanner } from "../../components/recipe-not-found-banner/recipe-not-found-banner.component";
import { FetchingRecipeFailedBanner } from "../../components/fetching-recipe-failed-banner/fetching-recipe-failed-banner.component";
import {
  EditRecipePageLoadingState,
  useGetInitialDataForRecipeEdit,
} from "./useGetInitialDataForRecipeEdit.hook";
import { BadPathBanner } from "../../components/bad-path-banner/bad-path-banner.component";
import { RecipeIsReadonlyBanner } from "../../components/recipe-is-readonly-banner/RecipeIsReadonlyBanner.component";
import { EditRecipeForm } from "./edit-recipe-form.component";
import { OfflineBanner } from "../../components/offline-banner/offline-banner.component";

export function EditRecipePage() {
  const recipeData = useGetInitialDataForRecipeEdit();

  return (
    <main className="pageMain">
      {recipeData.tag === EditRecipePageLoadingState.badRoute && (
        <BadPathBanner />
      )}
      {recipeData.tag === EditRecipePageLoadingState.notFound && (
        <RecipeNotFoundBanner />
      )}
      {recipeData.tag === EditRecipePageLoadingState.loadingFailed && (
        <FetchingRecipeFailedBanner />
      )}
      {recipeData.tag === EditRecipePageLoadingState.loading && (
        <FetchingRecipeBanner />
      )}
      {recipeData.tag === EditRecipePageLoadingState.readonly && (
        <RecipeIsReadonlyBanner recipeId={recipeData.recipe.id} />
      )}
      {recipeData.tag === EditRecipePageLoadingState.offline && (
        <OfflineBanner />
      )}
      {(recipeData.tag === EditRecipePageLoadingState.conflict ||
        recipeData.tag === EditRecipePageLoadingState.loaded) && (
        <EditRecipeForm
          key={recipeData.recipe.id}
          recipe={recipeData.recipe}
          book={recipeData.book}
          conflicted={recipeData.tag === EditRecipePageLoadingState.conflict}
        />
      )}
    </main>
  );
}
