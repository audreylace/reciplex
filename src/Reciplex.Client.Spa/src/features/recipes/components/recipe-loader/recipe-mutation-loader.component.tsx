import type {
  IRecipeBookModel,
  IRecipeModel,
} from "../../services/recipe-types";
import { FetchingRecipeFailedBanner } from "../recipe-banners/fetching-recipe-failed-banner.component";
import { RecipeNotFoundBanner } from "../recipe-banners/recipe-not-found-banner.component";
import { FetchingRecipeBanner } from "../recipe-banners/fetching-recipe-banner.component";
import { MutationLoader } from "../../../core/components/mutation-loader/mutation-loader.component";
import { useFusedRecipeByIdQuery } from "../../hooks/useFusedRecipeByIdQuery.hook";

/**
 * loads the recipe data and then render using `onRender`.
 */
export function RecipeMutationLoader({
  recipeId,
  dataLoaderRender,
  noCache,
  enabled,
  refetchInterval,
  fetchingRender,
}: RecipeMutationLoaderProps) {
  const recipeQuery = useFusedRecipeByIdQuery(recipeId ?? "", {
    noCache,
    refetchInterval,
    enabled,
  });

  return (
    <MutationLoader
      enabled={enabled}
      onRender={(data) => dataLoaderRender(data.book, data.recipe)}
      notFound={<RecipeNotFoundBanner />}
      queryResult={recipeQuery}
      fetchingBanner={fetchingRender ?? <FetchingRecipeBanner />}
      failureBanner={<FetchingRecipeFailedBanner />}
    />
  );
}

/** props for `RecipeMutationLoader` */
export interface RecipeMutationLoaderProps {
  /** the recipe id to edit */
  recipeId: string;
  /**
   * Invoked to render the content
   */
  dataLoaderRender: (
    book: IRecipeBookModel,
    recipe: IRecipeModel,
  ) => React.ReactNode;
  noCache?: boolean;
  refetchInterval?: number;
  enabled?: boolean;
  fetchingRender?: preact.ComponentChildren;
}
