import { useContext } from "preact/hooks";
import { RecipeDetailsContext } from "./recipe-details-context.component";

export function RecipeToolRender({ position }: IRecipeToolRenderProps) {
  const detailsContext = useContext(RecipeDetailsContext);

  if (!detailsContext) {
    return null;
  }

  const concept = detailsContext.toolCollection.getConceptByPosition(position);
  if (!concept) {
    return null;
  }

  const toolName = concept.toolName;
  const inlineText = concept.getInlineText(position);
  const toolSizeUnit = concept.toolSizeUnit;
  const toolSize = concept.toolSize;
  const toolQuantity = concept.getToolQuantityByPosition(position);

  if (inlineText) {
    return <span>{inlineText}</span>;
  }

  return (
    <span>
      {toolSize} {toolSizeUnit} {toolQuantity !== 1 ? "x" + toolQuantity : ""}
      {toolName}
    </span>
  );
}

export interface IRecipeToolRenderProps {
  position: number;
}
