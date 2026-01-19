import { useNavigate } from "react-router";

export function RetryBannerComponent({ message }: { message: string }) {
  const navigate = useNavigate();
  return (
    <p>
      {message}{" "}
      <a
        href="#"
        onClick={(event) => {
          event.preventDefault();
          navigate(0);
        }}
      >
        Try again?
      </a>
    </p>
  );
}
