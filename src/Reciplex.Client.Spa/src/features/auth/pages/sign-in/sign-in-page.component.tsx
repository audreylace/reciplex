import { InformationBanner } from "../../../core/components/banner/banner.component";
import formCommonStyleModule from "../../../core/form-common/form-common.module.css";
import { useChallenge } from "../../hooks/useChallenge.hook";
import { useActiveUser } from "../../hooks/useActiveUser.hook";
import { AccountSelector } from "../../components/accounts-selector/account-selector.component";

export function SignInPage() {
  const isSynced = useActiveUser((s) => s.synced);
  const challengeNeeded = useActiveUser((s) => s.challengeNeeded);
  const startChallenge = useChallenge();

  return (
    <main className={formCommonStyleModule.formMain}>
      {!isSynced && <InformationBanner title="Checking Sign-In Status" />}
      {isSynced && (
        <>
          {challengeNeeded && (
            <InformationBanner
              title="Not Signed In"
              message="You need to sign-in to use this app"
              buttonCaption="Sign-In"
              onButtonClick={() => startChallenge()}
            />
          )}
          {!challengeNeeded && <AccountSelector />}
        </>
      )}
    </main>
  );
}
