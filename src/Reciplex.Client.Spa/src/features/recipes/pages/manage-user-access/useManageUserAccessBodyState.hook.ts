import { useEffect, useState } from "preact/hooks";
import type {
  IRecipeBookUserPermissionsJsonRequest,
  IRecipeBookUserPermissionsJsonResponse,
} from "../../services/recipe-types";
import type { AccessControlsButtonsValues } from "./access-controls-row.component";

/** single state entry */
export interface IManageUserAccessBodyStateEntry {
  /** the previous permissions sent from the server */
  prev: IRecipeBookUserPermissionsJsonResponse;
  /** the new update to send to the server */
  new: IRecipeBookUserPermissionsJsonRequest | null;
}

/**
 * hook for managing user access state
 * @param accessListData the access list data. Will
 * load this into the hook's state the first time
 * it is not null or undefined.
 */
export function useManageUserAccessBodyState(
  accessListData: IRecipeBookUserPermissionsJsonResponse[] | null | undefined,
): IUseManageUserAccessBodyStateReturn {
  const [state, setState] = useState<IManageUserAccessBodyStateEntry[] | null>(
    null,
  );

  useEffect(() => {
    setState((prev) => hydrateAccessBodyState(prev, accessListData));
  }, [accessListData]);

  return {
    reset: () => setState(null),
    state,
    buildResponse: () => {
      return state?.map((v) => [v.prev.userKey, v.new]) ?? [];
    },
    updateUserStateEntry: (
      userKey: string,
      selectedValue: AccessControlsButtonsValues,
    ) => {
      setState((prev) =>
        prev ? updateStateEntry(prev, userKey, selectedValue) : prev,
      );
    },
  };
}

/** return from @see useManageUserAccessBodyState */
export interface IUseManageUserAccessBodyStateReturn {
  /** clears existing state from the hook */
  reset: () => void;
  /** current view of the state */
  state: IManageUserAccessBodyStateEntry[] | null;
  /**
   *  mutates a state entry
   * @param userKey the user to mutate
   * @param selectedValue the new state of the entry
   */
  updateUserStateEntry: (
    userKey: string,
    selectedValue: AccessControlsButtonsValues,
  ) => void;
  buildResponse: () => [
    string,
    IRecipeBookUserPermissionsJsonRequest | undefined | null,
  ][];
}

function hydrateAccessBodyState(
  prev: IManageUserAccessBodyStateEntry[] | null,
  accessListData: IRecipeBookUserPermissionsJsonResponse[] | undefined | null,
) {
  if (prev || !accessListData) {
    return prev;
  }

  return accessListData.map((responseJsonEntry) => {
    return {
      prev: responseJsonEntry,
      new: {
        reviewed: responseJsonEntry.reviewed,
        mayEditBook: responseJsonEntry.mayEditBook,
        mayViewBook: responseJsonEntry.mayViewBook,
      },
    };
  });
}

function updateStateEntry(
  state: IManageUserAccessBodyStateEntry[],
  userKey: string,
  selectedValue: AccessControlsButtonsValues,
): IManageUserAccessBodyStateEntry[] {
  const selectedIndex = state.findIndex(
    (entry) => entry.prev.userKey === userKey,
  );
  if (selectedIndex === -1) {
    return state;
  }

  return state.map((stateEntry) => {
    if (stateEntry.prev.userKey !== userKey) {
      return stateEntry;
    }

    let request: IRecipeBookUserPermissionsJsonRequest | null;
    switch (selectedValue) {
      case "none":
        request = {
          mayEditBook: false,
          mayViewBook: false,
          reviewed: true,
        };
        break;
      case "read":
        request = {
          mayEditBook: false,
          mayViewBook: true,
          reviewed: true,
        };
        break;
      case "readwrite":
        request = {
          mayEditBook: true,
          mayViewBook: true,
          reviewed: true,
        };
        break;
      default:
        request = null;
        break;
    }

    return {
      prev: stateEntry.prev,
      new: request,
    };
  });
}
