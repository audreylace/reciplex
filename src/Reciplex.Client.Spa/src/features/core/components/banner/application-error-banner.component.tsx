import { ErrorBanner } from "./banner.component";

/**
 * Error showed when an application error is hit
 */
export function ApplicationErrorBanner() {
  return (
    <ErrorBanner
      icon="bi bi-bug"
      title="Problem"
      message="We have encountered a problem and need to restart the application."
      to="/"
      buttonCaption={
        <>
          <i className="bi bi-arrow-clockwise"></i> Restart and Go Home
        </>
      }
    />
  );
}
