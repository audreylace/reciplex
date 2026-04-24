import detailsRenderStyleModule from "./details-render.module.css";
import { useContext, useMemo } from "preact/hooks";
import { RecipeDetailsContext } from "./recipe-details-context.component";
import { usePipeline } from "./usePipeline.hook";
import Paper from "@mui/material/Paper";
import Box from "@mui/material/Box";
import { Typography } from "@mui/material";

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
        <Typography variant="h5">Tools</Typography>
        <ToolList />
        <Typography variant="h5">Ingredients</Typography>
        <IngredientList />
        <Paper>
          <Box sx={{ p: 1 }}>
            <DetailsMdRender
              mayEdit={mayEdit}
              goToEditAction={goToEditAction}
            />
          </Box>
        </Paper>
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

    return detailsContext.ingredientCollection.getIngredientList();
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

    return detailsContext.toolCollection.getToolList();
  }, [detailsContext?.toolCollection]);

  if (!list) {
    return null;
  }

  return (
    <>
      <ul>
        {list.map((listEntry) => {
          let unitString = "";
          if (listEntry.sizeUnit && listEntry.size) {
            unitString = ` - ${listEntry.size} ${listEntry.sizeUnit}`;
          }
          return (
            <li key={listEntry.key}>
              {listEntry.toolName} x{listEntry.quantity}
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
        <Typography variant="body2">
          <i>No details</i>
        </Typography>
      );
    }
    return (
      <Typography
        variant="body2"
        onClick={goToEditAction}
        sx={{
          cursor: "pointer",
        }}
      >
        <i>Click to edit and add details</i>
      </Typography>
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
        {amount} {unit} {inlineText}
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

  const toolName = concept.toolName;
  const inlineText = concept.getInlineText(position);
  const toolSizeUnit = concept.toolSizeUnit;
  const toolSize = concept.toolSize;
  const toolQuantity = concept.getToolQuantityByPosition(position);

  if (inlineText) {
    return (
      <span className={detailsRenderStyleModule.ingredientText}>
        {inlineText}
      </span>
    );
  }

  return (
    <span className={detailsRenderStyleModule.ingredientText}>
      {toolSize} {toolSizeUnit} {toolQuantity !== 1 ? "x" + toolQuantity : ""}
      {toolName}
    </span>
  );
}
