import Stack from "@mui/material/Stack";
import { SearchField } from "./search-field.component";
import Typography from "@mui/material/Typography";
import Box from "@mui/material/Box";
import { BrowserTitle } from "../../../core/components/browser-title/browser-title.component";
import { useActiveUserKey } from "../../../auth/hooks/useActiveUser.hook";
import { FilterDialog } from "./filter-dialog";
import { useSearchPageData } from "./useSearchPageData.hook";
import { usePageSearchState } from "./usePageSearchState.hook";
import { ResultMapper } from "./result-mapper.component";

/**
 * Page used to search for recipes and books
 */
export function SearchPage() {
  const { state, updateSearchString, updateFilterKind } = usePageSearchState();

  const title = state.searchString
    ? `${state.searchString} - Search`
    : "Search";
  const userId = useActiveUserKey();
  const { isLoading: searchLoading, data: searchData } = useSearchPageData({
    searchString: state.searchString,
    searchResultKind: state.searchResultKind,
  });

  return (
    <>
      <BrowserTitle title={title} />
      <Stack spacing={2}>
        <SearchField
          value={state.searchString}
          onChange={(s) => {
            updateSearchString(s);
          }}
          loading={searchLoading}
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
        {userId &&
          searchData.map((result) => (
            <ResultMapper
              data={result}
              userId={userId}
              key={result.renderKey}
            />
          ))}
        {state.searchString && searchData && searchData.length < 1 && (
          <Box sx={{ textAlign: "center", p: 2 }}>
            <Typography variant="h5">No results</Typography>
          </Box>
        )}
      </Stack>
    </>
  );
}

/**
 * filter radio options for the dialog
 */
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
