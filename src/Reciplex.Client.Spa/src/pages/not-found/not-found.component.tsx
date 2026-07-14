import { useNavigate } from "react-router";
import { NotFoundAlert } from "../../features/core/components/not-found-alert/not-found-alert.component";
import { BrowserTitle } from "../../features/core/components/browser-title/browser-title.component";

export function NotFound() {
  const navigate = useNavigate();
  return (
    <>
      <BrowserTitle title="Not Found" />
      <NotFoundAlert
        href={"/"}
        onClick={() => navigate("/")}
        caption="Back to Reciplex"
      />
    </>
  );
}
