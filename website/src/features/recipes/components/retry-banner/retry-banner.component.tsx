import { useNavigate } from "react-router";
import styles from "./retry-banner.module.css";

export function RetryBannerComponent({ message }: { message: string }) {
  const navigate = useNavigate();
  return (
    <p>
      {message}{" "}
      <button
        className={styles.tryAgainButton}
        onClick={(event) => {
          event.preventDefault();
          navigate(0);
        }}
      >
        <i class="bi bi-arrow-clockwise"></i>
        Try again?
      </button>
    </p>
  );
}
