import type { Root } from "mdast";
import { useMemo } from "react";
import rehypeReact from "rehype-react";
import rehypeSanitize, { defaultSchema } from "rehype-sanitize";
import remarkParse from "remark-parse";
import remarkRehype from "remark-rehype";
import { unified } from "unified";
import {
  recipeSyntaxMarkDownTreeVisiter,
  RecipeAstToHastHandlers,
  RecipeAstElementAttributes,
  RecipeAstElements,
} from "../../utils/recipe-expressions/md-to-expressions-v2";
import { RecipeIngredientConceptCollection } from "./ingredient-concept";
import { RecipeToolConceptCollection } from "./tool-concept";
import type { IHandleRecipeAstNode } from "./concept-collection";

import { jsxDEV } from "react/jsx-dev-runtime";
import { Fragment, jsx, jsxs } from "react/jsx-runtime";

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
  return useMemo(() => {
    if (!mdText) {
      return null;
    }
    const ingredientCollection = new RecipeIngredientConceptCollection();
    const toolCollection = new RecipeToolConceptCollection();
    const collections: IHandleRecipeAstNode[] = [
      ingredientCollection,
      toolCollection,
    ];

    const processor = unified()
      .use(remarkParse)
      .use(() => {
        return (tree: Root) => {
          recipeSyntaxMarkDownTreeVisiter(tree, (node) => {
            collections.some((c) => c.tryAddNode(node));
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

    return {
      markdownText: mdText,
      component: component,
      ingredientCollection: ingredientCollection,
      toolCollection: toolCollection,
    };
  }, [components, mdText]);
}
