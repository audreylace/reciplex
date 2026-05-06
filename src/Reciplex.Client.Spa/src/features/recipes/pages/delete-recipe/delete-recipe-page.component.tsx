import { useParams } from "react-router";
import { RecipeNotFoundBanner } from "../../components/recipe-not-found-banner/recipe-not-found-banner.component";
import { DeleteRecipePageBody } from "./delete-recipe-page-body.component";

/**
 * Page for deleting a recipe
 */
export function DeleteRecipePage() {
  const { recipeId } = useParams<{
    recipeId: string;
  }>();

  if (!recipeId) {
    return <RecipeNotFoundBanner />;
  }

  return <DeleteRecipePageBody recipeId={recipeId} />;
}
