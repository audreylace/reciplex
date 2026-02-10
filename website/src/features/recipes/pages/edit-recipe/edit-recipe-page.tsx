import { type RecipeMetaFormModel } from "../../components/recipe-meta-fields/recipe-meta-fields.component";
import { FetchingRecipeBanner } from "../../components/fetching-recipe-banner/fetching-recipe-banner.component";
import { RecipeNotFoundBanner } from "../../components/recipe-not-found-banner/recipe-not-found-banner.component";
import { FetchingRecipeFailedBanner } from "../../components/fetching-recipe-failed-banner/fetching-recipe-failed-banner.component";
import {
  EditRecipePageLoadingState,
  useGetInitialDataForRecipeEdit,
} from "./hooks/useGetInitialDataForRecipeEdit.hook";
import { BadPathBanner } from "../../components/bad-path-banner/bad-path-banner.component";
import { RecipeIsReadonlyBanner } from "../../components/recipe-is-readonly-banner/RecipeIsReadonlyBanner.component";
import { EditRecipeControls } from "./components/edit-recipe-controls/edit-recipe-controls.component";
import styles from "./edit-recipe-page.module.css";

export function EditRecipePage({}: {}) {
  const recipeData = useGetInitialDataForRecipeEdit();

  return (
    <main className={styles.main}>
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
      {recipeData.tag === EditRecipePageLoadingState.offline && <p>Offline</p>}
      {(recipeData.tag === EditRecipePageLoadingState.conflict ||
        recipeData.tag === EditRecipePageLoadingState.loaded) && (
        <EditRecipeControls
          key={recipeData.recipe.id}
          recipe={recipeData.recipe}
          book={recipeData.book}
          conflicted={recipeData.tag === EditRecipePageLoadingState.conflict}
        />
      )}
    </main>
  );
}
