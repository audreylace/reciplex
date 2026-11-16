import Stack from "@mui/material/Stack";
import { SearchField } from "./search-field.component";
import { RecipeResult } from "./recipe-result.component";
import { useSearchParams } from "react-router";
import { useSearchRecipesAndBooks } from "../../hooks/useSearchRecipesAndBooks.hook";
import Typography from "@mui/material/Typography";
import Box from "@mui/material/Box";
import { BrowserTitle } from "../../../core/components/browser-title/browser-title.component";
import { useEffect, useMemo, useState } from "react";
import type {
  IRecipeBookModel,
  IRecipeListEntryJsonResponse,
  SearchResultKind,
} from "../../services/recipe-types";
import { BookResult } from "./book-result.component";
import { useActiveUserKey } from "../../../auth/hooks/useActiveUser.hook";
import { FilterDialog } from "./filter-dialog";

/**
 * Page used to search for recipes and books
 */
export function SearchPage() {
  const { state, updateSearchString, updateFilterKind } = usePageSearchState();
  const query = useSearchRecipesAndBooks({
    searchString: state.searchString,
    kind: state.searchResultKind,
  });
  const title = state.searchString
    ? `${state.searchString} - Search`
    : "Search";
  const userId = useActiveUserKey();

  const listToRender = useMemo(() => {
    const hits = query.data?.hits ?? [];
    const books: Record<string, IRecipeBookModel> = {};
    (query.data?.books ?? []).forEach((b) => (books[b.id] = b));
    const recipes: Record<string, IRecipeListEntryJsonResponse> = {};
    (query.data?.recipes ?? []).forEach((r) => (recipes[r.recipeKey] = r));

    const hitsData: TaggedHitUnionType[] = [];
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
        });
      } else if (ptr.kind === "book" && books[ptr.documentKey]) {
        hitsData.push({ kind: "book", book: books[ptr.documentKey] });
      }
    }

    return hitsData;
  }, [query.data?.books, query.data?.hits, query.data?.recipes]);

  const mapper = (d: TaggedHitUnionType) =>
    d.kind === "recipe" ? (
      <RecipeResult
        key={"recipe_" + d.recipe.recipeKey}
        name={d.recipe.name}
        shortDescription={d.recipe.shortDescription}
        recipeId={d.recipe.recipeKey}
        bookId={d.recipe.bookKey}
        bookTitle={d.book.name}
        shared={d.book.ownerId !== userId}
      />
    ) : d.kind === "book" ? (
      <BookResult
        name={d.book.name}
        shortDescription={d.book.shortDescription}
        bookId={d.book.id}
        key={"book_" + d.book.id}
        shared={d.book.ownerId !== userId}
      />
    ) : null;

  return (
    <>
      <BrowserTitle title={title} />
      <Stack spacing={2}>
        <SearchField
          value={state.searchString}
          onChange={(s) => {
            updateSearchString(s);
          }}
          loading={!!(query.isPending && query.isEnabled && state.searchString)}
          settingsDialog={
            <FilterDialog
              filterOptions={radioFilterOptions}
              selectedFilterOption={state.filterKind}
              onFilterChange={(newValue) => {
                updateFilterKind(newValue);
              }}
            />
          }
        />
        {listToRender.map(mapper)}
        {state.searchString && listToRender && listToRender.length < 1 && (
          <Box sx={{ textAlign: "center", p: 2 }}>
            <Typography variant="h5">No results</Typography>
          </Box>
        )}
      </Stack>
    </>
  );
}

type TaggedHitUnionType =
  | {
      kind: "recipe";
      recipe: IRecipeListEntryJsonResponse;
      book: IRecipeBookModel;
    }
  | { kind: "book"; book: IRecipeBookModel };

const radioFilterOptions = [
  {
    caption: "All",
    id: "none",
  },
  {
    caption: "Books",
    id: "book",
  },
  {
    caption: "Recipes",
    id: "recipe",
  },
];

function usePageSearchState(): IUsePageSearchStateReturn {
  const [searchParams, setSearchParams] = useSearchParams();

  const searchQueryString = searchParams.get("q") ?? "";
  const filterQueryString = searchParams.get("filter");

  const filterAndSearch = mapDocumentKind(filterQueryString ?? "");
  const [pageState, setPageState] = useState<ISearchPageState>({
    searchString: searchQueryString,
    filterKind: filterAndSearch.filterKind,
    searchResultKind: filterAndSearch.searchResultKind,
  });

  useEffect(() => {
    const handle = setTimeout(() => {
      setSearchParams({
        q: pageState.searchString,
        filter: pageState.filterKind,
      });
    }, 100);

    return () => clearTimeout(handle);
  }, [pageState.filterKind, pageState.searchString, setSearchParams]);

  return {
    state: pageState,
    updateSearchString: (newValue) => {
      setPageState((state) => {
        return { ...state, searchString: newValue };
      });
    },
    updateFilterKind: (newValue) => {
      setPageState((state) => {
        const newMappedValue = mapDocumentKind(newValue);
        return {
          ...state,
          filterKind: newMappedValue.filterKind,
          searchResultKind: newMappedValue.searchResultKind,
        };
      });
    },
  };
}

/** state for the search page */
interface ISearchPageState {
  /** the current filter string */
  searchString: string;
  /** document filter kind */
  filterKind: FilterKindType;
  searchResultKind: SearchResultKind[];
}

interface IUsePageSearchStateReturn {
  /** the current page state */
  state: ISearchPageState;
  updateSearchString: (newValue: string) => void;
  updateFilterKind: (filterKind: string) => void;
}

type FilterKindType = "book" | "recipe" | "none";

function mapDocumentKind(filterKindString: string) {
  let filterKind: FilterKindType = "none";
  let searchResultKind: SearchResultKind[] = ["recipe", "book"];
  switch (filterKindString) {
    case "recipe":
      filterKind = "recipe";
      searchResultKind = ["recipe"];
      break;
    case "book":
      filterKind = "book";
      searchResultKind = ["book"];
      break;
  }

  return { filterKind, searchResultKind };
}
