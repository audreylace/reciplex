import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useActiveUserKey } from "../../auth/hooks/useActiveUser.hook";
import type {
  IRecipeBookUserPermissionsJsonRequest,
  IRecipeBookUserPermissionsJsonResponse,
} from "../services/recipe-types";
import { useRecipeStoreContext } from "./useRecipeStoreContext.hook";
import { recipeBookAccessQueryKey } from "../utils/recipe-queries/recipe-query-key-factory";

/**
 * Mutation to modify the list of the users with access
 */
export function usePatchUsersWithBookAccess() {
  const currentUserKey = useActiveUserKey();
  const recipeStore = useRecipeStoreContext();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({ bookId, data }: IUsePatchUsersWithBookAccessArgs) => {
      if (!currentUserKey || !bookId) {
        throw new Error();
      }

      if (!data) {
        return;
      }

      return await recipeStore.patchUsersWithBookAccess(
        currentUserKey,
        bookId,
        data,
      );
    },
    onSuccess: (_, mutationArgs) => {
      if (!currentUserKey) {
        return;
      }

      queryClient.setQueriesData(
        {
          queryKey: recipeBookAccessQueryKey(
            currentUserKey,
            mutationArgs.bookId,
          ),
        },
        (cachedData: IRecipeBookUserPermissionsJsonResponse[]) => {
          const newCacheData: IRecipeBookUserPermissionsJsonResponse[] = [];

          cachedData.forEach((u) => {
            const index = mutationArgs.data.findIndex(
              (entry) => entry[0] === u.userKey,
            );

            if (index === -1) {
              newCacheData.push(u); // no modifications to preserve entry
              return;
            }

            const mutationEntry = mutationArgs.data[index];

            // only keep entry if the second arg has a value
            if (mutationEntry[1]) {
              newCacheData.push({
                userKey: u.userKey,
                bookKey: mutationArgs.bookId,
                mayEditBook: mutationEntry[1].mayEditBook,
                mayViewBook: mutationEntry[1].mayViewBook,
                reviewed: mutationEntry[1].reviewed,
                userDisplayName: u.userDisplayName,
              });
            }
          });

          return newCacheData;
        },
      );
    },
  });
}

/** data for @see usePatchUsersWithBookAccess*/
export interface IUsePatchUsersWithBookAccessArgs {
  /** the id of the book mutate */
  bookId: string;
  /** the data to patch */
  data: [string, IRecipeBookUserPermissionsJsonRequest | undefined | null][];
}
