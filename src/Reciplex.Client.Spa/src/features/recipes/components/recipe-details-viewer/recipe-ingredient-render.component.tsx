import { useContext } from "preact/hooks";
import { RecipeDetailsContext } from "./recipe-details-context.component";

export function RecipeIngredientRender({
  position,
}: IRecipeIngredientRenderProps) {
  const detailsContext = useContext(RecipeDetailsContext);

  if (!detailsContext) {
    return null;
  }

  const concept =
    detailsContext.ingredientCollection.getConceptByPosition(position);
  if (!concept) {
    return null;
  }

  const inlineText = concept.getRenderText(position);
  const unit = concept.unitText;
  const amount = concept.getAmountByPosition(position);

  if (amount) {
    return (
      <span>
        {amount} {unit} {inlineText}
      </span>
    );
  }

  return <span>{inlineText}</span>;
}

export interface IRecipeIngredientRenderProps {
  position: number;
}
