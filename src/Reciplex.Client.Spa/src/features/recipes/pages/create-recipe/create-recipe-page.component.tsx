import { makeViewRecipeBookPath, makeViewRecipePath } from "../../route-utils";
import { Link, useNavigate, useParams } from "react-router";
import { useGetRecipeBookById } from "../../hooks/useGetRecipeBookById.hook";
import { RecipeBookNotFoundBanner } from "../../components/book-banners/recipe-book-not-found-banner.component";
import { NameAndShortDescriptionForm } from "../../components/name-and-short-description-form/name-and-short-description-form.component";
import { useCreateRecipeMutation } from "../../hooks/useCreateRecipeMutation.hook";
import Alert from "@mui/material/Alert";
import Button from "@mui/material/Button";

/**
 * Entry point for create recipe page component
 */
export function CreateRecipePage() {
  const { bookId } = useParams<{
    bookId: string;
  }>();
  const navigate = useNavigate();
  const { reset, isPending, isError, mutateAsync } = useCreateRecipeMutation();
  const {
    data: book,
    isSuccess: querySuccess,
    isError: queryError,
    isPending: queryPending,
    refetch,
  } = useGetRecipeBookById(bookId);

  if ((querySuccess && !book) || !bookId) {
    return <RecipeBookNotFoundBanner />;
  }

  if (querySuccess && book && !book.mayEdit) {
    return (
      <Alert
        severity="error"
        variant="filled"
        action={
          <Button
            color="inherit"
            size="small"
            component={Link}
            to={makeViewRecipeBookPath(bookId)}
          >
            View Book
          </Button>
        }
      >
        You may not edit this book
      </Alert>
    );
  }

  return (
    <NameAndShortDescriptionForm
      legendText="Create New Recipe"
      nameLabel="Recipe Title"
      nameHelpText="Title of the recipe"
      nameMaxLength={128}
      shortDescriptionHelpText="Concise description of the recipe"
      shortDescriptionLabel="Recipe Short Description"
      shortDescriptionMaxLength={256}
      onSuccess={async (data) => {
        const recipe = await mutateAsync({
          name: data.name,
          shortDescription: data.shortDescription,
          bookId: bookId,
        });
        navigate(makeViewRecipePath(recipe.bookId, recipe.id));
      }}
      onReset={() => {
        if (isError) {
          reset();
        }
        if (queryError) {
          refetch();
        }
      }}
      pending={isPending}
      showError={isError || queryError}
      submitText="Create Recipe"
      showSkeleton={queryPending}
    />
  );
}
