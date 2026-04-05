import { LoadingFailedAlert } from "../../../core/components/loading-failed-alert/loading-failed-alert.component";
import { AccountNotFoundAlert } from "../../components/account-not-found-alert/account-not-found-alert.component";
import {
  AccountSettingsForm,
  type IOnSaveData,
} from "../../components/account-settings-form/account-settings-form.component";
import { useAccountMutationState } from "../../hooks/useAccountMutationState.hook";
import {
  useActiveUser,
  useActiveUserKey,
} from "../../hooks/useActiveUser.hook";
import { useSelectAccountNavigate } from "../../hooks/useSelectAccountNavigate.hook";
import { useUpdateAccountMutation } from "../../hooks/useUpdateAccountMutation.hook";

/** body of the account settings page */
export function AccountSettingsPageBody({
  userKey,
}: IAccountSettingsPageBodyProps) {
  const {
    query,
    concurrencyConflict,
    account,
    concurrencyToken,
    resetState,
    resetCount,
  } = useAccountMutationState(userKey);
  const navigateSelectAccount = useSelectAccountNavigate()[1];
  const saveMutation = useUpdateAccountMutation();

  const activeUserKey = useActiveUserKey();
  const setActiveUser = useActiveUser((s) => s.setActiveUser);

  // reset the form by clearing the react-query cache
  const onReset = () => {
    saveMutation.reset();
    if (concurrencyConflict) {
      saveMutation.reset();
      resetState();
    }
  };

  const onSave = async (data: IOnSaveData) => {
    if (!concurrencyToken) {
      return;
    }
    const result = await saveMutation.mutateAsync({
      userKey: userKey,
      concurrencyToken,
      displayName: data.displayName,
    });

    // update app state cache if this user
    // is the active user.
    if (userKey === activeUserKey) {
      setActiveUser({
        userKey: result.userKey,
        displayName: result.displayName,
      });
    }

    navigateSelectAccount();
  };

  if (query.status === "error") {
    return <LoadingFailedAlert />;
  }

  if (query.isSuccess && !account) {
    return <AccountNotFoundAlert />;
  }

  return (
    <AccountSettingsForm
      showSkeleton={query.isPending}
      key={resetCount}
      displayName={account?.displayName ?? ""}
      userKey={account?.userKey ?? ""}
      pending={saveMutation.status === "pending"}
      showSaveError={saveMutation.status === "error" && !concurrencyConflict}
      onSave={onSave}
      onReset={onReset}
      showConcurrencyError={concurrencyConflict}
    />
  );
}

/** properties for delete account page body */
export interface IAccountSettingsPageBodyProps {
  /** the account key */
  userKey: string;
}
