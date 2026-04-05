import { useQueryClient } from "@tanstack/react-query";
import { useSelectAccountNavigate } from "../../hooks/useSelectAccountNavigate.hook";
import { useDeleteAccountMutation } from "../../hooks/useDeleteAccountMutation.hook";
import {
  getAccountsQueryKey,
  useGetAccountsQuery,
} from "../../hooks/useGetAccountsQuery.hook";
import { useMemo, useState } from "preact/hooks";
import { LoadingFailedAlert } from "../../../core/components/loading-failed-alert/loading-failed-alert.component";
import { DeleteAccountForm } from "../../components/delete-account-form/delete-account-form.component";
import { AccountNotFoundAlert } from "../../components/account-not-found-alert/account-not-found-alert.component";

/** body of the delete account page */
export function DeleteAccountPageBody({
  userKey,
}: IDeleteAccountPageBodyProps) {
  const queryClient = useQueryClient();
  const navigateSelectAccount = useSelectAccountNavigate()[1];
  const deleteMutation = useDeleteAccountMutation();
  const accountsQuery = useGetAccountsQuery(undefined, { noCache: true });
  const [concurrencyToken, setConcurrencyToken] = useState<string | null>();

  // component key - incremented each time we reset the form
  // to recycle the component
  const [formKey, setFormKey] = useState(1);

  // extract account from the list
  const account = useMemo(() => {
    if (accountsQuery.isFetchedAfterMount) {
      return accountsQuery.data?.find((acc) => acc.userKey === userKey) ?? null;
    }
    return null;
  }, [accountsQuery.data, accountsQuery.isFetchedAfterMount, userKey]);

  // reset the form by clearing the react-query cache
  const onReset = () => {
    queryClient.resetQueries({ queryKey: getAccountsQueryKey });
    setConcurrencyToken(null);
    setFormKey((k) => k + 1);
  };

  const onDelete = async () => {
    if (!concurrencyToken) {
      return;
    }
    await deleteMutation.mutateAsync({ userKey, concurrencyToken });
    navigateSelectAccount();
  };

  // lock concurrency token to detect concurrent modifications
  setConcurrencyToken((token) => {
    if (!account || token) {
      return token;
    }
    return account.concurrencyTag;
  });

  if (accountsQuery.status === "error") {
    return <LoadingFailedAlert />;
  }

  if (accountsQuery.isFetchedAfterMount && !account) {
    return <AccountNotFoundAlert />;
  }

  return (
    <DeleteAccountForm
      showSkeleton={
        accountsQuery.isPending || !accountsQuery.isFetchedAfterMount
      }
      key={formKey}
      displayName={account?.displayName ?? ""}
      userKey={account?.userKey ?? ""}
      pending={deleteMutation.status === "pending"}
      showDeleteError={deleteMutation.status === "error"}
      onDelete={onDelete}
      onReset={onReset}
      showConcurrencyError={
        !!(
          concurrencyToken &&
          account &&
          concurrencyToken !== account.concurrencyTag
        )
      }
    />
  );
}

/** properties for delete account page body */
interface IDeleteAccountPageBodyProps {
  /** the account key */
  userKey: string;
}
