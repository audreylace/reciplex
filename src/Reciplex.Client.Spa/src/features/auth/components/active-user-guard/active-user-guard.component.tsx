import type { ComponentChildren } from "preact";
import {
  getPersistedActiveUser,
  useActiveUser,
  useActiveUserKey,
} from "../../hooks/useActiveUser.hook";
import { useLayoutEffect, useState } from "preact/hooks";
import {
  useGetAccountsQuery,
  useResetAccountsQuery,
} from "../../hooks/useGetAccountsQuery.hook";
import { useSelectAccountNavigate } from "../../hooks/useSelectAccountNavigate.hook";
import { LoadingIndicator } from "../../../core/components/loading-indicator/loading-indicator.component";
import { LoadingFailedAlert } from "../../../core/components/loading-failed-alert/loading-failed-alert.component";

/**
 * Redirects the user to the select account if a account has not yet been selected.
 * Assumes user is authenticated.
 */
export function ActiveUserGuard({
  children,
  allowNullUser,
}: {
  allowNullUser?: boolean;
  children: ComponentChildren;
}) {
  const userKey = useActiveUserKey();
  const setActiveUser = useActiveUser((s) => s.setActiveUser);
  const accountListQuery = useGetAccountsQuery();
  const goToSelectAccount = useSelectAccountNavigate()[1];
  const [renderChildren, setRenderChildren] = useState(false);
  const accountListQueryReset = useResetAccountsQuery();

  // run as soon as we have data for minimal delay
  useLayoutEffect(() => {
    if (accountListQuery.isSuccess) {
      const activeKey = userKey ?? getPersistedActiveUser();

      if (activeKey) {
        const matchedAccount = accountListQuery.data.find(
          (acc) => acc.userKey === activeKey,
        );

        if (matchedAccount) {
          setActiveUser({
            userKey: activeKey,
            displayName: matchedAccount.displayName,
          });
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
    setActiveUser,
    userKey,
  ]);

  if (accountListQuery.status === "pending") {
    return <LoadingIndicator />;
  }

  if (accountListQuery.status === "error") {
    return <LoadingFailedAlert onRetry={accountListQueryReset} />;
  }

  if (!renderChildren) {
    return null;
  }

  return children;
}
