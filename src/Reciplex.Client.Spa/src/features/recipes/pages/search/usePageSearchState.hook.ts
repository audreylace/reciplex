import { useState, useEffect } from "react";
import { useSearchParams } from "react-router";
import type { SearchResultKind } from "../../services/recipe-types";

/**
 * Hook that manages the input search state
 * @returns state object that is used to render the component
 */
export function usePageSearchState(): IUsePageSearchStateReturn {
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

/**
 * Return from `usePageSearchState(...)`
 */
interface IUsePageSearchStateReturn {
  /**
   * the current page state
   */
  state: ISearchPageState;
  /**
   * Action updating the search string
   * @param newValue the new search value
   */
  updateSearchString: (newValue: string) => void;
  /**
   * Action updating the filter kind
   * @param filterKind the new filter kind
   */
  updateFilterKind: (filterKind: string) => void;
}

/**
 * Computes state from `filterKindString`. Here the string comes from the radio options of the dialog.
 * @param filterKindString maps a filter kind string into a tuple.
 * @returns first value is the mapped `FilterKindType` and second is the `SearchResultKind[]`.
 * Both values are derived from `filterKindString`.
 */
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

/**
 * State for the search page
 */
interface ISearchPageState {
  /**
   * The current filter string
   */
  searchString: string;
  /**
   * Document filter kind for the dialog UI
   */
  filterKind: FilterKindType;
  /**
   * Filter values for the data hook. Derived from `filterKind`.
   */
  searchResultKind: SearchResultKind[];
}

/**
 * dialog filter kind
 */
type FilterKindType = "book" | "recipe" | "none";
