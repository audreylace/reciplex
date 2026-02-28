import type { UseQueryResult } from "@tanstack/react-query";
import type { ComponentChild } from "preact";
import { OfflineBanner } from "../banner/offline-banner.component";

export function MutationLoader<TData>({
  enabled,
  queryResult,
  notFound,
  onRender,
  fetchingBanner,
  failureBanner,
}: MutationLoaderProps<TData>) {
  enabled ??= true;
  if (!enabled) {
    return null;
  }

  switch (queryResult.status) {
    case "pending":
    case "success":
      if (queryResult.isFetchedAfterMount) {
        const data = queryResult.data;
        if (!data) {
          return notFound;
        }
        return onRender(data);
      }

      if (queryResult.isPaused) {
        return <OfflineBanner />;
      }

      if (typeof fetchingBanner === "function") {
        return fetchingBanner();
      }
      return fetchingBanner;
    case "error":
    default:
      return failureBanner;
  }
}

export interface MutationLoaderProps<TData> {
  enabled?: boolean;
  notFound: ComponentChild;
  onRender: (data: NonNullable<TData>) => React.ReactNode;
  fetchingBanner: preact.ComponentChildren;
  failureBanner: ComponentChild;
  queryResult: UseQueryResult<TData>;
}
