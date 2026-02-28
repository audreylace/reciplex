import { useNavigate } from "react-router";
import { FormButtons } from "../../../core/components/form-buttons/form-buttons.component";
import { PrimaryButton } from "../../../core/components/buttons/primary-button.component";
import { SuccessButton } from "../../../core/components/buttons/success-button.component";
import { ErrorBanner } from "../../../core/components/banner/banner.component";

export function ActionFailedTryAgainCancel({
  cancelAction,
  cancelCaption,
  reloadAction,
  tryAgainCaption,
  message,
}: ActionFailedTryAgainCancelProps) {
  const navigate = useNavigate();
  reloadAction ??= () => navigate(0);
  tryAgainCaption ??= "Try Again";
  return (
    <ErrorBanner title="Problem" message={message} icon="bi bi-bug">
      <FormButtons notInForm>
        <SuccessButton onClick={reloadAction}>{tryAgainCaption}</SuccessButton>
        <PrimaryButton onClick={cancelAction}>{cancelCaption}</PrimaryButton>
      </FormButtons>
    </ErrorBanner>
  );
}

export interface ActionFailedTryAgainCancelProps {
  message: string;
  tryAgainCaption?: string;
  cancelCaption: string;
  reloadAction?: () => void;
  cancelAction: () => void;
}
