import { create } from "zustand";
import { createJSONStorage, persist } from "zustand/middleware";

/**
 * Holds client side state for the recipe feature. This is not user specific.
 */
export const useRecipeClientStateContext = create<IUseRecipeClientState>()(
  persist(
    (set) => {
      return {
        recipeListPageSize: 10,
        bookListPageSize: 10,
        setRecipeListPageSize: (newSize: number) =>
          set({ recipeListPageSize: sanitizePageSize(newSize) }),
        setBookListPageSize: (newSize: number) =>
          set({
            bookListPageSize: sanitizePageSize(newSize),
          }),
      };
    },
    {
      name: "recipe-feature-client-state", // name of the item in localStorage
      storage: createJSONStorage(() => localStorage),
    },
  ),
);

/** state shape of `useRecipeStateContext` */
export interface IUseRecipeClientState {
  /**
   * Number of recipes presented to the end user when listing recipes.
   * This setting is controlled by the end user.
   */
  recipeListPageSize: number;
  /**
   * Sets `recipeListPageSize`
   * @param newSize `recipeListPageSize` will be set to this value
   */
  setRecipeListPageSize(newSize: number): void;

  /**
   * Number of books presented to the end user when listing books.
   * This setting is controlled by the end user.
   */
  bookListPageSize: number;
  /**
   * Sets `bookListPageSize`
   * @param newSize `bookListPageSize` will be set to this value
   */
  setBookListPageSize(newSize: number): void;
}

/**
 * Sanitizes page size input to ensure it meets the constraints of the domain model
 * @param incoming incoming value to sanitize
 * @returns the value after sanitization
 */
function sanitizePageSize(incoming: number): number {
  if (typeof incoming !== "number" || isNaN(incoming)) {
    return 10;
  }

  return Math.min(100, Math.max(5, incoming));
}
