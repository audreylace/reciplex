import { useNavigate } from "react-router";
import { ErrorBanner } from "../../../core/components/banner/banner.component";

/** banner that shows the use an error and clicking the button will reload the page */
export function RetryBannerComponent({
  message,
  buttonCaption,
}: {
  /** main message shown to the end user */
  message: string;
  /** action button caption. Default to `Try again?` */
  buttonCaption?: string;
}) {
  const navigate = useNavigate();
  return (
    <ErrorBanner
      title={"Problem"}
      message={message}
      icon="bi bi-bug"
      buttonCaption={
        <>
          <i className="bi bi-arrow-clockwise"></i>
          {buttonCaption ?? "Try again?"}
        </>
      }
      onButtonClick={() => {
        navigate(0);
      }}
    ></ErrorBanner>
  );
}
