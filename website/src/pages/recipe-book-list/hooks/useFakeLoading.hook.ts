import { useState, useLayoutEffect } from "preact/hooks";
import type { BookListNavigationAction } from "../../../features/recipes/route-utils";

export function useFakeLoading(
  source: BookListNavigationAction | undefined,
  index: string | undefined,
) {
  const [fakeLoading, setFakeLoading] = useState(true);

  useLayoutEffect(() => {
    setFakeLoading(true);
    const handle = setTimeout(() => setFakeLoading(false), 400); // reduce flicker on page change
    return () => {
      clearTimeout(handle);
    };
  }, [setFakeLoading, source, index]);

  return fakeLoading;
}
