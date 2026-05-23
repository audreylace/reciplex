import { useDeleteAccessToBookMutation } from "../../hooks/useDeleteAccessToBookMutation.hook";
import { InvitePageBodyTemplate } from "./invite-form-template.component";

/**
 * Body for withdrawing a access request to a book whether approved or not. Also main
 * ui for users to leave a book.
 */
export function WithdrawRequestBody({
  name,
  shortDescription,
  bookId,
  requestApproved,
  shareKey,
}: IWithdrawRequestBodyProps) {
  const mutation = useDeleteAccessToBookMutation();

  const title = requestApproved
    ? "Leaving recipe book"
    : "Canceling recipe book access request";

  const buttonCaption = requestApproved ? "Leave Book" : "Cancel Request";

  return (
    <InvitePageBodyTemplate
      name={name}
      shortDescription={shortDescription}
      title={title}
      buttonCaption={buttonCaption}
      buttonDisabled={!mutation.isIdle}
      showErrorBanner={mutation.isError}
      showLoadingIndicator={mutation.isPending}
      onClick={() => {
        if (mutation.isIdle) {
          mutation.mutateAsync({ bookId, shareKey });
        }
      }}
    />
  );
}

/** props for `<WithdrawRequestBody />` */
export interface IWithdrawRequestBodyProps {
  /** name of the book */
  name: string;
  /** book short description */
  shortDescription: string;
  /** id of the book */
  bookId: string;
  /** if the user's request has been approved */
  requestApproved?: boolean;
  /** the share key if known. Helps manage the cache. */
  shareKey?: string;
}
