import { ApplicationErrorBanner } from "../banner/application-error-banner.component";

export function QueryStatusDispatch({
  loadingStatus,
  error,
  pending,
  success,
}: {
  /** overall status of the load */
  loadingStatus: LoadingStatusValues;
  pending: preact.ComponentChildren;
  error: preact.ComponentChildren;
  success: preact.ComponentChildren;
}) {
  switch (loadingStatus) {
    case "error":
      return error;
    case "pending":
      return pending;
    case "success":
      return success;
    default:
      return <ApplicationErrorBanner />;
  }
}

export type LoadingStatusValues = "pending" | "error" | "success";
