import type { FetchStatus } from "@tanstack/react-query";
import { ApplicationErrorBanner } from "../banner/application-error-banner.component";

/**
 * Dispatches the fetching status of a react query to the appropriate UI
 */
export function FetchingStatusDispatch({
  fetchStatus,
  fetching,
  idle,
  paused,
}: {
  /** the status of the fetch */
  fetchStatus: FetchStatus;
  fetching: preact.ComponentChildren | undefined;
  idle: preact.ComponentChildren | undefined;
  paused: preact.ComponentChildren | undefined;
}) {
  switch (fetchStatus) {
    case "fetching":
      return fetching;

    case "idle":
      return idle;

    case "paused":
      return paused;

    default:
      return <ApplicationErrorBanner />;
  }
}
