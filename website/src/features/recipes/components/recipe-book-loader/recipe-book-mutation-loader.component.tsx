import { useGetRecipeBookById } from "../../hooks/useGetRecipeBookById.hook";
import { RecipeBookNotFoundBanner } from "../book-banners/recipe-book-not-found-banner.component";
import { FetchingRecipeBookBanner } from "../book-banners/fetching-recipe-book-banner.component";
import { FetchingRecipeBookFailedBanner } from "../book-banners/fetching-recipe-book-failed-banner.component";
import type { IRecipeBookModel } from "../../services/recipe-types";
import { MutationLoader } from "../../../core/components/mutation-loader/mutation-loader.component";

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

  return (
    <MutationLoader
      enabled={enabled}
      onRender={(data) => onRender(data)}
      notFound={<RecipeBookNotFoundBanner />}
      queryResult={recipeBookQuery}
      fetchingBanner={<FetchingRecipeBookBanner />}
      failureBanner={<FetchingRecipeBookFailedBanner />}
    />
  );
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
