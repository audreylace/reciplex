import { useParams } from "react-router";
import { useGetRecipeByIdQuery } from "../../hooks/useGetRecipeByIdQuery.hook";
import { ApplicationErrorBanner } from "../../../core/components/banner/application-error-banner.component";
import { RecipeNameAndDescription } from "../../components/recipe-title-and-description/recipe-title-and-description.component";
import { ViewRecipePageBody } from "./view-recipe-page-body.component";
import { RecipeMenu } from "./recipe-menu.component";

/**
 * page for viewing a recipe
 */
export function ViewRecipePage() {
  const { recipeId } = useParams<{
    recipeId: string;
  }>();
  const recipeQuery = useGetRecipeByIdQuery(recipeId);

  const recipeData = recipeQuery.data;
  return (
    <main className="pageMain">
      <RecipeNameAndDescription
        name={recipeData?.recipe?.name}
        shortDescription={recipeData?.recipe?.shortDescription}
      >
        {recipeData && (
          <RecipeMenu
            bookId={recipeData.book.id}
            recipeId={recipeData.recipe.id}
            mayEdit={recipeData.recipe.mayEdit}
            name={recipeData.recipe.name}
            shortDescription={recipeData.recipe.shortDescription}
          />
        )}
      </RecipeNameAndDescription>
      {!recipeId && <ApplicationErrorBanner />}
      {recipeId && (
        <ViewRecipePageBody
          data={recipeQuery.data}
          loadingStatus={recipeQuery.status}
          fetchStatus={recipeQuery.fetchStatus}
        />
      )}
    </main>
  );
}
