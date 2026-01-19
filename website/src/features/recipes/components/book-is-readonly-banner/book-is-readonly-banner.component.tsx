import { makeViewRecipeBookPath } from "../../route-utils";
import { ActionBanner } from "../action-banner/action-banner.component";

/**
 * Read-only banner for a recipe book
 * @param param0 react args
 * @returns jsx tree to render
 */
export function BookIsReadonlyBanner({ bookId }: { bookId: string }) {
  return (
    <ActionBanner
      to={makeViewRecipeBookPath(bookId)}
      message="You may not edit this recipe book"
      linkText="Return to recipe book"
    />
  );
}
