import { RecipeBookNotFoundBanner } from "../../components/recipe-book-not-found-banner/recipe-book-not-found-banner.component";
import { useActiveUserKey } from "../../../auth/hooks/useActiveUser.hook";
import { LoadingFailedAlert } from "../../../core/components/loading-failed-alert/loading-failed-alert.component";
import { LoadingIndicator } from "../../../core/components/loading-indicator/loading-indicator.component";
import { PageHeader } from "../../../core/components/page-header/page-header.component";
import { useGetRecipeBookById } from "../../hooks/useGetRecipeBookById.hook";
import { useGetRecipeBookSharedAccessStatus } from "../../hooks/useGetRecipeBookSharedAccessStatus.hook";
import { RequestAccessToRecipeBookStatus } from "../../services/recipe-types";
import { RequestSubmittedBanner } from "./request-submitted-banner.component";
import { UserOwnsBook } from "./user-owns-book.component";
import { RequestGrantedBanner } from "./request-granted-banner.component";
import { WithdrawRequestBody } from "./withdraw-request-form.component";
import { RequestAccessBody } from "./request-access-body.component";
import { AssertString } from "../../../sentinel/stringUtilities";

/** body of the page for accepting invitations to a recipe book */
export function AcceptBookInvitePageBody({
  bookId,
  shareKey,
}: IAcceptBookInvitePageBodyProps) {
  const userKey = useActiveUserKey();
  const { data: bookData, isPending: bookIsPending } =
    useGetRecipeBookById(bookId);
  const {
    isError,
    isPending,
    data: shareAccessData,
  } = useGetRecipeBookSharedAccessStatus(bookId, shareKey);

  // Handle user requesting access to their own book
  // Disregard status of share access status request since it is not relevant.
  if (bookData && bookData.ownerId === userKey) {
    return <UserOwnsBook bookId={bookId} />;
  }

  // wait until both requests resolve. DO NOT
  // present edit dialog or error UI until we determine
  // if the user owns the book.
  if (isPending || bookIsPending) {
    return <LoadingIndicator />;
  }

  // MUST come after bookIsPending and user owns book check.
  // The get request for shared access will be in an error state
  // when user requests access to their own book.
  if (isError) {
    return <LoadingFailedAlert />;
  }

  if (
    !shareAccessData ||
    (shareAccessData.status ===
      RequestAccessToRecipeBookStatus.NoRequestInProgress &&
      !shareKey)
  ) {
    return <RecipeBookNotFoundBanner />;
  }

  let title = "";
  switch (shareAccessData.status) {
    case RequestAccessToRecipeBookStatus.Approved:
      title = "Leave Recipe Book";
      break;
    default:
    case RequestAccessToRecipeBookStatus.NoRequestInProgress:
      title = "Accept Invitation";
      break;
    case RequestAccessToRecipeBookStatus.Pending:
      title = " Cancel Request";
      break;
  }

  return (
    <>
      <PageHeader title={title} />

      {shareAccessData.status === RequestAccessToRecipeBookStatus.Pending && (
        <RequestSubmittedBanner />
      )}
      {shareAccessData.status === RequestAccessToRecipeBookStatus.Approved && (
        <RequestGrantedBanner bookId={bookId} />
      )}

      {shareAccessData.status ===
        RequestAccessToRecipeBookStatus.NoRequestInProgress && (
        <RequestAccessBody
          name={shareAccessData.name}
          shortDescription={shareAccessData.shortDescription}
          shareKey={AssertString(shareKey)}
          bookId={bookId}
        />
      )}

      {(shareAccessData.status === RequestAccessToRecipeBookStatus.Approved ||
        shareAccessData.status === RequestAccessToRecipeBookStatus.Pending) && (
        <WithdrawRequestBody
          name={shareAccessData.name}
          shortDescription={shareAccessData.shortDescription}
          shareKey={shareKey}
          bookId={bookId}
          requestApproved={
            shareAccessData.status === RequestAccessToRecipeBookStatus.Approved
          }
        />
      )}
    </>
  );
}

/** props for `<AcceptBookInvitePageBody />` */
export interface IAcceptBookInvitePageBodyProps {
  /** the book id */
  bookId: string;
  /** the book's share key */
  shareKey?: string;
}
