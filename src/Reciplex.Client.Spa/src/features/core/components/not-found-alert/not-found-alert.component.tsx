import Button from "@mui/material/Button";
import Alert from "@mui/material/Alert";

/** Tells the user that the requested entity does not exist */
export function NotFoundAlert({
  show,
  href,
  caption,
  onClick,
}: ILoadingFailedAlertProps) {
  if (show === false) {
    return null;
  }

  return (
    <Alert
      severity="error"
      variant="filled"
      action={
        <Button color="inherit" size="small" href={href} onClick={onClick}>
          {caption}
        </Button>
      }
    >
      Not Found
    </Alert>
  );
}
/** properties for `LoadingFailedAlert` */
export interface ILoadingFailedAlertProps {
  /** flag controlling if the banner should render */
  show?: boolean | undefined | null;
  /** optional callback overriding the default action. Invoked when the user clicks the retry button.  */
  onClick?: () => void;
  href?: string;
  caption?: string;
}
