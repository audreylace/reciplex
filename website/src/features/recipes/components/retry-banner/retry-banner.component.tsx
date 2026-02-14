import { useNavigate } from "react-router";
import { PrimaryButton } from "../../../core/primary-button/primary-button.component";

export function RetryBannerComponent({ message }: { message: string }) {
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
        <i class="bi bi-arrow-clockwise"></i>
        Try again?
      </PrimaryButton>
    </p>
  );
}
