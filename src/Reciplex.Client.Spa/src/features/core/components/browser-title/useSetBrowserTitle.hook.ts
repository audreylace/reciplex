import { useEffect } from "react";
import { create } from "zustand";

/** root state object for browser title */
const useBrowserTitleState = create<IStateActions & IStateObject>()((set) => {
  return {
    title: "Reciplex",
    concurrencyToken: {},
    setTitle: (args) => {
      set({
        title: args.title + " - Reciplex",
        concurrencyToken: args.concurrencyToken,
      });
    },
    clearTitle: (args) => {
      set((prev) => {
        if (prev.concurrencyToken === args.concurrentToken) {
          return {
            title: "Reciplex",
            concurrencyToken: {},
          };
        }

        return prev;
      });
    },
  };
});

/** shape of the state data */
interface IStateObject {
  /** concurrency token, used to deal with out of order mounts-demounts */
  concurrencyToken: object;
  /** current application title */
  title: string;
}

/** state actions */
interface IStateActions {
  /**
   * sets the title
   * @param args action args
   */
  setTitle(args: { title: string; concurrencyToken: object }): void;
  /**
   * clears the title
   * @param args action args
   */
  clearTitle(args: { concurrentToken: object }): void;
}

/**
 * hook for setting the browser title
 * @param title the title of the browser
 */
export function useSetBrowserTitle(title: string) {
  const clearTitle = useBrowserTitleState((s) => s.clearTitle);
  const setTitle = useBrowserTitleState((s) => s.setTitle);
  useEffect(() => {
    const token = {};
    setTitle({ title, concurrencyToken: token });
    return () => {
      clearTitle({ concurrentToken: token });
    };
  }, [clearTitle, setTitle, title]);
}

/** Internal hook for propagating the current title to the browser window */
export function useInternal_PropagateWindowTitle() {
  const title = useBrowserTitleState((s) => s.title);
  useEffect(() => {
    window.document.title = title;
  }, [title]);
}
