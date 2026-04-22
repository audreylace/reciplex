import { type QueryClient } from "@tanstack/react-query";
import type {
  IGetRecipeBooksResult,
  IGetRecipesInBookResult,
  IRecipeBookModel,
  IRecipeModel,
} from "../../services/recipe-types";
import {
  recipeBookListCacheKeyBranch,
  recipeBookQueryKey,
  recipeCacheKeyBranch,
  recipeListCacheKeyBranch,
  recipeQueryKey,
  recipeQueryKeyRoot,
  type IRecipeListArgsKeyNode,
} from "./recipe-query-key-factory";
/**
 * Caches a new or replaces an existing recipe record in the client.
 * @param client the query client
 * @param userKey the user key
 * @param recipe recipe data to cache. string instead of the model means the recipe is deleted. Related
 * cached data is cleared.
 */
export function updateRecipeInCache(
  client: QueryClient,
  userKey: string,
  recipe: IRecipeModel,
  isNew?: boolean,
): void;
export function updateRecipeInCache(
  client: QueryClient,
  userKey: string,
  recipeId: string,
): void;
export function updateRecipeInCache(
  client: QueryClient,
  userKey: string,
  modelOrId: IRecipeModel | string,
  isNew?: boolean,
) {
  const { idToFind, dataToCache } = extractModelAndId(modelOrId);

  if (isNew) {
    if (!dataToCache) {
      throw Error("isNew can only be set when caching data");
    }
    client.setQueryData(recipeQueryKey(userKey, idToFind), dataToCache);
    dropCachedRecipesPagesFilteredByBookKey(
      client,
      userKey,
      dataToCache.bookId,
    );
    return;
  }

  // update recipe entry with new state
  updateRecipeCacheData(client, userKey, idToFind, dataToCache);
}

export function updateRecipeBookInCache(
  client: QueryClient,
  userKey: string,
  modelOrId: IRecipeBookModel | string,
  isNew?: boolean,
) {
  const { idToFind, dataToCache } = extractModelAndId(modelOrId);

  if (isNew) {
    if (!dataToCache) {
      throw Error("isNew can only be set when caching data");
    }

    client.setQueryData(recipeBookQueryKey(userKey, idToFind), dataToCache);
    // invalidate all cache book list data
    client.resetQueries({
      queryKey: [recipeQueryKeyRoot(userKey), recipeBookListCacheKeyBranch],
    });

    return;
  }

  updateRecipeBookCacheData(client, userKey, idToFind, dataToCache);
}

function dropCachedRecipesFilteredByBook(
  client: QueryClient,
  userKey: string,
  bookId: string,
) {
  const rootKey = recipeQueryKeyRoot(userKey);
  // drop all recipes belonging to the book
  client.setQueriesData(
    { queryKey: [rootKey, recipeCacheKeyBranch] },
    (old: IRecipeModel | null | undefined) => {
      if (!old) {
        return old;
      }
      if (old.bookId === bookId) {
        return null;
      }
      return old;
    },
  );
}

function dropCachedRecipesPagesFilteredByBookKey(
  client: QueryClient,
  userKey: string,
  bookKey: string,
) {
  client.resetQueries({
    queryKey: [recipeQueryKeyRoot(userKey), recipeListCacheKeyBranch],
    predicate: (cacheEntry) => {
      const cacheBookKey = (
        cacheEntry.queryKey[2] as IRecipeListArgsKeyNode | undefined
      )?.bookKey;

      // if the entry is not scoped to a specific book, drop it
      if (!cacheBookKey) {
        return true;
      }

      // clear any pages scoped to the request book if
      // the clear should be limited to a specific book.
      if (cacheBookKey === bookKey) {
        return true;
      }
      return false;
    },
  });
}

function extractModelAndId<T extends { id: string }>(modelOrId: T | string) {
  let dataToCache;
  let idToFind;
  if (typeof modelOrId === "string") {
    dataToCache = null;
    idToFind = modelOrId;
  } else {
    dataToCache = modelOrId;
    idToFind = modelOrId.id;
  }

  return { dataToCache, idToFind };
}

function updateRecipeCacheData(
  client: QueryClient,
  userKey: string,
  id: string,
  data: IRecipeModel | null,
) {
  client.setQueryData(recipeQueryKey(userKey, id), data);

  // search through cache of recipe list pages deleting/updating the recipe anywhere
  // entries are found.
  client.setQueriesData(
    { queryKey: [recipeQueryKeyRoot(userKey), recipeListCacheKeyBranch] },
    (old: IGetRecipesInBookResult | null | undefined) => {
      if (!old) {
        return old;
      }
      const index = old.recipes.findIndex((r) => r.id === id);
      if (index === -1) {
        return old;
      }

      const newList = old.recipes.slice(0);
      if (data) {
        newList.splice(index, 1, data);
      } else {
        newList.splice(index, 1);
      }

      return { ...old, recipes: newList };
    },
  );
}

function updateRecipeBookCacheData(
  client: QueryClient,
  userKey: string,
  id: string,
  dataToCache: IRecipeBookModel | null,
) {
  client.setQueryData(recipeBookQueryKey(userKey, id), dataToCache);

  if (!dataToCache) {
    dropCachedRecipesPagesFilteredByBookKey(client, userKey, id);
    dropCachedRecipesFilteredByBook(client, userKey, id);
  }

  // delete/update the book in book list pages
  client.setQueriesData(
    { queryKey: [recipeQueryKeyRoot(userKey), recipeBookListCacheKeyBranch] },
    (old: IGetRecipeBooksResult | null | undefined) => {
      if (!old) {
        return old;
      }
      const index = old.books.findIndex((b) => b.id === id);
      if (index === -1) {
        return old;
      }

      const newList = old.books.slice(0);
      if (dataToCache) {
        newList.splice(index, 1, dataToCache);
      } else {
        newList.splice(index, 1);
      }

      return { ...old, books: newList };
    },
  );
}
