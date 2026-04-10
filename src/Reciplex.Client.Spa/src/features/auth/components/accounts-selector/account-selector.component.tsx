import {
  useGetAccountsQuery,
  useResetAccountsQuery,
} from "../../hooks/useGetAccountsQuery.hook";
import Stack from "@mui/material/Stack";
import { LoadingFailedAlert } from "../../../core/components/loading-failed-alert/loading-failed-alert.component";
import { AddAccountButton } from "./add-account-button.component";
import { TopBanner } from "./top-banner.component";
import { ComponentSkeleton } from "./component-skeleton.component";
import { AccountCard } from "./account-card.component";
import { useEffect } from "preact/hooks";
import { useSignUpNavigate } from "../../hooks/useSignUpNavigate.hook";

export function AccountSelector({
  redirectToSignUpIfNeeded,
}: {
  /** when true, this component will redirect the user to the sing up page if they do not have any accounts */
  redirectToSignUpIfNeeded?: boolean;
}) {
  const accountQuery = useGetAccountsQuery();
  const goToSignUp = useSignUpNavigate()[1];
  const resetAccountsQuery = useResetAccountsQuery();

  const length = accountQuery.data?.length ?? 0;
  useEffect(() => {
    if (accountQuery.isSuccess && length <= 0 && redirectToSignUpIfNeeded) {
      goToSignUp({
        replace: true,
      });
    }
  }, [length, accountQuery.isSuccess, goToSignUp, redirectToSignUpIfNeeded]);

  if (accountQuery.status === "pending") {
    return <ComponentSkeleton />;
  }

  return (
    <>
      <TopBanner />
      <Stack spacing={2}>
        <LoadingFailedAlert
          show={accountQuery.status === "error"}
          onRetry={resetAccountsQuery}
        />
        {accountQuery.data && accountQuery.status === "success" && (
          <>
            <AddAccountButton />
            {accountQuery.data.map((acc) => (
              <AccountCard key={acc.userKey} account={acc} />
            ))}
            {accountQuery.data.length > 0 && <AddAccountButton />}
          </>
        )}
      </Stack>
    </>
  );
}
