import { useParams } from "react-router";
import { NotFoundAlert } from "../../../core/components/not-found-alert/not-found-alert.component";
import { EditRecipePageBody } from "./edit-recipe-page-body.component";

/** Page for editing a recipe */
export function EditRecipePage() {
  const { recipeId } = useParams<{
    recipeId: string;
  }>();

  if (!recipeId) {
    return <NotFoundAlert />;
  }

  return <EditRecipePageBody recipeId={recipeId} />;
}
