import { create } from "zustand";

const localStoreKey = "selectedUserKey";

export const useActiveUser = create<IUseActiveUser>((set) => ({
  userKey: null,
  displayName: null,
  reset: () => {
    set((prev: IStateObject): IStateObject => {
      return {
        ...prev,
        userKey: null,
        displayName: null,
      };
    });
  },
  setActiveUser: (args?: ISetActiveUserArgs) =>
    set((prev: IStateObject): IStateObject => {
      if (!args) {
        clearPersistedActiveUserId();
        return {
          ...prev,
          userKey: null,
          displayName: null,
        };
      } else {
        persistActiveUserId(args.userKey);
      }
      return {
        ...prev,
        userKey: args.userKey,
        displayName: args.displayName,
      };
    }),
}));

export function getPersistedActiveUser(): string | null {
  return localStorage.getItem(localStoreKey);
}
export function persistActiveUserId(id: string): void {
  localStorage.setItem(localStoreKey, id);
}
export function clearPersistedActiveUserId() {
  localStorage.removeItem(localStoreKey);
}
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
