import Alert from "@mui/material/Alert";
import { LoadingFailedAlert } from "../../../core/components/loading-failed-alert/loading-failed-alert.component";
import { LoadingIndicator } from "../../../core/components/loading-indicator/loading-indicator.component";
import Button from "@mui/material/Button";
import { useChallenge } from "../../hooks/useChallenge.hook";
import { useNeedChallengeQuery } from "../../hooks/useNeedChallengeQuery.hook";
import type { ComponentChildren } from "preact";

/** guards a route refusing to render the component if a challenge action is required */
export function ChallengeGuard({ children }: { children: ComponentChildren }) {
  const challengeQuery = useNeedChallengeQuery();
  const startChallenge = useChallenge();

  return (
    <>
      <LoadingIndicator show={challengeQuery.isPending} />
      <LoadingFailedAlert show={challengeQuery.isError} />
      {challengeQuery.isSuccess && challengeQuery.data && (
        <Alert
          severity="info"
          variant="filled"
          action={
            <Button
              color="inherit"
              size="small"
              onClick={() => startChallenge()}
            >
              Sign-In
            </Button>
          }
        >
          You need to sign-in to use this app
        </Alert>
      )}
      {challengeQuery.isSuccess && !challengeQuery.data && children}
    </>
  );
}
