import type { IRecipeBookModel } from "../../services/recipe-types";
import { BookSettingsMenuButton } from "./book-settings-menu-button.component";

/** wraps `BookSettingsMenuButton` plucking off the relevant properties from `IRecipeBookModel` */
export function BookSettingsMenuButtonViaModel({
  book,
}: IBookSettingsMenuButtonViaModelProps) {
  if (!book) {
    return null;
  }
  return (
    <BookSettingsMenuButton
      bookId={book.id}
      mayEdit={book?.mayEdit ?? false}
      mayDelete={book?.mayDelete ?? false}
      mayManageShareAccess={book?.mayManageAccess ?? false}
    />
  );
}

/** props for `BookSettingsMenuButtonViaModel` */
export interface IBookSettingsMenuButtonViaModelProps {
  /** the book model to render the menu button for */
  book?: IRecipeBookModel | null;
}
