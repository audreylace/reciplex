import { OfflineBanner } from "../offline-banner/offline-banner.component";
import type {
  IRecipeBookModel,
  IRecipeModel,
} from "../../services/recipe-types";
import { useGetRecipeByIdQuery } from "../../hooks/useGetRecipeByIdQuery.hook";
import { FetchingRecipeFailedBanner } from "../fetching-recipe-failed-banner/fetching-recipe-failed-banner.component";
import { RecipeNotFoundBanner } from "../recipe-not-found-banner/recipe-not-found-banner.component";
import { FetchingRecipeBanner } from "../fetching-recipe-banner/fetching-recipe-banner.component";

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
  const recipeQuery = useGetRecipeByIdQuery(recipeId ?? "", {
    noCache,
    refetchInterval,
    enabled,
  });

  enabled ??= true;
  if (!enabled) {
    return null;
  }

  if (recipeQuery.isFetchedAfterMount) {
    const data = recipeQuery.data;
    if (!data) {
      return <RecipeNotFoundBanner />;
    }
    return dataLoaderRender(data.book, data.recipe);
  } else if (recipeQuery.isError) {
    return <FetchingRecipeFailedBanner />;
  } else if (recipeQuery.isPaused) {
    return <OfflineBanner />;
  }

  if (!fetchingRender) {
    return <FetchingRecipeBanner />;
  }

  if (typeof fetchingRender === "function") {
    return fetchingRender();
  }
  return fetchingRender;
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
  fetchingRender?:
    | preact.ComponentChildren
    | undefined
    | (() => preact.ComponentChildren | undefined);
}
