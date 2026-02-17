import { useGetRecipeBookById } from "../../hooks/useGetRecipeBookById.hook";
import { RecipeBookNotFoundBanner } from "../recipe-book-not-found-banner/recipe-book-not-found-banner.component";
import { EditRecipeBookForm } from "./edit-recipe-book-form.component";
import { FetchingRecipeBookBanner } from "../fetching-recipe-book-banner/fetching-recipe-book-banner.component";
import { OfflineBanner } from "../offline-banner/offline-banner.component";
import { FetchingRecipeBookFailedBanner } from "../fetching-recipe-book-failed-banner/fetching-recipe-book-failed-banner.component";

/**
 * loads the recipe book data and then renders the form
 * @todo Allow outside caller to control reload action.
 *       Requires prop drilling the reload action to
 *       the underlying banners that trigger a reload.
 */
export function RecipeBookEditor({
  bookId,
  onCancel,
  onSaved,
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
      <EditRecipeBookForm onCancel={onCancel} onSaved={onSaved} data={data} />
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
  onCancel: () => void;
  /**
   * Invoked once the user is done editing
   */
  onSaved: () => void;
}
