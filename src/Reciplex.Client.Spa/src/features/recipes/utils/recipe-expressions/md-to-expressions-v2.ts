import { is } from "unist-util-is";
import { visitParents } from "unist-util-visit-parents";
import type { Node as AstNode } from "unist";
import type { Root, Text as MdAstTextNode } from "mdast";
import {
  parseTextForRecipeExpression,
  type ICommandTokens,
  type RExpIngredientCommand,
} from "./expression-syntax-v2";

/**
 * Plugin for adding recipe expression syntax to mark down
 * @param onCreated optional method invoked each time a new recipe expression node is created
 * @returns MD visitor method
 */
export function recipeExpressionSyntaxPlugin() {
  return (tree: Root) => recipeSyntaxMarkDownTreeVisiter(tree);
}

export const RecipeAstElements = ["recipeExpression"];
export const RecipeAstElementAttributes: Record<string, string[]> = {
  recipeExpression: ["position"],
};

export type RecipeExpressionAstNodeData = {
  position: number;
  command: RExpIngredientCommand;
};

export type RecipeExpressionAstNode = {
  type: "recipeExpression";
  data: RecipeExpressionAstNodeData;
};

function mapExpressionAstToHast(_: unknown, node: RecipeExpressionAstNode) {
  return {
    type: "element",
    tagName: "recipeExpression",
    properties: {
      position: node.data.position, // in Hast conversion, strip away all data except the position.
    },
    children: [],
  };
}

export const RecipeAstToHastHandlers = {
  recipeExpression: mapExpressionAstToHast,
};

export function recipeSyntaxMarkDownTreeVisiter(
  tree: Root,
  onCreated?: (node: RecipeExpressionAstNode) => void,
) {
  let position = 0;
  visitParents(tree, function (node: AstNode, parents: AstNode[]) {
    if (is(node, "text")) {
      const textNode = node as MdAstTextNode;

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

      const segments = parseTextForRecipeExpression(textNode.value);

      // failed to parse. Do nothing.
      if (segments === null || segments.length <= 0) {
        return;
      }

      // no commands. Do nothing.
      if (!segments.some((s) => s.type === "command")) {
        return;
      }

      const newChildren: AstNode[] = [];
      for (const segment of segments) {
        let maybeCommandNode: MdAstTextNode | RecipeExpressionAstNode;
        switch (segment.type) {
          case "outside-text": // preserve existing text
            newChildren.push({
              value: segment.tokens.map((t) => t.image).join(""),
              type: "text",
            } as MdAstTextNode);
            break;
          case "command":
            maybeCommandNode = handleCommand(segment, () => position++);
            if (maybeCommandNode.type === "recipeExpression") {
              onCreated?.(maybeCommandNode);
            }
            newChildren.push(maybeCommandNode);
            break;
        }
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

function handleCommand(
  commandTokens: ICommandTokens,
  nextId: () => number,
): MdAstTextNode | RecipeExpressionAstNode {
  // echo gets turned into a text node
  if (commandTokens.command?.tag === "echo") {
    return {
      value: commandTokens.command.text,
      type: "text",
    };
  }

  if (commandTokens.command?.tag === "ingredient") {
    return {
      data: { command: commandTokens.command, position: nextId() },
      type: "recipeExpression",
    };
  }

  return {
    value: commandTokens.tokens.map((t) => t.image).join(),
    type: "text",
  };
}
