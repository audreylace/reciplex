import { QueryClient } from "@tanstack/react-query";

export const queryClientStaleTime = 1000 * 60;
export const queryClientGcTime = 5 * 1000 * 60;
export function makeClient() {
  return new QueryClient({
    defaultOptions: {
      queries: {
        staleTime: queryClientStaleTime,
        gcTime: queryClientGcTime,
        retry: 2,
      },
    },
  });
}
