import Alert from "@mui/material/Alert";
import Button from "@mui/material/Button";
import { useNavigate } from "react-router";
import { makeViewRecipeBookPath } from "../../route-utils";
import { useEffect } from "react";

/** shows banner and redirects user to book page. Use when the user owns the book. */
export function UserOwnsBook({ bookId }: IUserOwnsBookProps) {
  const navigate = useNavigate();
  // jump user to book if they request access and they own it
  useEffect(() => {
    navigate(makeViewRecipeBookPath(bookId), { replace: true });
  }, [bookId, navigate]);

  // edge case - user requests their own book and the userEffect has not fired
  return (
    <Alert
      severity="info"
      variant="filled"
      action={
        <Button
          color="inherit"
          size="small"
          onClick={() => {
            navigate(makeViewRecipeBookPath(bookId));
          }}
        >
          View Book
        </Button>
      }
    >
      You own this book
    </Alert>
  );
}

/** props for `<UserOwnsBook />` */
export interface IUserOwnsBookProps {
  /** the id of the book */
  bookId: string;
}
