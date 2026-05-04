import { makeEditRecipePath } from "../../route-utils";
import { useNavigate, useParams } from "react-router";
import { useGetRecipeBookById } from "../../hooks/useGetRecipeBookById.hook";
import { RecipeBookNotFoundBanner } from "../../components/recipe-book-not-found-banner/recipe-book-not-found-banner.component";
import { NameAndShortDescriptionForm } from "../../components/name-and-short-description-form/name-and-short-description-form.component";
import { useCreateRecipeMutation } from "../../hooks/useCreateRecipeMutation.hook";
import { BookMutationNotAuthorizedBanner } from "../../components/book-mutation-not-authorized-banner/book-mutation-not-authorized-banner.component";

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
      <BookMutationNotAuthorizedBanner
        bookId={book.id}
        message="You may not add recipes to this book"
      />
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
        navigate(makeEditRecipePath(recipe.bookId, recipe.id));
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
