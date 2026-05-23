import Alert from "@mui/material/Alert";
import { LoadingFailedAlert } from "../../../core/components/loading-failed-alert/loading-failed-alert.component";
import { LoadingIndicator } from "../../../core/components/loading-indicator/loading-indicator.component";
import Button from "@mui/material/Button";
import { useChallenge } from "../../hooks/useChallenge.hook";
import { useNeedChallengeQuery } from "../../hooks/useNeedChallengeQuery.hook";

/** guards a route refusing to render the component if a challenge action is required */
export function ChallengeGuard({ children }: { children?: React.ReactNode }) {
  const challengeQuery = useNeedChallengeQuery();
  const startChallenge = useChallenge();

  if (challengeQuery.isPending) {
    return <LoadingIndicator />;
  }
  if (challengeQuery.isError) {
    return <LoadingFailedAlert />;
  }

  if (challengeQuery.isSuccess && challengeQuery.data) {
    return (
      <Alert
        severity="info"
        variant="filled"
        action={
          <Button color="inherit" size="small" onClick={() => startChallenge()}>
            Sign-In
          </Button>
        }
      >
        You need to sign-in to use this app
      </Alert>
    );
  }

  return <>{children}</>;
}
