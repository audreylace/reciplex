import Markdown from "react-markdown";
import rehypeSanitize, { defaultSchema } from "rehype-sanitize";
import { visit } from "unist-util-visit";

import { findAndReplace } from "mdast-util-find-and-replace";

import detailsRenderStyleModule from "./details-render.module.css";
import { extractRecipeSyntax } from "../../utils/recipe-expressions/lexer2";
import type { HTMLAttributes } from "preact";

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
      remarkPlugins={[extractRecipeSyntax]}
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
        console.log(`${value} : ${node}`);
        console.log(node);
        //console.log(selectedString);

        return { type: "recipe-expression", value: value };
        // return "gotcha";
      },
    ]);
  };
}
