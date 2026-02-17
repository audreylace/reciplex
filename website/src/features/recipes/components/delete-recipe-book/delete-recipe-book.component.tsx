import { useGetRecipeBookById } from "../../hooks/useGetRecipeBookById.hook";
import { RecipeBookNotFoundBanner } from "../recipe-book-not-found-banner/recipe-book-not-found-banner.component";
import { FetchingRecipeBookBanner } from "../fetching-recipe-book-banner/fetching-recipe-book-banner.component";
import { OfflineBanner } from "../offline-banner/offline-banner.component";
import { FetchingRecipeBookFailedBanner } from "../fetching-recipe-book-failed-banner/fetching-recipe-book-failed-banner.component";
import { DeleteRecipeBookForm } from "./delete-recipe-book-form";

/**
 * loads the recipe book data and then renders the form used to delete the book
 * @todo Allow outside caller to control reload action.
 *       Requires prop drilling the reload action to
 *       the underlying banners that trigger a reload.
 * @todo Figure out how to DRY this up with `RecipeBookEditor`. The code is almost the same.
 */
export function DeleteRecipeBook({
  bookId,
  onCancel,
  onDeleted,
}: RecipeBookEditorProps) {
  const recipeBookQuery = useGetRecipeBookById(bookId, {
    noCache: true,
    refetchInterval: 10000,
  });

  if (recipeBookQuery.isFetchedAfterMount) {
    const data = recipeBookQuery.data;
    if (!data) {
      return <RecipeBookNotFoundBanner />;
    }
    return (
      <DeleteRecipeBookForm
        onCancel={onCancel}
        onDeleted={onDeleted}
        data={data}
      />
    );
  } else if (recipeBookQuery.isError) {
    return <FetchingRecipeBookFailedBanner />;
  } else if (recipeBookQuery.isPaused) {
    return <OfflineBanner />;
  }

  return <FetchingRecipeBookBanner />;
}

/** props for `RecipeBookEditor` */
export interface RecipeBookEditorProps {
  /** the book id to edit */
  bookId: string;
  /**
   * Invoked on cancel
   */
  onCancel: (info?: {
    name: string;
    shortDescription: string;
    bookId: string;
  }) => void;
  /**
   * Invoked once the user has deleted the book
   */
  onDeleted: () => void;
}
