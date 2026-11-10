import Stack from "@mui/material/Stack";
import { SearchField } from "./search-field.component";
import { RecipeResult } from "./recipe-result.component";
import { useSearchParams } from "react-router";
import { useSearchRecipesInBook } from "../../hooks/useSearchRecipesInBook.hook";
import Typography from "@mui/material/Typography";
import Box from "@mui/material/Box";
/**
 * Page used to search for recipes and books
 */
export function SearchPage() {
  const [searchParams, setSearchParams] = useSearchParams();
  const searchString = searchParams.get("q") ?? "";
  const query = useSearchRecipesInBook({ searchString, bookId: "bXHoXJcY" });

  return (
    <>
      <Stack spacing={2}>
        <SearchField
          value={searchString}
          onChange={(s) => {
            setSearchParams({ q: s });
          }}
          loading={!!(query.isPending && searchString)}
        />
        {query.data?.map((d) => (
          <RecipeResult
            key={d.recipeKey}
            name={d.name}
            shortDescription={d.shortDescription}
            recipeId={d.recipeKey}
            bookId={d.bookKey}
            bookTitle={""}
            shared
          />
        ))}
        {searchString && query.data && query.data.length < 1 && (
          <Box sx={{ textAlign: "center", p: 2 }}>
            <Typography variant="h5">No results</Typography>
          </Box>
        )}
      </Stack>
    </>
  );
}
