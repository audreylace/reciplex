import { ErrorBanner } from "../../../core/components/banner/banner.component";

/**
 * Error showed when a component
 * somehow is activated with path parameters
 * that should not be possible.
 */
export function BadPathBanner() {
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
