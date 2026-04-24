import Box from "@mui/material/Box";
import { LoadingFailedAlert } from "../../../core/components/loading-failed-alert/loading-failed-alert.component";
import { LoadingIndicator } from "../../../core/components/loading-indicator/loading-indicator.component";
import { RecipeBookNotFoundBanner } from "../../components/recipe-book-not-found-banner/recipe-book-not-found-banner.component";
import { RecipeTable } from "../../components/recipe-table/recipe-table.component";
import { useGetRecipeBookById } from "../../hooks/useGetRecipeBookById.hook";
import { useGetRecipesInBookQuery } from "../../hooks/useGetRecipesInBookQuery.hook";
import Stack from "@mui/material/Stack";
import Typography from "@mui/material/Typography";
import { RecipeBookMenu } from "../../components/book-menu/recipe-book-menu.component";
import { useRecipeClientStateContext } from "../../hooks/useRecipeClientStateContext.hook";

export function ViewRecipeBookPageBody({
  bookId,
  source,
  at,
}: {
  bookId: string;
  source: "next" | "previous";
  at?: string;
}) {
  const selectedSize = useRecipeClientStateContext(
    (state) => state.recipeListPageSize,
  );
  const {
    data: book,
    isError: getRecipeBookError,
    isPending: bookPending,
  } = useGetRecipeBookById(bookId);

  const {
    isError: getRecipesError,
    isPending: getRecipesPending,
    data: recipeList,
    isSuccess: recipeListSuccess,
  } = useGetRecipesInBookQuery(bookId, source, at, selectedSize);

  if (
    getRecipesError ||
    getRecipeBookError ||
    (recipeListSuccess && !recipeList)
  ) {
    return <LoadingFailedAlert />;
  }

  if (bookPending) {
    return <LoadingIndicator />;
  }

  if (!book) {
    return <RecipeBookNotFoundBanner />;
  }

  return (
    <>
      <BodyHeader
        bookId={book.id}
        bookName={book.name}
        bookDescription={book.shortDescription}
        mayEditBook={book.mayEdit}
        mayDeleteBook={book.mayDelete}
      />
      <RecipeTable
        pending={getRecipesPending}
        recipes={recipeList?.recipes}
        next={recipeList?.nextCursor}
        previous={recipeList?.previousCursor}
      />
    </>
  );
}

function BodyHeader({
  bookId,
  mayEditBook,
  mayDeleteBook,
  bookName,
  bookDescription,
}: IBodyHeaderProps) {
  return (
    <Box
      sx={{
        my: 2,
      }}
    >
      <Stack direction={"row"} sx={{ width: "100%" }}>
        <Box
          sx={{
            flex: "1 1 auto",
          }}
        >
          <Typography variant="h4">{bookName}</Typography>
        </Box>
        <RecipeBookMenu
          bookId={bookId}
          mayEdit={mayEditBook}
          mayDelete={mayDeleteBook}
        />
      </Stack>
      <Typography variant="subtitle1">{bookDescription}</Typography>
    </Box>
  );
}

interface IBodyHeaderProps {
  bookId: string;
  mayEditBook: boolean | undefined;
  mayDeleteBook: boolean | undefined;
  bookName: string;
  bookDescription: string;
}
