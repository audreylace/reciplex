import { useQuery } from "@tanstack/react-query";
import { useActiveUserKey } from "../../auth/hooks/useActiveUser.hook";
import { AssertString } from "../../sentinel/stringUtilities";
import { recipeBookSharedAccessQueryKey } from "../utils/recipe-queries/recipe-query-key-factory";
import { useRecipeStoreContext } from "./useRecipeStoreContext.hook";

/**
 * gets recipe book shared access status
 * @param bookId the book id
 * @param shareKey the share key
 * @returns the query result
 */
export function useGetRecipeBookSharedAccessStatus(
  bookId: string,
  shareKey?: string,
) {
  const userKey = useActiveUserKey();
  const recipeStore = useRecipeStoreContext();
  return useQuery({
    enabled: !!userKey && !!bookId,
    queryKey: recipeBookSharedAccessQueryKey(userKey ?? "", bookId, shareKey),
    queryFn: () =>
      recipeStore.getRecipeBookShareStatus(
        AssertString(userKey),
        bookId,
        shareKey,
      ),
  });
}
