import Alert from "@mui/material/Alert";
import Button from "@mui/material/Button";
import { useNavigate } from "react-router";
import { makeViewRecipeBookPath } from "../../route-utils";

export function RequestGrantedBanner({ bookId }: IRequestGrantedBannerProps) {
  const navigate = useNavigate();
  return (
    <Alert
      severity="success"
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
      Request granted
    </Alert>
  );
}

/** props for `<RequestGrantedBanner />` */
export interface IRequestGrantedBannerProps {
  /** the book id */
  bookId: string;
}
