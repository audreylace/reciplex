import { Link, useNavigate } from "react-router";
import { useDeleteRecipeMutation } from "../../hooks/useDeleteRecipeMutation.hook";
import {
  useGetRecipeByIdQuery,
  useGetRecipeByIdQueryKey,
} from "../../hooks/useGetRecipeByIdQuery.hook";
import { useMutationFormState } from "../../../core/hooks/useMutationFormState.hook";
import { makeViewRecipeBookPath, makeViewRecipePath } from "../../route-utils";
import { LoadingFailedAlert } from "../../../core/components/loading-failed-alert/loading-failed-alert.component";
import { RecipeNotFoundBanner } from "../../components/recipe-not-found-banner/recipe-not-found-banner.component";
import Alert from "@mui/material/Alert";
import Button from "@mui/material/Button";
import { DeleteWithNameVerification } from "../../../core/components/delete-with-name-verification/delete-with-name-verification.component";
import { PageHeader } from "../../../core/components/page-header/page-header.component";
import { LoadingIndicator } from "../../../core/components/loading-indicator/loading-indicator.component";

export function DeleteRecipePageBody({ recipeId }: IDeleteRecipePageBodyProps) {
  const {
    reset: mutationReset,
    mutateAsync,
    isError: mutateIsError,
    isPending: mutateIsPending,
  } = useDeleteRecipeMutation();
  const recipeQueryKey = useGetRecipeByIdQueryKey(recipeId);
  const recipeQuery = useGetRecipeByIdQuery(recipeId, { alwaysFresh: true });
  const navigate = useNavigate();
  const { resetCount, resetState, concurrencyConflict, concurrencyToken } =
    useMutationFormState({
      concurrencyTokenProvider: (b) => b.versionTag,
      queryKey: recipeQueryKey,
      query: recipeQuery,
      onReset: () => mutationReset(),
    });

  const {
    isSuccess: queryIsSuccess,
    isPending: queryIsPending,
    isError: queryIsError,
    data: recipe,
  } = recipeQuery;

  const onDelete = async () => {
    if (!concurrencyToken || concurrencyConflict || !recipe) {
      return;
    }
    const bookId = recipe.bookId;
    await mutateAsync({
      id: recipeId,
      versionTag: concurrencyToken,
    });

    navigate(makeViewRecipeBookPath(bookId));
  };

  if (queryIsError) {
    return <LoadingFailedAlert />;
  }

  if (queryIsSuccess && !recipe) {
    return <RecipeNotFoundBanner />;
  }

  if (queryIsPending) {
    return <LoadingIndicator />;
  }

  if (queryIsSuccess && recipe && !recipe.mayEdit) {
    return (
      <Alert
        severity="error"
        variant="filled"
        action={
          <Button
            color="inherit"
            size="small"
            component={Link}
            to={makeViewRecipePath(recipe.bookId, recipe.id)}
          >
            View Recipe
          </Button>
        }
      >
        You may not delete this recipe
      </Alert>
    );
  }

  return (
    <>
      <PageHeader
        title="Confirm Permanent Recipe Deletion"
        subTitle="All deletions are final and can not be undone. Verify that this is the correct recipe before continuing."
      />
      <DeleteWithNameVerification
        entityType="Recipe"
        key={resetCount}
        name={recipe?.name ?? ""}
        uniqueKey={recipe?.id ?? ""}
        pending={mutateIsPending}
        showDeleteError={mutateIsError && !concurrencyConflict}
        onDelete={onDelete}
        onReset={resetState}
        showConcurrencyError={concurrencyConflict}
        description={recipe?.shortDescription}
      />
    </>
  );
}

export interface IDeleteRecipePageBodyProps {
  recipeId: string;
}
