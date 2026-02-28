import { makeViewRecipePath } from "../../route-utils";
import { ErrorBanner } from "../../../core/components/banner/banner.component";

/** Banner showed when the user lacks permissions to mutate a recipe */
export function RecipeIsReadonlyBanner({
  bookId,
  recipeId,
}: RecipeIsReadonlyBannerProps) {
  return (
    <ErrorBanner
      to={makeViewRecipePath(bookId, recipeId)}
      icon="bi bi-ban"
      title="Recipe Read-Only"
      message="You may not edit this recipe"
      buttonCaption="View recipe"
    />
  );
}

/** Component properties for `RecipeIsReadonlyBanner` */
export interface RecipeIsReadonlyBannerProps {
  /** The id of the book holding the recipe */
  bookId: string;
  /** The id of the read-only recipe */
  recipeId: string;
}
