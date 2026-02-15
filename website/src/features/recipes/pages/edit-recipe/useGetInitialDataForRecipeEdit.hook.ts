import { useParams } from "react-router";
import { useGetRecipeByIdQuery } from "../../hooks/useGetRecipeByIdQuery.hook";
import type {
  IRecipeBookModel,
  IRecipeModel,
} from "../../../../services/recipe-store";
import { useState } from "preact/hooks";

/**
 * hook for getting the initial data for the recipe page
 */
export function useGetInitialDataForRecipeEdit(): GetInitialDataForRecipeEditReturn {
  const { recipeId } = useParams<{ recipeId: string }>();
  const recipeQuery = useGetRecipeByIdQuery(recipeId ?? "", {
    noCache: true,
    refetchInterval: 15000,
  });
  const [loadedData, setLoadedData] =
    useState<GetInitialDataForRecipeEditReturnWithData | null>(null);

  if (loadedData) {
    // catch changes to the underlying data
    if (recipeQuery.data?.recipe?.versionTag !== loadedData.recipe.versionTag) {
      const newState: GetInitialDataForRecipeEditReturnWithData = {
        tag: "conflict",
        recipe: loadedData.recipe,
        book: loadedData.book,
      };
      setLoadedData((s) => (s && s.tag !== "conflict" ? newState : s));
      return newState;
    }

    return loadedData;
  }

  if (!recipeId) {
    // bad route -- missing route parameters
    return { tag: "bad-route" };
  }

  if (recipeQuery.status === "success") {
    // only want new data not cached data from react query
    if (!recipeQuery.isFetchedAfterMount) {
      if (recipeQuery.isPaused) {
        return { tag: "offline" };
      }
      if (recipeQuery.isRefetching) {
        return { tag: "loading" };
      }
    }

    // API returns null when entry is not found
    if (!recipeQuery.data) {
      return { tag: "not-found" };
    }

    // lock in data after first load
    if (!loadedData) {
      const newState: GetInitialDataForRecipeEditReturnWithData = {
        tag: "loaded",
        recipe: recipeQuery.data.recipe,
        book: recipeQuery.data.book,
      };
      setLoadedData(newState);
      return newState;
    }

    return { tag: "loading" };
  }

  // pending load
  if (recipeQuery.status === "pending") {
    if (recipeQuery.isPaused) {
      return { tag: "offline" };
    }
    return { tag: "loading" };
  }

  return { tag: "loading-failed" };
}

export const EditRecipePageLoadingState = {
  loading: "loading",
  loadingFailed: "loading-failed",
  notFound: "not-found",
  badRoute: "bad-route",
  loaded: "loaded",
  offline: "offline",
  conflict: "conflict",
  readonly: "read-only",
} as const;
export type EditRecipePageLoadingState =
  (typeof EditRecipePageLoadingState)[keyof typeof EditRecipePageLoadingState];

type GetInitialDataForRecipeEditReturn =
  | GetInitialDataForRecipeEditReturnNoData<
      "loading" | "loading-failed" | "not-found" | "bad-route" | "offline"
    >
  | GetInitialDataForRecipeEditReturnWithData;

interface GetInitialDataForRecipeEditReturnBase {
  tag: EditRecipePageLoadingState;
}

interface GetInitialDataForRecipeEditReturnNoData<
  TTag extends EditRecipePageLoadingState,
> extends GetInitialDataForRecipeEditReturnBase {
  tag: TTag;
}

interface GetInitialDataForRecipeEditReturnWithData extends GetInitialDataForRecipeEditReturnBase {
  tag: "loaded" | "conflict" | "read-only";
  recipe: IRecipeModel;
  book: IRecipeBookModel;
}
