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
} from "../../services/recipe-types";
import { BookResult } from "./book-result.component";
import { useActiveUserKey } from "../../../auth/hooks/useActiveUser.hook";

/**
 * Page used to search for recipes and books
 */
export function SearchPage() {
  const [searchParams, setSearchParams] = useSearchParams();
  const searchString = searchParams.get("q") ?? "";
  const [fieldV, setFieldV] = useState(searchString);
  const query = useSearchRecipesAndBooks({ searchString });
  const title = searchString ? `${searchString} - Search` : "Search";
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

  useEffect(() => {
    const handle = setTimeout(() => {
      if (searchString !== fieldV) {
        setSearchParams({ q: fieldV });
      }
    }, 100);

    return () => clearTimeout(handle);
  }, [fieldV, searchString, setSearchParams]);

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
          value={fieldV}
          onChange={(s) => {
            setFieldV(s);
          }}
          loading={
            !!(query.isPending && searchString) || searchString !== fieldV
          }
        />
        {listToRender.map(mapper)}
        {searchString && listToRender && listToRender.length < 1 && (
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
