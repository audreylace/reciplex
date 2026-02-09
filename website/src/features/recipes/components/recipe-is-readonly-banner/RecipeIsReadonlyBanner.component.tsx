import { makeViewRecipePath } from "../../route-utils";
import { ActionBanner } from "../action-banner/action-banner.component";

export function RecipeIsReadonlyBanner({ recipeId }: { recipeId: string }) {
  return (
    <ActionBanner
      to={makeViewRecipePath(recipeId)}
      message="You may not edit this recipe"
      linkText="View recipe"
    />
  );
}
