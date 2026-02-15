import { useNavigate } from "react-router";
import { PrimaryButton } from "../../../core/components/primary-button/primary-button.component";

export function RetryBannerComponent({
  message,
  buttonCaption,
}: {
  message: string;
  buttonCaption?: string;
}) {
  const navigate = useNavigate();
  return (
    <p>
      {message}{" "}
      <PrimaryButton
        onClick={(event) => {
          event.preventDefault();
          navigate(0);
        }}
      >
        <i className="bi bi-arrow-clockwise"></i>
        {buttonCaption ?? "Try again?"}
      </PrimaryButton>
    </p>
  );
}
