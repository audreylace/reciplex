import { useNavigate } from "react-router";
import { DeleteWithNameVerification } from "../../../core/components/delete-with-name-verification/delete-with-name-verification.component";
import { useMutationFormState } from "../../../core/hooks/useMutationFormState.hook";
import { LoadingFailedAlert } from "../../../core/components/loading-failed-alert/loading-failed-alert.component";
import { RecipeBookNotFoundBanner } from "../../components/book-banners/recipe-book-not-found-banner.component";
import { useDeleteRecipeBookMutation } from "../../hooks/useDeleteRecipeBookMutation.hook";
import {
  useGetRecipeBookById,
  useGetRecipeBookByIdCacheKey,
} from "../../hooks/useGetRecipeBookById.hook";
import { makeBookListPath, makeViewRecipeBookPath } from "../../route-utils";
import Alert from "@mui/material/Alert";
import Button from "@mui/material/Button";

/** body of the delete recipe book page */
export function DeleteRecipeBookPageBody({
  bookKey,
}: IDeleteRecipeBookPageBodyProps) {
  const deleteRecipeBookMutation = useDeleteRecipeBookMutation();
  const bookQueryKey = useGetRecipeBookByIdCacheKey(bookKey);
  const bookQuery = useGetRecipeBookById(bookKey, { noCache: true });
  const navigate = useNavigate();
  const { resetCount, resetState, concurrencyConflict, concurrencyToken } =
    useMutationFormState({
      concurrencyTokenProvider: (b) => b.versionTag,
      queryKey: bookQueryKey,
      query: bookQuery,
      onReset: () => deleteRecipeBookMutation.reset(),
    });

  const onDelete = async () => {
    if (!concurrencyToken || concurrencyConflict) {
      return;
    }
    await deleteRecipeBookMutation.mutateAsync({
      bookId: bookKey,
      versionTag: concurrencyToken,
    });

    navigate(makeBookListPath());
  };

  if (bookQuery.status === "error") {
    return <LoadingFailedAlert />;
  }

  if (bookQuery.isSuccess && !bookQuery.data) {
    return <RecipeBookNotFoundBanner />;
  }

  if (bookQuery.isSuccess && bookQuery.data && !bookQuery.data.mayDelete) {
    return (
      <Alert
        severity="error"
        variant="filled"
        action={
          <Button
            color="inherit"
            size="small"
            href={makeViewRecipeBookPath(bookKey)}
            onClick={() => {
              navigate(makeViewRecipeBookPath(bookKey));
            }}
          >
            View Book
          </Button>
        }
      >
        You may not delete this book
      </Alert>
    );
  }

  return (
    <DeleteWithNameVerification
      entityType="Recipe Book"
      showSkeleton={bookQuery.isPending}
      key={resetCount}
      name={bookQuery.data?.name ?? ""}
      uniqueKey={bookQuery.data?.id ?? ""}
      pending={deleteRecipeBookMutation.status === "pending"}
      showDeleteError={
        deleteRecipeBookMutation.status === "error" && !concurrencyConflict
      }
      onDelete={onDelete}
      onReset={resetState}
      showConcurrencyError={concurrencyConflict}
      description={bookQuery.data?.shortDescription}
    />
  );
}

/** props for `DeleteRecipeBookPageBody` */
export interface IDeleteRecipeBookPageBodyProps {
  /** the key of the book to delete */
  bookKey: string;
}
