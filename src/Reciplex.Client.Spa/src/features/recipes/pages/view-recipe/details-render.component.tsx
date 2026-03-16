import detailsRenderStyleModule from "./details-render.module.css";
import { useContext, useMemo } from "preact/hooks";
import { RecipeDetailsContext } from "./recipe-details-context.component";
import { usePipeline } from "./usePipeine.hook";

const componentMap = {
  recipeIngredientExpression: RecipeIngredientRender,
  recipeToolExpression: RecipeToolRender,
};

/** renders the recipe details */
export function DetailsRender({
  detailsMd,
  mayEdit,
  goToEditAction,
}: IDetailsRenderProps) {
  const detailsContextModel = usePipeline(detailsMd, componentMap);

  return (
    <>
      <RecipeDetailsContext.Provider value={detailsContextModel}>
        <h4>Tools</h4>
        <ToolList />
        <h4>Ingredients</h4>
        <IngredientList />
        <div className={detailsRenderStyleModule.detailsWrapper}>
          <DetailsMdRender mayEdit={mayEdit} goToEditAction={goToEditAction} />
        </div>
      </RecipeDetailsContext.Provider>
    </>
  );
}

/** props for  `DetailsRender` */
export interface IDetailsRenderProps {
  detailsMd?: string;
  mayEdit: boolean;
  goToEditAction: () => void;
}

function IngredientList() {
  const detailsContext = useContext(RecipeDetailsContext);

  const list = useMemo(() => {
    if (!detailsContext?.ingredientCollection) {
      return null;
    }

    return detailsContext.ingredientCollection.getList();
  }, [detailsContext?.ingredientCollection]);

  if (!list) {
    return null;
  }

  return (
    <>
      <ul>
        {list.map((listEntry) => {
          let unitString = "";
          if (listEntry.unit && listEntry.amount) {
            unitString = ` - ${listEntry.amount} ${listEntry.unit}`;
          } else if (listEntry.amount) {
            unitString = ` - ${listEntry.amount}`;
          }
          return (
            <li key={listEntry.key}>
              {listEntry.title}
              {unitString}
            </li>
          );
        })}
      </ul>
    </>
  );
}

function ToolList() {
  const detailsContext = useContext(RecipeDetailsContext);

  const list = useMemo(() => {
    if (!detailsContext?.toolCollection) {
      return null;
    }

    return detailsContext.toolCollection.getList();
  }, [detailsContext?.toolCollection]);

  if (!list) {
    return null;
  }

  return (
    <>
      <ul>
        {list.map((listEntry) => {
          let unitString = "";
          if (listEntry.unit && listEntry.amount) {
            unitString = ` - ${listEntry.amount} ${listEntry.unit}`;
          } else if (listEntry.amount) {
            unitString = ` - ${listEntry.amount}`;
          }
          return (
            <li key={listEntry.key}>
              {listEntry.title}
              {unitString}
            </li>
          );
        })}
      </ul>
    </>
  );
}

/** inner md render */
function DetailsMdRender({
  mayEdit,
  goToEditAction,
}: {
  mayEdit: IDetailsRenderProps["mayEdit"];
  goToEditAction: IDetailsRenderProps["goToEditAction"];
}) {
  const detailsContext = useContext(RecipeDetailsContext);
  if (!detailsContext?.component) {
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

  return detailsContext?.component;
}

function RecipeIngredientRender({ position }: { position: number }) {
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
      <span className={detailsRenderStyleModule.ingredientText}>
        {amount} {unit}
        {" of "}
        {inlineText}
      </span>
    );
  }

  return (
    <span className={detailsRenderStyleModule.ingredientText}>
      {inlineText}
    </span>
  );
}

function RecipeToolRender({ position }: { position: number }) {
  const detailsContext = useContext(RecipeDetailsContext);

  if (!detailsContext) {
    return null;
  }

  const concept = detailsContext.toolCollection.getConceptByPosition(position);
  if (!concept) {
    return null;
  }

  const inlineText = concept.getRenderText(position);
  const unit = concept.unitText;
  const amount = concept.getAmountByPosition(position);

  if (amount) {
    return (
      <span className={detailsRenderStyleModule.ingredientText}>
        {amount} {unit}
        {inlineText}
      </span>
    );
  }

  return (
    <span className={detailsRenderStyleModule.ingredientText}>
      {inlineText}
    </span>
  );
}
