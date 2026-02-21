import { useState, useCallback } from "preact/hooks";

/**
 * hook for reading and writing to local storage
 * @param key the local storage key to read and write from
 * @returns the value stored in local storage
 */
export function useLocalStorage(
  key: string,
): [string | null, (value: string | undefined | null) => void] {
  const [_, triggerRender] = useState<object>({}); // trigger render on local storage change

  const updater = useCallback(
    (value: string | undefined | null) => {
      if (value === null || typeof value === "undefined") {
        localStorage.removeItem(key);
      } else {
        localStorage.setItem(key, value);
      }
      triggerRender({});
    },
    [key],
  );

  return [localStorage.getItem(key), updater];
}
