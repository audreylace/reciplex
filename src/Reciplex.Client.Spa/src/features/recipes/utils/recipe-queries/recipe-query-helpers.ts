import { hashKey, type QueryClient } from "@tanstack/react-query";
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
  let dataToCache;
  let idToFind;
  if (typeof modelOrId === "string") {
    dataToCache = null;
    idToFind = modelOrId;
  } else {
    dataToCache = modelOrId;
    idToFind = modelOrId.id;
  }

  // update recipe entry with new state
  client.setQueryData(recipeQueryKey(userKey, idToFind), dataToCache);
  const rootKey = recipeQueryKeyRoot(userKey);

  if (isNew) {
    if (!dataToCache) {
      throw Error("isNew can only be set when caching data");
    }

    // if its new, then we don't know what page it belongs
    // on so just invalidate all pages that it could belong in.
    const cacheHash = hashKey([rootKey]);
    client.resetQueries({
      predicate: (cacheEntry) => {
        if (cacheHash !== hashKey([cacheEntry.queryKey[0]])) {
          return false;
        }

        if (cacheEntry.queryKey[1] !== recipeListCacheKeyBranch) {
          return false;
        }

        const bookKey = (
          cacheEntry.queryKey[2] as IRecipeListArgsKeyNode | undefined
        )?.bookKey;
        if (
          // non-book specific page. Recipe could be here based on search.
          !bookKey ||
          // book specific page. Recipe could be here based on the ordering.
          dataToCache.bookId === bookKey
        ) {
          return true;
        }
        return false;
      },
    });
    return;
  }

  // search through cache of recipe list pages deleting/updating the recipe anywhere
  // entries are found.
  client.setQueriesData(
    { queryKey: [rootKey, recipeListCacheKeyBranch] },
    (old: IGetRecipesInBookResult) => {
      const index = old.recipes.findIndex((r) => r.id === idToFind);
      if (index === -1) {
        return old;
      }

      const newList = old.recipes.slice(0);
      if (dataToCache) {
        newList.splice(index, 1, dataToCache);
      } else {
        newList.splice(index, 1);
      }

      return { ...old, recipes: newList };
    },
  );
}

export function updateRecipeBookInCache(
  client: QueryClient,
  userKey: string,
  modelOrId: IRecipeBookModel | string,
  isNew?: boolean,
) {
  let dataToCache;
  let idToFind;
  if (typeof modelOrId === "string") {
    dataToCache = null;
    idToFind = modelOrId;
  } else {
    dataToCache = modelOrId;
    idToFind = modelOrId.id;
  }

  // update recipe book entry with new state
  client.setQueryData(recipeBookQueryKey(userKey, idToFind), dataToCache);
  const rootKey = recipeQueryKeyRoot(userKey);

  if (isNew) {
    if (!dataToCache) {
      throw Error("isNew can only be set when caching data");
    }

    // invalidate all cache book list data
    client.resetQueries({
      queryKey: [rootKey, recipeBookListCacheKeyBranch],
    });

    return;
  }

  if (!dataToCache) {
    // drop all recipe list pages that could contain recipes related to the book
    invalidateRecipePageSetViaBookId(client, userKey, idToFind);

    // drop all recipes belonging to the book
    client.setQueriesData(
      { queryKey: [rootKey, recipeCacheKeyBranch] },
      (old: IRecipeModel) => {
        if (old.bookId === idToFind) {
          return null;
        }
        return old;
      },
    );
  }

  // delete/update the book in book list pages
  client.setQueriesData(
    { queryKey: [rootKey, recipeBookListCacheKeyBranch] },
    (old: IGetRecipeBooksResult) => {
      const index = old.books.findIndex((b) => b.id === idToFind);
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

function invalidateRecipePageSetViaBookId(
  client: QueryClient,
  userKey: string,
  bookKey?: string,
) {
  const rootKey = recipeQueryKeyRoot(userKey);
  const cacheHash = hashKey([rootKey]);

  client.resetQueries({
    predicate: (cacheEntry) => {
      // scope to recipe cache entries
      if (cacheHash !== hashKey([cacheEntry.queryKey[0]])) {
        return false;
      }

      // scope to recipe list
      if (cacheEntry.queryKey[1] === recipeListCacheKeyBranch) {
        // if the entry is not scoped to a specific book, drop it
        const cacheBookKey = (
          cacheEntry.queryKey[2] as IRecipeListArgsKeyNode | undefined
        )?.bookKey;

        if (!cacheBookKey) {
          return true;
        }

        // clear any pages scoped to the request book if
        // the clear should be limited to a specific book.
        if (bookKey && cacheBookKey === bookKey) {
          return true;
        }
      }

      return false;
    },
  });
}
