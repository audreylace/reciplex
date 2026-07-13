import Button from "@mui/material/Button";
import { ContentTitle } from "../../../core/components/content-title/content-title.component";
import { ContentWrapper } from "../../../core/components/content-wrapper/content-wrapper.component";
import { PageHeader } from "../../../core/components/page-header/page-header.component";
import { useSignOutMutation } from "../../hooks/useSignOutMutation.hook";
import { OperationFailedAlert } from "../../../core/components/operation-failed-alert/operation-failed-alert.component";
import { BrowserTitle } from "../../../core/components/browser-title/browser-title.component";

/** body for the `SignOutPage` component */
export function SignOutPageBody() {
  const mutation = useSignOutMutation();
  return (
    <>
      <BrowserTitle title="Sign Out" />
      <PageHeader title="Signing Out" />
      {mutation.isError && (
        <OperationFailedAlert onRetry={() => mutation.reset()} />
      )}
      <ContentWrapper>
        <ContentTitle>Continue signing out of Reciplex?</ContentTitle>
        <Button
          variant="contained"
          size="large"
          loading={mutation.isPending}
          disabled={!mutation.isIdle}
          onClick={async () => {
            await mutation.mutateAsync({}).then(() => {});
            window.location.href = "/";
          }}
        >
          Sign Out
        </Button>
      </ContentWrapper>
    </>
  );
}
