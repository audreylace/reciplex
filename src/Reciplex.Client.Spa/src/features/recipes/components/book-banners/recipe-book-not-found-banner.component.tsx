import { NotFoundAlert } from "../../../core/components/not-found-alert/not-found-alert.component";
import { makeBookListPath } from "../../route-utils";

/** error banner shown when a recipe book was not found */
export function RecipeBookNotFoundBanner() {
  return (
    <NotFoundAlert
      href={makeBookListPath()}
      caption="View Books"
      entityName="Recipe Book"
    />
  );
}
