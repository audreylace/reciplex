import Button from "@mui/material/Button";
import Alert from "@mui/material/Alert";
import { useRefreshPage } from "../../hooks/useRefreshPage.hook";

/** Tells the user that the load failed and gives them an option to refresh the page */
export function LoadingFailedAlert({
  show,
  onRetry,
}: ILoadingFailedAlertProps) {
  const [thisPagePath, refreshPageAction] = useRefreshPage();

  if (show === false) {
    return null;
  }

  return (
    <Alert
      severity="error"
      variant="filled"
      action={
        <Button
          color="inherit"
          size="small"
          href={thisPagePath}
          onClick={onRetry ?? refreshPageAction}
        >
          Retry
        </Button>
      }
    >
      Loading Failed
    </Alert>
  );
}
/** properties for `LoadingFailedAlert` */
export interface ILoadingFailedAlertProps {
  /** flag controlling if the banner should render */
  show?: boolean | undefined | null;
  /** optional callback overriding the default action. Invoked when the user clicks the retry button.  */
  onRetry?: () => void;
}
