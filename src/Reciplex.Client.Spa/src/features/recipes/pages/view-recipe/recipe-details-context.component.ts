import { createContext } from "preact";
import type {
  RecipeIngredientExpressionAstNode,
  RecipeToolExpressionAstNode,
} from "../../utils/recipe-expressions/md-to-expressions-v2";
import type { RecipeConceptCollection } from "./recipe-concept";

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
  ingredientCollection: RecipeConceptCollection<RecipeIngredientExpressionAstNode>;
  toolCollection: RecipeConceptCollection<RecipeToolExpressionAstNode>;
}

/**
 * The recipe details context
 */
export const RecipeDetailsContext =
  createContext<IRecipeDetailsContextModel | null>(null);
