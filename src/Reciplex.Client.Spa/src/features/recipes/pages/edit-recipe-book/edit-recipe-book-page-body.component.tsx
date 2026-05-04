import { Link, useNavigate } from "react-router";
import { makeViewRecipeBookPath } from "../../route-utils";
import { RecipeBookNotFoundBanner } from "../../components/recipe-book-not-found-banner/recipe-book-not-found-banner.component";
import {
  useGetRecipeBookById,
  useGetRecipeBookByIdCacheKey,
} from "../../hooks/useGetRecipeBookById.hook";
import { useMutationFormState } from "../../../core/hooks/useMutationFormState.hook";
import { useUpdateRecipeBookMutation } from "../../hooks/useUpdateRecipeBookMutation.hook";
import { LoadingFailedAlert } from "../../../core/components/loading-failed-alert/loading-failed-alert.component";
import { NameAndShortDescriptionForm } from "../../components/name-and-short-description-form/name-and-short-description-form.component";
import { BookMutationNotAuthorizedBanner } from "../../components/book-mutation-not-authorized-banner/book-mutation-not-authorized-banner.component";
import Typography from "@mui/material/Typography";
import Stack from "@mui/material/Stack";

export function EditRecipeBookPageBody({ bookId }: { bookId: string }) {
  const navigate = useNavigate();
  const {
    reset: mutationReset,
    isError: mutationError,
    isPending: mutationPending,
    mutateAsync,
  } = useUpdateRecipeBookMutation();
  const bookQueryKey = useGetRecipeBookByIdCacheKey(bookId);
  const bookQuery = useGetRecipeBookById(bookId, { alwaysFresh: true });
  const { resetCount, resetState, concurrencyConflict, concurrencyToken } =
    useMutationFormState({
      concurrencyTokenProvider: (b) => b.versionTag,
      queryKey: bookQueryKey,
      query: bookQuery,
      onReset: () => mutationReset(),
    });

  if (bookQuery.status === "error") {
    return <LoadingFailedAlert />;
  }

  if (bookQuery.isSuccess && !bookQuery.data) {
    return <RecipeBookNotFoundBanner />;
  }

  if (bookQuery.isSuccess && bookQuery.data && !bookQuery.data.mayDelete) {
    return (
      <BookMutationNotAuthorizedBanner
        bookId={bookQuery.data.id}
        message="You may not edit  this book"
      />
    );
  }

  return (
    <>
      <Typography variant="h2" sx={{ mb: 2 }}>
        <Stack direction={"row"} gap={2}>
          Editing Recipe Book
        </Stack>
      </Typography>
      <NameAndShortDescriptionForm
        key={resetCount}
        legendText={`Modify recipe book title and description`}
        nameLabel="Book Title"
        nameHelpText="Title of the recipe book"
        nameMaxLength={128}
        shortDescriptionHelpText="Concise description of the book's content or purpose"
        shortDescriptionLabel="Book Short Description"
        shortDescriptionMaxLength={256}
        onSuccess={async (data) => {
          if (!concurrencyToken || concurrencyConflict) {
            return;
          }
          await mutateAsync({
            recipeBookId: bookId,
            name: data.name,
            shortDescription: data.shortDescription,
            versionTag: concurrencyToken,
          });
          navigate(makeViewRecipeBookPath(bookId));
        }}
        onReset={() => {
          if (concurrencyConflict) {
            resetState();
          } else {
            mutationReset();
          }
        }}
        pending={mutationPending}
        showError={mutationError}
        submitText="Save"
        showSkeleton={bookQuery.isPending}
        showConflict={concurrencyConflict}
        values={{
          name: bookQuery.data?.name ?? "",
          shortDescription: bookQuery.data?.shortDescription ?? "",
        }}
      />
    </>
  );
}
