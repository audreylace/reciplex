import { create } from "zustand";
import { persist, createJSONStorage } from "zustand/middleware";

export const useActiveUser = create<IUseActiveUser>()(
  persist(
    (set) => ({
      userKey: null,
      displayName: null,
      reset: () => set({ userKey: null, displayName: null }),
      setActiveUser: (args) => {
        if (!args) {
          set({ userKey: null, displayName: null });
        } else {
          set({ userKey: args.userKey, displayName: args.displayName });
        }
      },
    }),
    {
      name: "active-user-storage", // name of the item in localStorage
      storage: createJSONStorage(() => localStorage),
    },
  ),
);

export interface IUseActiveUser extends IStateObject {
  setActiveUser: (args?: ISetActiveUserArgs) => void;
  reset: () => void;
}

interface IStateObject {
  userKey: string | null;
  displayName: string | null;
}

export interface ISetActiveUserArgs {
  userKey: string;
  displayName: string;
}

export function useActiveUserKey() {
  return useActiveUser((s) => s.userKey);
}
