import { useContext } from "preact/hooks";
import { type IRecipeBookStore } from "../../../services/recipe-store";
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
    throw Error();
  }
  return context;
}
