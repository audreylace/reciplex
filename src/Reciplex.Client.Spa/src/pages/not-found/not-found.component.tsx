import { useNavigate } from "react-router";
import { NotFoundAlert } from "../../features/core/components/not-found-alert/not-found-alert.component";

export function NotFound() {
  const navigate = useNavigate();
  return (
    <NotFoundAlert
      href={"/"}
      onClick={() => navigate("/")}
      caption="Back to Reciplex"
    />
  );
}
