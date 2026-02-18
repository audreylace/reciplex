import { useGetRecipeBookById } from "../../hooks/useGetRecipeBookById.hook";
import { RecipeBookNotFoundBanner } from "../recipe-book-not-found-banner/recipe-book-not-found-banner.component";
import { FetchingRecipeBookBanner } from "../fetching-recipe-book-banner/fetching-recipe-book-banner.component";
import { OfflineBanner } from "../offline-banner/offline-banner.component";
import { FetchingRecipeBookFailedBanner } from "../fetching-recipe-book-failed-banner/fetching-recipe-book-failed-banner.component";
import type { IRecipeBookModel } from "../../services/recipe-types";

/**
 * loads the recipe book data and then render using `onRender`.
 */
export function RecipeBookMutationLoader({
  bookId,
  onRender,
  noCache,
  enabled,
  refetchInterval,
}: RecipeBookEditorProps) {
  const recipeBookQuery = useGetRecipeBookById(bookId, {
    noCache,
    refetchInterval,
    enabled,
  });

  if (recipeBookQuery.isFetchedAfterMount) {
    const data = recipeBookQuery.data;
    if (!data) {
      return <RecipeBookNotFoundBanner />;
    }
    return onRender(data);
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
   * Invoked to render the content
   */
  onRender: (book: IRecipeBookModel) => React.ReactNode;
  noCache?: boolean;
  refetchInterval?: number;
  enabled?: boolean;
}
