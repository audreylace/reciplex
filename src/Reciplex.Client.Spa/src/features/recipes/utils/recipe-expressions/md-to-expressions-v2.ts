import { is } from "unist-util-is";
import { visitParents } from "unist-util-visit-parents";
import type { Node as AstNode } from "unist";
import type { Root, Text as MdAstTextNode } from "mdast";
import {
  parseTextForRecipeExpression,
  type ICommandTokens,
} from "./expression-syntax-v2";
import type { IRExpIngredientCommand } from "./ingredient-command-syntax";
import type { IRExpToolCommand } from "./tool-command-syntax";

/**
 * Plugin for adding recipe expression syntax to mark down
 * @param onCreated optional method invoked each time a new recipe expression node is created
 * @returns MD visitor method
 */
export function recipeExpressionSyntaxPlugin() {
  return (tree: Root) => recipeSyntaxMarkDownTreeVisiter(tree);
}

export type RecipeAstNodes =
  | RecipeIngredientExpressionAstNode
  | RecipeToolExpressionAstNode;

export const RecipeAstElements = [
  "recipeIngredientExpression",
  "recipeToolExpression",
];
export const RecipeAstElementAttributes: Record<string, string[]> = {
  recipeIngredientExpression: ["position"],
  recipeToolExpression: ["position"],
};

export type RecipeExpressionAstNodeData<TCommand> = {
  position: number;
  command: TCommand;
};

export type RecipeIngredientExpressionAstNode = {
  type: "recipeIngredientExpression";
  data: RecipeExpressionAstNodeData<IRExpIngredientCommand>;
};

export type RecipeToolExpressionAstNode = {
  type: "recipeToolExpression";
  data: RecipeExpressionAstNodeData<IRExpToolCommand>;
};

function mapExpressionAstToHast(_: unknown, node: RecipeAstNodes) {
  return {
    type: "element",
    tagName: node.type,
    properties: {
      position: node.data.position,
    },
    children: [],
  };
}

export const RecipeAstToHastHandlers = {
  recipeIngredientExpression: mapExpressionAstToHast,
  recipeToolExpression: mapExpressionAstToHast,
};

export function recipeSyntaxMarkDownTreeVisiter(
  tree: Root,
  onCreated?: (node: RecipeAstNodes) => void,
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
        let maybeCommandNode: RecipeAstNodes | MdAstTextNode;
        switch (segment.type) {
          case "outside-text": // preserve existing text
            newChildren.push({
              value: segment.tokens.map((t) => t.image).join(""),
              type: "text",
            } as MdAstTextNode);
            break;
          case "command":
            maybeCommandNode = handleCommand(segment, () => position++);
            if (
              maybeCommandNode.type === "recipeIngredientExpression" ||
              maybeCommandNode.type === "recipeToolExpression"
            ) {
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
):
  | MdAstTextNode
  | RecipeIngredientExpressionAstNode
  | RecipeToolExpressionAstNode {
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
      type: "recipeIngredientExpression",
    };
  }

  if (commandTokens.command?.tag === "tool") {
    return {
      data: { command: commandTokens.command, position: nextId() },
      type: "recipeToolExpression",
    };
  }

  return {
    value: commandTokens.tokens.map((t) => t.image).join(),
    type: "text",
  };
}
