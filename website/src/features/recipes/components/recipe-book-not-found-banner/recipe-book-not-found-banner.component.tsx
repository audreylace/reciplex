import { makeBookListPath } from "../../route-utils";
import { ActionBanner } from "../action-banner/action-banner.component";

export function RecipeBookNotFoundBanner({}: {}) {
  return (
    <ActionBanner
      to={makeBookListPath()}
      linkText="View you recipe books"
      message="Recipe book not found"
    />
  );
}
