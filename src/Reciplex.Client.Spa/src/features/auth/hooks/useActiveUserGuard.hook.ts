import { useEffect, useState } from "preact/hooks";
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
  const [renderChildren, setRenderChildren] = useState(false);
  const accountListQueryReset = useResetAccountsQuery();

  // run as soon as we have data for minimal delay
  useEffect(() => {
    if (accountListQuery.isSuccess) {
      if (userKey) {
        const matchedAccount = accountListQuery.data.find(
          (acc) => acc.userKey === userKey,
        );

        if (matchedAccount) {
          setRenderChildren(true);
          return;
        }
      }

      if (accountListQuery.data.length === 1) {
        setActiveUser({
          userKey: accountListQuery.data[0].userKey,
          displayName: accountListQuery.data[0].displayName,
        });
        setRenderChildren(true);
        return;
      }
      resetUser();

      if (allowNullUser) {
        setRenderChildren(true);
        return;
      }

      setRenderChildren(false);
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
