import { useNavigate } from "react-router";
import { FormButtons } from "../../../core/components/form-buttons/form-buttons.component";
import { PrimaryButton } from "../../../core/components/primary-button/primary-button.component";
import { SuccessButton } from "../../../core/components/success-button/success-button.component";

export function ActionFailedTryAgainCancel({
  cancelAction,
  cancelCaption,
  reloadAction,
  tryAgainCaption,
  message,
}: ActionFailedTryAgainCancelProps) {
  const navigate = useNavigate();
  reloadAction ??= () => navigate(0);
  tryAgainCaption ??= "Reload and try again?";
  return (
    <>
      <p>{message}</p>
      <FormButtons>
        <SuccessButton onClick={reloadAction}>{tryAgainCaption}</SuccessButton>
        <PrimaryButton onClick={cancelAction}>{cancelCaption}</PrimaryButton>
      </FormButtons>
    </>
  );
}

export interface ActionFailedTryAgainCancelProps {
  message: string;
  tryAgainCaption?: string;
  cancelCaption: string;
  reloadAction?: () => void;
  cancelAction: () => void;
}
