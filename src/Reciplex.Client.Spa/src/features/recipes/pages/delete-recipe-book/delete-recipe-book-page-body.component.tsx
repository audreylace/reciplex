import { useNavigate } from "react-router";
import { DeleteWithNameVerification } from "../../../core/components/delete-with-name-verification/delete-with-name-verification.component";
import { useMutationFormState } from "../../../core/hooks/useMutationFormState.hook";
import { LoadingFailedAlert } from "../../../core/components/loading-failed-alert/loading-failed-alert.component";
import { RecipeBookNotFoundBanner } from "../../components/recipe-book-not-found-banner/recipe-book-not-found-banner.component";
import { useDeleteRecipeBookMutation } from "../../hooks/useDeleteRecipeBookMutation.hook";
import {
  useGetRecipeBookById,
  useGetRecipeBookByIdCacheKey,
} from "../../hooks/useGetRecipeBookById.hook";
import { makeBookListPath } from "../../route-utils";
import { BookMutationNotAuthorizedBanner } from "../../components/book-mutation-not-authorized-banner/book-mutation-not-authorized-banner.component";
import { PageHeader } from "../../../core/components/page-header/page-header.component";
import { LoadingIndicator } from "../../../core/components/loading-indicator/loading-indicator.component";
import { BookSettingsMenuButton } from "../../components/book-settings-menu/book-settings-menu-button.component";

/** body of the delete recipe book page */
export function DeleteRecipeBookPageBody({
  bookKey,
}: IDeleteRecipeBookPageBodyProps) {
  const deleteRecipeBookMutation = useDeleteRecipeBookMutation();
  const bookQueryKey = useGetRecipeBookByIdCacheKey(bookKey);
  const bookQuery = useGetRecipeBookById(bookKey, { alwaysFresh: true });
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
      <BookMutationNotAuthorizedBanner
        bookId={bookQuery.data.id}
        message="You may not delete this book"
      />
    );
  }

  if (bookQuery.isPending) {
    return <LoadingIndicator />;
  }

  return (
    <>
      <PageHeader
        title="Confirm Permanent Recipe Book Deletion"
        subTitle="This book and all of its recipes will be permanently deleted. Verify that this is the correct recipe book before continuing. Once confirmed, this action can not be undone."
        sideComponent={
          <BookSettingsMenuButton
            bookId={bookKey}
            mayEdit={bookQuery.data?.mayEdit ?? false}
            mayDelete={bookQuery.data?.mayDelete ?? false}
            mayManageShareAccess={bookQuery.data?.mayManageAccess ?? false}
          />
        }
      />
      <DeleteWithNameVerification
        entityType="Recipe Book"
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
    </>
  );
}

/** props for `DeleteRecipeBookPageBody` */
export interface IDeleteRecipeBookPageBodyProps {
  /** the key of the book to delete */
  bookKey: string;
}
