import Button from "@mui/material/Button";
import Alert from "@mui/material/Alert";

/** Tells the user that the requested entity does not exist */
export function NotFoundAlert({
  show,
  href,
  caption,
  onClick,
  entityName,
}: INotFoundAlertProps) {
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
          href={href}
          onClick={(e) => {
            if (onClick) {
              e.preventDefault();
              onClick();
            }
          }}
        >
          {caption}
        </Button>
      }
    >
      {entityName} Not Found
    </Alert>
  );
}
/** properties for `NotFoundAlert` */
export interface INotFoundAlertProps {
  /** flag controlling if the banner should render */
  show?: boolean | undefined | null;
  /**
   * optional callback overriding the default
   * action. Invoked when the user clicks the retry button.
   */
  onClick?: () => void;
  /** the path the button will navigate the user to */
  href?: string;
  /** the button caption */
  caption?: string;
  entityName?: string;
}
