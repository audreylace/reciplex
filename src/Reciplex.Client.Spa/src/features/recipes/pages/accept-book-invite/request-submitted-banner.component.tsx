import Alert from "@mui/material/Alert";
import { useNavigate } from "react-router";
import { makeBookListPath } from "../../route-utils";

/** Banner shown when a user has submitted a request to access a recipe but it has not yet been approved */
export function RequestSubmittedBanner() {
  const navigate = useNavigate();
  return (
    <Alert
      severity="info"
      variant="filled"
      onClick={() => {
        navigate(makeBookListPath());
      }}
    >
      Request submitted
    </Alert>
  );
}
