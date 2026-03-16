import type { Root } from "mdast";
import { useState, useEffect } from "preact/hooks";
import { Fragment, jsx, jsxs, jsxDEV } from "preact/jsx-runtime";
import rehypeReact from "rehype-react";
import rehypeSanitize, { defaultSchema } from "rehype-sanitize";
import remarkParse from "remark-parse";
import remarkRehype from "remark-rehype";
import { unified } from "unified";
import {
  type RecipeIngredientExpressionAstNode,
  type RecipeToolExpressionAstNode,
  recipeSyntaxMarkDownTreeVisiter,
  RecipeAstToHastHandlers,
  RecipeAstElementAttributes,
  RecipeAstElements,
} from "../../utils/recipe-expressions/md-to-expressions-v2";
import { RecipeConceptCollection } from "./recipe-concept";
import type { IRecipeDetailsContextModel } from "./recipe-details-context.component";

const customSchema = {
  ...defaultSchema,
  tagNames: [
    // @ts-expect-error expanding tagNames is the expected pattern for rehype-remark per their docs
    ...defaultSchema.tagNames,
    ...RecipeAstElements,
  ],
  attributes: {
    ...defaultSchema.attributes,
    ...RecipeAstElementAttributes,
  },
};

export interface IPipelineComponents {
  recipeIngredientExpression: React.ReactNode;
  recipeToolExpression: React.ReactNode;
}

export function usePipeline(
  mdText: string | null | undefined,
  components: IPipelineComponents,
) {
  const [detailsContextModel, setDetailsContextModel] =
    useState<IRecipeDetailsContextModel | null>(null);

  useEffect(() => {
    if (!mdText) {
      setDetailsContextModel(null);
      return;
    }
    const ingredientCollection =
      new RecipeConceptCollection<RecipeIngredientExpressionAstNode>();
    const toolCollection =
      new RecipeConceptCollection<RecipeToolExpressionAstNode>();
    const processor = unified()
      .use(remarkParse)
      .use(() => {
        return (tree: Root) => {
          recipeSyntaxMarkDownTreeVisiter(tree, (node) => {
            if (node.type === "recipeIngredientExpression") {
              ingredientCollection.addAstNode(node);
            }

            if (node.type === "recipeToolExpression") {
              toolCollection.addAstNode(node);
            }
          });
        };
      })
      .use(remarkRehype, { handlers: RecipeAstToHastHandlers })
      .use(rehypeSanitize, customSchema)
      .use(rehypeReact, {
        Fragment,
        jsx,
        jsxs,
        jsxDEV,
        components: components,
      });

    const component = processor.processSync(mdText).result;

    setDetailsContextModel({
      markdownText: mdText,
      component: component,
      ingredientCollection: ingredientCollection,
      toolCollection: toolCollection,
    });
  }, [components, mdText]);

  return detailsContextModel;
}
