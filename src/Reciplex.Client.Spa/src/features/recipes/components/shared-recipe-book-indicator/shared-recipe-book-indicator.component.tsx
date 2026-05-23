import { useGetRecipeBookById } from "../../hooks/useGetRecipeBookById.hook";
import { useActiveUserKey } from "../../../auth/hooks/useActiveUser.hook";
import { SharedRecipeBookIndicatorNoLoad } from "./shared-recipe-book-indicator-no-load.component";

/**
 * Chip indicating if a recipe book is shared.
 * Determines if book is shared with the user. If the book is shared then it renders.
 * Otherwise it renders as null.
 */
export function SharedRecipeBookIndicator({
  bookId,
}: ISharedRecipeBookIndicatorProps) {
  const activeKey = useActiveUserKey();
  const book = useGetRecipeBookById(bookId);
  if (book.data && activeKey && activeKey !== book.data.ownerId && bookId) {
    return <SharedRecipeBookIndicatorNoLoad bookId={bookId} />;
  }
  return null;
}

/** props for `<SharedRecipeBookIndicator />` */
export interface ISharedRecipeBookIndicatorProps {
  /** the id of the book */
  bookId?: string;
}
