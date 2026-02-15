import { makeViewRecipePath } from "../../route-utils";
import { ActionBanner } from "../action-banner/action-banner.component";

export function RecipeIsReadonlyBanner({
  bookId,
  recipeId,
}: {
  bookId: string;
  recipeId: string;
}) {
  return (
    <ActionBanner
      to={makeViewRecipePath(bookId, recipeId)}
      message="You may not edit this recipe"
      linkText="View recipe"
    />
  );
}
