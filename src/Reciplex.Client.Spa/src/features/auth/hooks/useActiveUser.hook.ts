import { create } from "zustand";

const localStoreKey = "selectedUserKey";

export const useActiveUser = create<IUseActiveUser>((set) => ({
  userKey: null,
  challengeNeeded: undefined,
  synced: false,
  displayName: null,
  reset: () => {
    set((prev: IStateObject): IStateObject => {
      return {
        ...prev,
        synced: false,
        userKey: null,
        challengeNeeded: false,
        displayName: null,
      };
    });
  },
  setChallengeStatus: (flag: boolean) => {
    set((prev: IStateObject): IStateObject => {
      return {
        ...prev,
        challengeNeeded: flag,
        synced: true,
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
          synced: true,
          challengeNeeded: false,
        };
      } else {
        persistActiveUserId(args.userKey);
      }
      return {
        ...prev,
        userKey: args.userKey,
        displayName: args.displayName,
        challengeNeeded: false,
        synced: true,
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
  setChallengeStatus: (flag: boolean) => void;
  reset: () => void;
}

interface IStateObject {
  userKey: string | null;
  displayName: string | null;
  challengeNeeded: boolean | undefined;
  synced: boolean;
}

export interface ISetActiveUserArgs {
  userKey: string;
  displayName: string;
}

export function useActiveUserKey() {
  return useActiveUser((s) => s.userKey);
}
