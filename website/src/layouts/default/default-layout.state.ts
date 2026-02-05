import { useEffect } from "preact/hooks";
import { create } from "zustand";

export const useLayoutTitle = create<{
  title: string;
  clearToken: string;
  setTitle: (newValue: string, clearToken: string) => void;
  clearTitle: (clearToken: string) => void;
}>((set) => ({
  title: "",
  clearToken: "",
  setTitle: (title: string, clearToken: string) =>
    set((_) => ({ title, clearToken })),
  clearTitle: (clearToken: string) =>
    set((state) => {
      if (state.clearToken === clearToken && clearToken) {
        return {
          title: "",
          clearToken: "",
        };
      }
      return state;
    }),
}));

export function useSetTitle(title: string) {
  const setTitle = useLayoutTitle((state) => state.setTitle);
  const clearTitle = useLayoutTitle((state) => state.clearTitle);

  useEffect(() => {
    const array = new Uint8Array(10);
    self.crypto.getRandomValues(array);
    const s = (array as unknown as any).toBase64();
    setTitle(title, s);
    return () => {
      clearTitle(s);
    };
  }, [setTitle, clearTitle, title]);
}
