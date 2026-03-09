import Markdown from "react-markdown";
import rehypeSanitize, { defaultSchema } from "rehype-sanitize";
import detailsRenderStyleModule from "./details-render.module.css";

import {
  RecipeAstElementAttributes,
  RecipeAstElements,
  RecipeAstToHastHandlers,
  recipeExpressionSyntax,
  RecipeFreeformIngredientElement,
  RecipeIngredientWithUnit,
  type RecipeExpressionAstNodeData,
} from "../../utils/recipe-expressions/md-to-expressions";
import { useContext, useEffect, useState } from "preact/hooks";
import { unified } from "unified";
import remarkParse from "remark-parse";
import type { Root } from "mdast";
import type {
  RecipeIngredientFreeformModel,
  RecipeIngredientWithUnitModel,
} from "../../utils/recipe-expressions/expression-types";
import { createContext } from "preact";
import { visitParents } from "unist-util-visit-parents";

/**
 * The model for the recipe details context
 */
interface RecipeDetailsContextModel {
  /**
   * The markdown text
   */
  markdownText: string;
  /**
   * the expressions in order of walking the tree
   */
  expressions: RecipeExpressionAstNodeData[];
  expressionByPosition: Record<number, RecipeExpressionAstNodeData>;
  /**
   * Mapping from proper command name to the actual expressions index from the tree
   */
  expressionVariables: Record<string, RecipeExpressionAstNodeData>;
  /**
   * the markdown AST tree root
   */
  mdAstRoot: Root;
}

/**
 * The recipe details context
 */
const RecipeDetailsContext = createContext<RecipeDetailsContextModel | null>(
  null,
);

type ExtendedNodeTypes<T> =
  | T
  | {
      type: "recipeIngredientWithUnit";
      data: { position: number; expression: RecipeIngredientWithUnitModel };
    }
  | {
      type: "recipeIngredientFreeform";
      data: { position: number; expression: RecipeIngredientFreeformModel };
    };

/** renders the recipe details */
export function DetailsRender(props: DetailsRenderProps) {
  const [detailsContextModel, setDetailsContextModel] =
    useState<RecipeDetailsContextModel | null>(null);

  useEffect(() => {
    if (props.detailsMd) {
      const processor = unified().use(remarkParse);
      const ast = processor.parse(props.detailsMd);
      recipeExpressionSyntax()(ast);

      const expressions: RecipeExpressionAstNodeData[] = [];
      const expressionByPosition: Record<number, RecipeExpressionAstNodeData> =
        {};
      visitParents(ast, (node) => {
        const extendedNode: ExtendedNodeTypes<typeof node> =
          node as unknown as ExtendedNodeTypes<typeof node>;
        switch (extendedNode.type) {
          case RecipeFreeformIngredientElement:
          case RecipeIngredientWithUnit:
            expressions.push(extendedNode.data);
            expressionByPosition[extendedNode.data.position] =
              extendedNode.data;
            break;
        }
      });

      setDetailsContextModel({
        markdownText: props.detailsMd,
        expressionVariables: {},
        expressions: expressions,
        mdAstRoot: ast,
        expressionByPosition: expressionByPosition,
      });
      return;
    }
    setDetailsContextModel(null);
  }, [props.detailsMd]);

  return (
    <>
      <RecipeDetailsContext.Provider value={detailsContextModel}>
        <h4>Recipe Details</h4>
        <IngredientList />
        <div className={detailsRenderStyleModule.detailsWrapper}>
          <DetailsMdRender {...props} />
        </div>
      </RecipeDetailsContext.Provider>
    </>
  );
}

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

/** props for  `DetailsRender` */
export interface DetailsRenderProps {
  detailsMd?: string;
  mayEdit: boolean;
  goToEditAction: () => void;
}

function IngredientList() {
  const detailsContext = useContext(RecipeDetailsContext);

  if (!detailsContext) {
    return null;
  }

  return (
    <>
      <ul>
        {detailsContext.expressions.map((node) => (
          <li key={node.position}>{node.expression.text}</li>
        ))}
      </ul>
    </>
  );
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
      remarkPlugins={[recipeExpressionSyntax]}
      allowedElements={customSchema.tagNames}
      remarkRehypeOptions={{
        // @ts-expect-error not picking up hast custom elements properly
        handlers: RecipeAstToHastHandlers,
      }}
      components={{
        // @ts-expect-error not picking up hast custom elements properly
        recipeExpressionNode: RecipeIngredientRender,
      }}
    >
      {detailsMd}
    </Markdown>
  );
}

function RecipeIngredientRender({ position }: { position: number }) {
  const detailsContext = useContext(RecipeDetailsContext);

  if (!detailsContext) {
    return null;
  }

  const node = detailsContext.expressionByPosition[position];
  if (!node) {
    return null;
  }

  switch (node.expression.tag) {
    case "ingredient-freeform":
      return (
        <span className={detailsRenderStyleModule.ingredientText}>
          {node.expression.text}
        </span>
      );
    case "ingredient-with-unit":
      return (
        <span className={detailsRenderStyleModule.ingredientText}>
          {node.expression.amountText} {node.expression.unit}
          {" of "}
          {node.expression.text}
        </span>
      );
  }

  return null;
}
