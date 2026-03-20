import { useState } from "preact/hooks";
import { ApplicationErrorBanner } from "../../../core/components/banner/application-error-banner.component";
import { InformationBanner } from "../../../core/components/banner/banner.component";
import { DangerButton } from "../../../core/components/buttons/danger-button.component";
import { useGetAccountsQuery } from "../../hooks/useGetAccountsQuery.hook";
import accountListStylesModule from "./account-selector.module.css";
import { SignUpForm } from "../sign-up-form/sign-up-form.component";
import { useNavigate } from "react-router";
import { PrimaryButton } from "../../../core/components/buttons/primary-button.component";
import { SuccessButton } from "../../../core/components/buttons/success-button.component";
import { makeBookListPath } from "../../../recipes/route-utils";
import { useActiveUser } from "../../hooks/useActiveUser.hook";
import type { IHttpUserJson } from "../../http-clients/users-http-client";

export function AccountSelector() {
  const accountQuery = useGetAccountsQuery();
  const [manualAddAccount, setManualAddAccount] = useState(false);

  switch (accountQuery.status) {
    case "error":
    default:
      return <ApplicationErrorBanner />;

    case "pending":
      return <InformationBanner title="Fetching your accounts" />;

    case "success":
      if (
        accountQuery.data &&
        accountQuery.data.length > 0 &&
        !manualAddAccount
      ) {
        return (
          <>
            <h1>Whose Cooking?</h1>
            <div className={accountListStylesModule.listWrapper}>
              {accountQuery.data.map((acc) => (
                <SelectAccountButton account={acc} key={acc.userKey} />
              ))}
            </div>
            <DangerButton onClick={() => setManualAddAccount(true)}>
              Add Account
            </DangerButton>
          </>
        );
      }

      return (
        <SignUpForm
          cancel={
            manualAddAccount ? () => setManualAddAccount(false) : undefined
          }
        />
      );
  }
}

/**
 * Button to select an account
 */
function SelectAccountButton({ account }: { account: IHttpUserJson }) {
  const setActiveUser = useActiveUser((s) => s.setActiveUser);
  const userKey = useActiveUser((s) => s.userKey);

  const navigate = useNavigate();
  const handleClick = () => {
    setActiveUser({
      userKey: account.userKey,
      displayName: account.displayName,
    });
    navigate(makeBookListPath());
  };

  const isActive = userKey == account.userKey;
  const Button = isActive ? PrimaryButton : SuccessButton;

  return (
    <Button className={accountListStylesModule.button} onClick={handleClick}>
      <h3 className={accountListStylesModule.header}>
        {account.displayName} {userKey === account.userKey ? "(active)" : ""}
      </h3>
    </Button>
  );
}
