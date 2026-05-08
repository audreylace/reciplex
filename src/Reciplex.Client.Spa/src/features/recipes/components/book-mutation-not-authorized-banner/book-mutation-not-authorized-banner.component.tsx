import Alert from "@mui/material/Alert";
import Button from "@mui/material/Button";
import { makeViewRecipeBookPath } from "../../route-utils";
import { Link } from "react-router";

/**
 * Banner shown to user when they
 * are not authorized to perform an action
 */
export function BookMutationNotAuthorizedBanner({
  bookId,
  message,
}: IBookNotAuthorizedBannerProps) {
  return (
    <Alert
      severity="error"
      variant="filled"
      action={
        <Button
          color="inherit"
          size="small"
          component={Link}
          to={makeViewRecipeBookPath(bookId)}
        >
          View Book
        </Button>
      }
    >
      {message}
    </Alert>
  );
}

/**
 * Properties for `BookNotAuthorizedBanner`
 * @see BookMutationNotAuthorizedBanner
 */
export interface IBookNotAuthorizedBannerProps {
  /** the message shown in the banner */
  message: string;
  /** the id of the book. Used to navigate the user back the book view page */
  bookId: string;
}
