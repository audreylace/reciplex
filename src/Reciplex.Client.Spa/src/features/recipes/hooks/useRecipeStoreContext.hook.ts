import { useContext } from "preact/hooks";
import { type IRecipeBookStore } from "../services/recipe-types";
import { createContext } from "preact";

/**
 * context for storing the recipe book store
 */
export const RecipeStore = createContext<IRecipeBookStore | null>(null);

/**
 * hook that returns the recipe store
 * @returns the recipe store throwing on error
 */
export function useRecipeStoreContext() {
  const context = useContext(RecipeStore);
  if (!context) {
    throw Error(
      "useRecipeStoreContext must be used within a RecipeStore provider",
    );
  }
  return context;
}
