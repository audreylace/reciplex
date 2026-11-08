import { useSearchParams } from "react-router";
import { useGetRecipesInBookQuery } from "../../hooks/useGetRecipesInBookQuery.hook";
import { useRecipeClientStateContext } from "../../hooks/useRecipeClientStateContext.hook";
import type { IRecipeListEntryJsonResponse } from "../../services/recipe-types";

/**
 * the cursor type. Encodes if the user went forward or backwards.
 */
export type RecipeListCursorType = "next" | "previous";

/**
 * Loads recipe list and determines if there are previous and next pages.
 * Page loads are separate requests.
 * @param param0 hook arguments
 * @returns recipe list and cursor data
 */
export function useBrowseRecipesInBookAsList({
  enable,
  bookId,
}: IUseListRecipesInBookArgs): IUseRecipeListPageCursors {
  const [searchParams, setSearchParams] = useSearchParams();
  const source =
    searchParams.get("source") === "previous" ? "previous" : "next";
  const at = searchParams.get("at") ?? undefined;
  const selectedSize = useRecipeClientStateContext(
    (state) => state.recipeListPageSize,
  );
  const recipeListQuery = useGetRecipesInBookQuery(
    bookId,
    source,
    at,
    selectedSize,
    enable,
  );

  let nextQueryPos;
  let previousQueryPos;
  if (recipeListQuery.isSuccess && recipeListQuery.data && enable) {
    if (source !== "next" || recipeListQuery.data.length > 0) {
      nextQueryPos =
        recipeListQuery.data[recipeListQuery.data.length - 1]?.recipeKey ?? at;
    }
    if (source !== "previous" || recipeListQuery.data.length > 0) {
      previousQueryPos = recipeListQuery.data[0]?.recipeKey ?? at;
    }
  }

  const { isSuccess: nextRecipeSuccess, isPending: nextRecipePending } =
    useGetRecipesInBookQuery(
      bookId,
      "next",
      nextQueryPos,
      selectedSize,
      recipeListQuery.isSuccess && !!nextQueryPos && enable,
    );
  const { isSuccess: previousRecipeSuccess, isPending: previousRecipePending } =
    useGetRecipesInBookQuery(
      bookId,
      "previous",
      previousQueryPos,
      selectedSize,
      recipeListQuery.isSuccess && !!previousQueryPos && enable,
    );

  return {
    previousPending: enable && previousRecipePending,
    previousSuccess: previousRecipeSuccess,
    nextPending: enable && nextRecipePending,
    nextSuccess: nextRecipeSuccess,
    dataPending: recipeListQuery.isPending,
    dataSuccess: recipeListQuery.isSuccess,
    dataError: recipeListQuery.isError,
    data:
      recipeListQuery.isSuccess && recipeListQuery.data
        ? recipeListQuery.data
        : undefined,

    // TODO - these are broken - fix
    goNext: nextQueryPos
      ? () => {
          setSearchParams({
            at: nextQueryPos,
            source: "next",
          });
        }
      : undefined,

    jumpToEnd: nextQueryPos
      ? () => {
          setSearchParams({
            source: "previous",
          });
        }
      : undefined,

    goPrevious: previousQueryPos
      ? () => {
          setSearchParams({
            at: previousQueryPos,
            source: "previous",
          });
        }
      : undefined,

    jumpToStart: previousQueryPos
      ? () => {
          setSearchParams({
            source: "next",
          });
        }
      : undefined,
  };
}

/** result from the hook `useListRecipesInBook` */
export interface IUseRecipeListPageCursors {
  /** if the request loading the previous page is complete */
  previousSuccess: boolean;
  /** if the request for the previous page is in progress */
  previousPending: boolean;
  /** if the request for the next page has completed */
  nextSuccess: boolean;
  /** if the request for the next page is in progress */
  nextPending: boolean;
  /** data for the current page */
  data?: IRecipeListEntryJsonResponse[];
  /** if data for the current page has loaded in */
  dataSuccess: boolean;
  /** if data for the current page is still loading */
  dataPending: boolean;
  /** if the data load failed */
  dataError: boolean;
  /** handler to go to next page; undefined if there is not a next page */
  goNext?: () => void;
  /** handler to go to previous page; undefined if there is not a previous page */
  goPrevious?: () => void;
  /** handler to go to last page; undefined if there is not a last page */
  jumpToEnd?: () => void;
  /** handler to go to first page; undefined if there is not a first page */
  jumpToStart?: () => void;
}

/** args for the hook `useListRecipesInBook` */
export interface IUseListRecipesInBookArgs {
  /** if the hook is enabled */
  enable: boolean;
  /** the id of the book */
  bookId: string;
}
