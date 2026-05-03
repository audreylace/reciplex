import { NotFoundAlert } from "../../../core/components/not-found-alert/not-found-alert.component";
import {
  useGetRecipeByIdQuery,
  useGetRecipeByIdQueryKey,
} from "../../hooks/useGetRecipeByIdQuery.hook";
import { useMutationFormState } from "../../../core/hooks/useMutationFormState.hook";
import { LoadingFailedAlert } from "../../../core/components/loading-failed-alert/loading-failed-alert.component";
import { LoadingIndicator } from "../../../core/components/loading-indicator/loading-indicator.component";
import Alert from "@mui/material/Alert";
import { EditRecipeForm } from "../../components/edit-recipe-form/edit-recipe-form.component";
import { useNavigate } from "react-router";
import { makeViewRecipePath } from "../../route-utils";

export function EditRecipePageBody({ recipeId }: { recipeId: string }) {
  const recipeQueryKey = useGetRecipeByIdQueryKey(recipeId);
  const recipeQuery = useGetRecipeByIdQuery(recipeId, { alwaysFresh: true });
  const navigate = useNavigate();

  const { resetState, concurrencyConflict, concurrencyToken } =
    useMutationFormState({
      concurrencyTokenProvider: (b) => b.versionTag,
      queryKey: recipeQueryKey,
      query: recipeQuery,
    });

  const {
    isSuccess: isLoadSuccess,
    isPending: isLoadPending,
    data: recipe,
  } = recipeQuery;

  if (isLoadSuccess) {
    if (!recipe || !concurrencyToken) {
      return <NotFoundAlert />;
    }
    if (!recipe.mayEdit) {
      return <Alert>May not edit this recipe</Alert>;
    }
    return (
      <EditRecipeForm
        concurrencyConflict={concurrencyConflict}
        onReset={() => {
          resetState();
        }}
        concurrencyToken={concurrencyToken}
        data={recipe}
        onCancel={() => navigate(makeViewRecipePath(recipe.bookId, recipe.id))}
      />
    );
  }

  if (isLoadPending) {
    return <LoadingIndicator />;
  }

  return <LoadingFailedAlert />;
}
