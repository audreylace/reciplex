import { useEffect } from "react";
import { useActiveUser, useActiveUserKey } from "./useActiveUser.hook";
import {
  useGetAccountsQuery,
  useResetAccountsQuery,
} from "./useGetAccountsQuery.hook";
import { useSelectAccountNavigate } from "./useSelectAccountNavigate.hook";

export function useActiveUserGuard(allowNullUser?: boolean) {
  const userKey = useActiveUserKey();
  const setActiveUser = useActiveUser((s) => s.setActiveUser);
  const resetUser = useActiveUser((s) => s.reset);
  const accountListQuery = useGetAccountsQuery();
  const goToSelectAccount = useSelectAccountNavigate()[1];
  const accountListQueryReset = useResetAccountsQuery();

  let renderChildren = allowNullUser;
  if (!renderChildren && accountListQuery.isSuccess && userKey) {
    const matchedAccount = accountListQuery.data.find(
      (acc) => acc.userKey === userKey,
    );

    if (matchedAccount) {
      renderChildren = true;
    }
  }

  // run as soon as we have data for minimal delay
  useEffect(() => {
    if (accountListQuery.isSuccess) {
      if (userKey) {
        const matchedAccount = accountListQuery.data.find(
          (acc) => acc.userKey === userKey,
        );

        if (matchedAccount) {
          return;
        }
      }

      if (accountListQuery.data.length === 1) {
        setActiveUser({
          userKey: accountListQuery.data[0].userKey,
          displayName: accountListQuery.data[0].displayName,
        });

        return;
      }
      resetUser();

      if (allowNullUser) {
        return;
      }

      goToSelectAccount({ replace: true });
    }
  }, [
    accountListQuery.data,
    accountListQuery.isSuccess,
    allowNullUser,
    goToSelectAccount,
    resetUser,
    setActiveUser,
    userKey,
  ]);

  return {
    renderChildren,
    status: accountListQuery.status,
    retry: accountListQueryReset,
  };
}
