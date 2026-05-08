import { createContext } from "preact";
import type { RecipeIngredientConceptCollection } from "./ingredient-concept";
import type { RecipeToolConceptCollection } from "./tool-concept";

/**
 * The model for the recipe details context
 */
export interface IRecipeDetailsContextModel {
  /**
   * The raw markdown text
   */
  markdownText: string;
  /**
   * the compiled react component for rendering the markdown
   */
  component: React.ReactElement;
  /**
   * ingredients in the markdown collection
   */
  ingredientCollection: RecipeIngredientConceptCollection;
  /**
   * tools in the markdown collection
   */
  toolCollection: RecipeToolConceptCollection;
}

/**
 * The recipe details context
 */
export const RecipeDetailsContext =
  createContext<IRecipeDetailsContextModel | null>(null);
