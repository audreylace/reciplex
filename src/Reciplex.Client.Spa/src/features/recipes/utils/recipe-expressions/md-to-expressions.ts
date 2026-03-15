import { is } from "unist-util-is";
import { visitParents } from "unist-util-visit-parents";
import type { Node as AstNode } from "unist";
import type { Root, Text as MdAstTextNode } from "mdast";
import {
  parseTextForRecipeExpression,
  type RecipeIngredientFreeformModel,
  type RecipeIngredientWithUnitModel,
} from "./expression-types";

/**
 * Plugin for adding recipe expression syntax to mark down
 * @returns MD visitor method
 */
export function recipeExpressionSyntax() {
  return MarkDownTreeVisiter;
}

export const RecipeFreeformIngredientElement = "recipeIngredientFreeform";
export const RecipeIngredientWithUnit = "recipeIngredientWithUnit";
export const RecipeAstElements = ["recipeExpressionNode"];
export const RecipeAstElementAttributes: Record<string, string[]> = {
  recipeExpressionNode: ["position"],
};

export type RecipeExpressionAstNodeData =
  | {
      position: number;
      expression: RecipeIngredientFreeformModel;
    }
  | {
      position: number;
      expression: RecipeIngredientWithUnitModel;
    };

function mapExpressionAstToHast(
  _: unknown,
  node: AstNode & {
    data: RecipeExpressionAstNodeData;
  },
) {
  return {
    type: "element",
    tagName: "recipeExpressionNode",
    properties: {
      position: node.data.position,
    },
    children: [],
  };
}

export const RecipeAstToHastHandlers = {
  [RecipeFreeformIngredientElement]: mapExpressionAstToHast,
  [RecipeIngredientWithUnit]: mapExpressionAstToHast,
};

function MarkDownTreeVisiter(tree: Root) {
  let position = 0;
  visitParents(tree, function (node: AstNode, parents: AstNode[]) {
    if (is(node, "text")) {
      const textNode = node as MdAstTextNode;
      const recipeExpressions = parseTextForRecipeExpression(textNode.value);

      // failed to parse or no expressions. Do nothing.
      if (recipeExpressions === null || recipeExpressions.length <= 0) {
        return;
      }

      // attempt to go to the direct parent of this node.
      // If the parent has an children array then continue
      // since we need to replace the current node with new one.
      const directParent = parents[parents.length - 1];
      let childrenArray = null;
      if (
        directParent &&
        (directParent as unknown as { children: AstNode[] | undefined | null })
          .children
      ) {
        childrenArray = (directParent as unknown as { children: AstNode[] })
          .children;
      } else {
        return;
      }

      let lastExpressionEnd = 0;
      const newChildren: AstNode[] = [];
      for (const expression of recipeExpressions) {
        if (expression.startIndex !== lastExpressionEnd) {
          const leadingText = textNode.value.slice(
            lastExpressionEnd,
            expression.startIndex,
          );

          newChildren.push({
            value: leadingText,
            type: "text",
          } as MdAstTextNode);
        }

        switch (expression.tag) {
          case "expression-echo":
            newChildren.push({
              value: expression.text,
              type: "text",
            } as MdAstTextNode);
            break;
          case "ingredient-freeform":
            newChildren.push({
              type: RecipeFreeformIngredientElement,
              data: { position: position++, expression },
            });
            break;
          case "ingredient-with-unit":
            newChildren.push({
              type: RecipeIngredientWithUnit,
              data: { position: position++, expression },
            });
            break;
        }
        lastExpressionEnd = expression.endIndex;
      }

      if (lastExpressionEnd < textNode.value.length) {
        newChildren.push({
          value: textNode.value.slice(lastExpressionEnd),
          type: "text",
        } as MdAstTextNode);
      }

      const nodeIndex = childrenArray.indexOf(textNode);
      if (nodeIndex === -1) {
        return;
      }
      childrenArray.splice(nodeIndex, 1, ...newChildren);
      return nodeIndex + newChildren.length;
    }
  });
}
