import { useNavigate } from "react-router";
import { ActionFailedTryAgainCancel } from "../action-failed-try-again-cancel/action-failed-try-again-cancel.component";

/** error banner shown when loading a recipe fails */
export function FetchingRecipeFailedBanner() {
  const navigate = useNavigate();
  return (
    <ActionFailedTryAgainCancel
      message="Failed to fetch requested recipe"
      cancelCaption="Go Home"
      cancelAction={() => navigate("/")}
    />
  );
}
