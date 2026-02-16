import { useGetRecipeByIdQuery } from "./useGetRecipeByIdQuery.hook";
import type {
  IGetRecipeByIdResult,
  IRecipeBookModel,
  IRecipeModel,
} from "../services/recipe-types";
import { useState } from "preact/hooks";
import { useUpdateRecipeMutation } from "./useUpdateRecipeMutation.hook";
import { useDeleteRecipeMutation } from "./useDeleteRecipeMutation";

/**
 * hook for loading recipe data that is meant to be mutated in some way
 * @param recipeId the id of the recipe to load
 */
export function useEditRecipe(
  recipeId: string | undefined | null,
): UseEditRecipeReturn {
  const recipeQuery = useGetRecipeByIdQuery(recipeId ?? "", {
    noCache: true,
    refetchInterval: 15000,
  });
  const [loadedData, setLoadedData] = useState<
    UseEditRecipeReturnWithData | UseEditRecipeReturnWithDataAndAction | null
  >(null);

  const recipeMutation = useUpdateRecipeMutation();
  const deleteMutation = useDeleteRecipeMutation();

  if (loadedData) {
    // cascade mutation error back to caller
    if (
      recipeMutation.status === "error" ||
      deleteMutation.status === "error"
    ) {
      const newState: UseEditRecipeReturnWithData = {
        tag: "mutate-error",
        recipe: loadedData.recipe,
        book: loadedData.book,
      };
      setLoadedData((s) => (s && s.tag !== "mutate-error" ? newState : s));
    }

    // catch changes to the underlying data
    if (
      loadedData.tag === "loaded" &&
      recipeQuery.data?.recipe.versionTag !== loadedData.recipe.versionTag
    ) {
      const newState: UseEditRecipeReturnWithData = {
        tag: "conflict",
        recipe: loadedData.recipe,
        book: loadedData.book,
      };
      setLoadedData(newState);
      return newState;
    }

    return loadedData;
  }

  if (!recipeId) {
    // bad route -- missing route parameters
    return { tag: "bad-route" };
  }

  if (recipeQuery.status === "success") {
    // only want new data not cached data from react query
    if (!recipeQuery.isFetchedAfterMount) {
      if (recipeQuery.isPaused) {
        return { tag: "offline" };
      }
      if (recipeQuery.isRefetching) {
        return { tag: "loading" };
      }
    }

    // API returns null when entry is not found
    if (!recipeQuery.data) {
      return { tag: "not-found" };
    }

    // lock in data after first load
    if (!loadedData) {
      const recipe = recipeQuery.data.recipe;
      const book = recipeQuery.data.book;

      if (!recipe.mayEdit) {
        const readonlyState: UseEditRecipeReturnWithData = {
          tag: "read-only",
          recipe: recipeQuery.data.recipe,
          book: recipeQuery.data.book,
        };
        setLoadedData(readonlyState);
        return readonlyState;
      }

      const newState: UseEditRecipeReturnWithDataAndAction = {
        tag: "loaded",
        recipe: recipeQuery.data.recipe,
        book: recipeQuery.data.book,
        delete: async () => {
          let canEdit = false;
          setLoadedData((previous) => {
            if (previous?.tag === "loaded" && recipe.mayEdit) {
              canEdit = true;
              return {
                tag: "deleting",
                recipe: previous.recipe,
                book: previous.book,
              };
            }
            return previous;
          });

          if (!canEdit) {
            return;
          }

          await deleteMutation.mutateAsync({
            id: recipeId,
            versionTag: recipe.versionTag,
          });

          setLoadedData({
            tag: "deleted",
            recipe: recipe,
            book: book,
          });
        },
        update: async (newState: {
          recipeName: string;
          recipeDescription: string;
          recipeDetails: string;
        }) => {
          let canEdit = false;
          setLoadedData((previous) => {
            if (previous?.tag === "loaded" && recipe.mayEdit) {
              canEdit = true;
              return {
                tag: "saving",
                recipe: previous.recipe,
                book: previous.book,
              };
            }
            return previous;
          });

          if (!canEdit) {
            throw Error("invalid action: user can not modify recipe");
          }

          const result = await recipeMutation.mutateAsync({
            recipeId: recipe.id,
            name: newState.recipeName,
            shortDescription: newState.recipeDescription,
            details: newState.recipeDetails,
            versionTag: recipe.versionTag,
          });

          setLoadedData({
            tag: "mutated",
            recipe: recipe,
            book: book,
          });

          return result;
        },
      };
      setLoadedData(newState);
      return newState;
    }

    return { tag: "loading" };
  }

  // pending load
  if (recipeQuery.status === "pending") {
    if (recipeQuery.isPaused) {
      return { tag: "offline" };
    }
    return { tag: "loading" };
  }

  return { tag: "loading-failed" };
}

export const EditRecipeLoadingState = {
  loading: "loading",
  loadingFailed: "loading-failed",
  notFound: "not-found",
  badRoute: "bad-route",
  loaded: "loaded",
  offline: "offline",
  conflict: "conflict",
  readonly: "read-only",
  mutated: "mutated",
  deleted: "deleted",
  mutateError: "mutate-error",
  saving: "saving",
  deleting: "deleting",
} as const;
export type EditRecipeLoadingState =
  (typeof EditRecipeLoadingState)[keyof typeof EditRecipeLoadingState];

type UseEditRecipeReturn =
  | UseEditRecipeReturnNoData<
      "loading" | "loading-failed" | "not-found" | "bad-route" | "offline"
    >
  | UseEditRecipeReturnWithData
  | UseEditRecipeReturnWithDataAndAction;

interface GetInitialDataForRecipeEditReturnBase {
  tag: EditRecipeLoadingState;
}

interface UseEditRecipeReturnNoData<
  TTag extends EditRecipeLoadingState,
> extends GetInitialDataForRecipeEditReturnBase {
  tag: TTag;
  recipe?: undefined;
  book?: undefined;
}

interface UseEditRecipeReturnWithData extends GetInitialDataForRecipeEditReturnBase {
  tag:
    | "conflict"
    | "read-only"
    | "mutated"
    | "deleted"
    | "mutate-error"
    | "saving"
    | "deleting"
    | "read-only";
  recipe: IRecipeModel;
  book: IRecipeBookModel;
}

interface UseEditRecipeReturnWithDataAndAction extends GetInitialDataForRecipeEditReturnBase {
  tag: "loaded";
  recipe: IRecipeModel;
  book: IRecipeBookModel;
  update: (newState: {
    recipeName: string;
    recipeDescription: string;
    recipeDetails: string;
  }) => Promise<IGetRecipeByIdResult>;
  delete: () => Promise<void>;
}
