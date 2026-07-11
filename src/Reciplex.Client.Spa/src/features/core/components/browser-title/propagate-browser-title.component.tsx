import { useInternal_PropagateWindowTitle } from "./useSetBrowserTitle.hook";

/**
 * Framework component that reacts to title changes in the application.
 * Should only be one in the application.
 */
export function PropagateBrowserTitle() {
  useInternal_PropagateWindowTitle();
  return null;
}
