export function QueryStatusDispatch({
  loadingStatus,
  error,
  pending,
  success,
}: {
  /** overall status of the load */
  loadingStatus: LoadingStatusValues;
  pending:
    | preact.ComponentChildren
    | undefined
    | (() => preact.ComponentChildren | undefined);
  error:
    | preact.ComponentChildren
    | undefined
    | (() => preact.ComponentChildren | undefined);
  success:
    | preact.ComponentChildren
    | undefined
    | (() => preact.ComponentChildren | undefined);
}) {
  switch (loadingStatus) {
    case "error":
      return typeof error !== "function" ? error : error();
    case "pending":
      return typeof pending !== "function" ? pending : pending();
    case "success":
      return typeof success !== "function" ? success : success();
  }
}

export type LoadingStatusValues = "pending" | "error" | "success";
