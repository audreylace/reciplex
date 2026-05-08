import { useSelectAccountNavigate } from "../../hooks/useSelectAccountNavigate.hook";
import { useDeleteAccountMutation } from "../../hooks/useDeleteAccountMutation.hook";
import { LoadingFailedAlert } from "../../../core/components/loading-failed-alert/loading-failed-alert.component";
import { AccountNotFoundAlert } from "../../components/account-not-found-alert/account-not-found-alert.component";
import { useAccountMutationState } from "../../hooks/useAccountMutationState.hook";
import {
  useActiveUser,
  useActiveUserKey,
} from "../../hooks/useActiveUser.hook";
import { DeleteWithNameVerification } from "../../../core/components/delete-with-name-verification/delete-with-name-verification.component";
import { LoadingIndicator } from "../../../core/components/loading-indicator/loading-indicator.component";
import { PageHeader } from "../../../core/components/page-header/page-header.component";

/** body of the delete account page */
export function DeleteAccountPageBody({
  userKey,
}: IDeleteAccountPageBodyProps) {
  const {
    query,
    concurrencyConflict,
    account,
    concurrencyToken,
    resetState,
    resetCount,
  } = useAccountMutationState(userKey);
  const navigateSelectAccount = useSelectAccountNavigate()[1];
  const deleteMutation = useDeleteAccountMutation();

  const activeUserKey = useActiveUserKey();
  const resetActiveUser = useActiveUser((s) => s.reset);

  // reset the form by clearing the react-query cache
  const onReset = () => {
    deleteMutation.reset();
    resetState();
  };

  const onDelete = async () => {
    if (!concurrencyToken || concurrencyConflict) {
      return;
    }
    await deleteMutation.mutateAsync({ userKey, concurrencyToken });
    if (activeUserKey === userKey) {
      resetActiveUser();
    }

    navigateSelectAccount();
  };

  if (query.status === "error") {
    return <LoadingFailedAlert />;
  }

  if (query.isSuccess && !account) {
    return <AccountNotFoundAlert />;
  }

  if (query.isPending) {
    return <LoadingIndicator />;
  }

  return (
    <>
      <PageHeader
        title="Confirm Permanent Account Deletion"
        subTitle="Delete account including all recipes and books this account owns? This includes shared recipe and recipe books. Verify that this is the correct account before continuing. This is permanent and can not be undone."
      />
      <DeleteWithNameVerification
        entityType="Account"
        key={resetCount}
        name={account?.displayName ?? ""}
        uniqueKey={account?.userKey ?? ""}
        pending={deleteMutation.status === "pending"}
        showDeleteError={
          deleteMutation.status === "error" && !concurrencyConflict
        }
        onDelete={onDelete}
        onReset={onReset}
        showConcurrencyError={concurrencyConflict}
      />
    </>
  );
}

/** properties for delete account page body */
interface IDeleteAccountPageBodyProps {
  /** the account key */
  userKey: string;
}
