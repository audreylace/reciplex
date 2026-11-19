import { useMemo } from "react";
import { useSearchRecipesAndBooks } from "../../hooks/useSearchRecipesAndBooks.hook";
import type {
  IRecipeBookModel,
  IRecipeListEntryJsonResponse,
  SearchResultKind,
} from "../../services/recipe-types";

/**
 * hook that computes the data to show in the search page based on the user's query
 * @param param0 hook args
 * @returns a object with `data` holding the results, and `isLoading` if the query is still in progress
 */
export function useSearchPageData({
  searchString,
  searchResultKind,
}: IUseSearchPageDataArgs): IUseSearchPageDataReturn {
  const query = useSearchRecipesAndBooks({
    searchString: searchString,
    kind: searchResultKind,
  });
  const listToRender = useMemo(() => {
    const hits = query.data?.hits ?? [];
    const books: Record<string, IRecipeBookModel> = {};
    (query.data?.books ?? []).forEach((b) => (books[b.id] = b));
    const recipes: Record<string, IRecipeListEntryJsonResponse> = {};
    (query.data?.recipes ?? []).forEach((r) => (recipes[r.recipeKey] = r));

    const hitsData: TaggedSearchPageDataUnionType[] = [];
    for (const ptr of hits) {
      if (
        ptr.kind === "recipe" &&
        recipes[ptr.documentKey] &&
        books[recipes[ptr.documentKey].bookKey]
      ) {
        hitsData.push({
          kind: "recipe",
          recipe: recipes[ptr.documentKey],
          book: books[recipes[ptr.documentKey].bookKey],
          renderKey: `r_${recipes[ptr.documentKey].recipeKey}`,
        });
      } else if (ptr.kind === "book" && books[ptr.documentKey]) {
        hitsData.push({
          kind: "book",
          book: books[ptr.documentKey],
          renderKey: `b_${books[ptr.documentKey].id}`,
        });
      }
    }

    return hitsData;
  }, [query.data?.books, query.data?.hits, query.data?.recipes]);

  return {
    data: listToRender,
    isLoading: !!(query.isPending && query.isEnabled && searchString),
  };
}

/**
 * Shape of the individual objects in the list of results rendered to screen
 */
export type TaggedSearchPageDataUnionType =
  | {
      kind: "recipe";
      recipe: IRecipeListEntryJsonResponse;
      book: IRecipeBookModel;
      renderKey: string;
    }
  | { kind: "book"; book: IRecipeBookModel; renderKey: string };

/**
 * shape of the object passed to the `useSearchPageData` hook
 */
interface IUseSearchPageDataArgs {
  /** the current search string */
  searchString: string;
  /** the set of documents to retrieve */
  searchResultKind: SearchResultKind[];
}

/**
 * shape of the object returned from the `useSearchPageData` hook
 */
interface IUseSearchPageDataReturn {
  /** the data to render */
  data: TaggedSearchPageDataUnionType[];
  /** true if the data is still being loaded */
  isLoading: boolean;
}
