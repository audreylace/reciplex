import { useParams } from "react-router";
import { RecipeNotFoundBanner } from "../../components/recipe-not-found-banner/recipe-not-found-banner.component";
import { DeleteRecipePageBody } from "./delete-recipe-page-body.component";
import { BrowserTitle } from "../../../core/components/browser-title/browser-title.component";

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

  return (
    <>
      <BrowserTitle title="Deleting Recipe" />
      <DeleteRecipePageBody recipeId={recipeId} />;
    </>
  );
}
