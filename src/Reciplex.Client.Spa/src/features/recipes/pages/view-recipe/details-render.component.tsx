import Markdown from "react-markdown";
import rehypeSanitize, { defaultSchema } from "rehype-sanitize";
import { visit } from "unist-util-visit";
import { visitParents } from "unist-util-visit-parents";
import type { Node as UNode } from "@types/unist";
import type { Root, Text as MDastText } from "mdast";
import { is } from "unist-util-is";

import { findAndReplace } from "mdast-util-find-and-replace";

import detailsRenderStyleModule from "./details-render.module.css";
import {} from "../../utils/recipe-expressions/lexer2";
import type { HTMLAttributes } from "preact";
import {
  RecipeExpressionLexer3,
  RecipeExpressionParser,
  RecipeExpressionParserInstance,
  RecipeExpressionVisitor,
} from "../../utils/recipe-expressions/lexer3";
import type { IToken } from "chevrotain";

/** renders the recipe details */
export function DetailsRender(props: DetailsRenderProps) {
  return (
    <>
      <h4>Recipe Details</h4>
      <div className={detailsRenderStyleModule.detailsWrapper}>
        <DetailsMdRender {...props} />
      </div>
    </>
  );
}

const customSchema = {
  ...defaultSchema,
  tagNames: [
    // @ts-expect-error expanding tagNames is the expected pattern for rehype-remark per their docs
    ...defaultSchema.tagNames,
    "recipe", // Add your custom element name here
  ],
  attributes: {
    ...defaultSchema.attributes,
    recipe: ["*"],
  },
};

/** props for  `DetailsRender` */
export interface DetailsRenderProps {
  detailsMd?: string;
  mayEdit: boolean;
  goToEditAction: () => void;
}

/** inner md render */
function DetailsMdRender({
  detailsMd,
  mayEdit,
  goToEditAction,
}: DetailsRenderProps) {
  if (!detailsMd) {
    if (!mayEdit) {
      return (
        <p>
          <i>No details</i>
        </p>
      );
    }
    return (
      <p
        className={detailsRenderStyleModule.emptyDetails}
        onClick={goToEditAction}
      >
        <i>Click to edit and add details</i>
      </p>
    );
  }

  return (
    <Markdown
      rehypePlugins={[[rehypeSanitize, customSchema]]}
      remarkPlugins={[/*extractRecipeSyntax,*/ extractRecipeSyntax2]}
      allowedElement={"recipe"}
      remarkRehypeOptions={{
        handlers: {
          recipe: (state, node) => {
            //return h("recipe", { className: "custom-node" }, node.children);

            return {
              type: "element",
              tagName: "recipe",
              properties: {
                value: node.value,
              },
              children: [],
            };
          },
        },
      }}
      components={{
        recipe: (props: HTMLAttributes<HTMLElement>) => {
          console.log(props.className);

          return <span>{props.value}</span>;
        },
      }}
    >
      {detailsMd}
    </Markdown>
  );
}

export function extractRecipeSyntax() {
  return function (tree) {
    findAndReplace(tree, [
      /\(\(\s(?:[^()]|"\(|"\))*?\s\)\)/g,
      function (value, node) {
        //console.log(`${value} : ${node}`);
        //console.log(node);
        //console.log(selectedString);

        return { type: "recipe-expression", value: value };
        // return "gotcha";
      },
    ]);
  };
}

function extractRecipeSyntax2() {
  return (tree: Root) => {
    visitParents(tree, function (node: UNode, parents: UNode[]) {
      if (is(node, "text")) {
        const textNode = node as MDastText;
        const lexingResult = RecipeExpressionLexer3.tokenize(textNode.value);

        let tokenSet: IToken[] = [];
        let isInExpression = false;
        const groups: IToken[][] = [];
        lexingResult.tokens.forEach((token) => {
          if (token.tokenType.name === "OpeningSQuaredExpression") {
            isInExpression = true;
            tokenSet = [];
          }

          if (isInExpression) {
            //console.log(token);
            tokenSet.push(token);
          }
          if (token.tokenType.name === "ClosingSQuaredExpression") {
            isInExpression = false;
            groups.push(tokenSet);
            tokenSet = [];
          }
        });

        groups.forEach((set) => {
          if (set.length < 3) {
            return;
          }

          const atomIndex = set.findIndex(
            (t) => t.tokenType.name === "AtomExpression",
          );
          if (atomIndex === 1 || atomIndex === 2) {
            if (
              atomIndex === 2 &&
              set[1].tokenType.name !== "SQuaredWhitespaceExpression"
            ) {
              return;
            }
          }
          console.log(set[atomIndex].image);
        });

        //RecipeExpressionParserInstance.input = lexingResult.tokens;
        //const cst = RecipeExpressionParserInstance.recipeTextWithExpressions();
        //const visitor = new RecipeExpressionVisitor();
        //if (cst) {
        //   //console.log(cst);
        //   console.log(visitor.visit(cst));
        //visitor.visit(cst);
        // }
      }
    });
  };
}

type RecipeModelTags = "ingredient-freeform" | "ingredient-with-unit";
type RecipeModels = RecipeIngredientFreeform | RecipeIngredientWithUnit;
interface RecipeIngredientFreeform extends RecipeModel {
  text: string;
  tag: "ingredient-freeform";
}

interface RecipeIngredientWithUnit extends RecipeModel {
  text: string;
  amount: number;
  unit: string;
  tag: "ingredient-with-unit";
}

interface RecipeModel {
  tag: RecipeModelTags;
}

function RecipeIngredientFreeformExtractor(): RecipeIngredientFreeform | null {}
