import { RecipeDetailsContext } from "./recipe-details-context.component";
import { usePipeline } from "./usePipeline.hook";
import { DetailsMdRender } from "./details-md-render.component";
import { ToolList } from "./tool-list.component";
import { RecipeIngredientRender } from "./recipe-ingredient-render.component";
import { RecipeToolRender } from "./recipe-tool-render.component";
import { IngredientList } from "./ingredient-list.component";
import Paper from "@mui/material/Paper";
import Typography from "@mui/material/Typography";

const componentMap = {
  recipeIngredientExpression: RecipeIngredientRender,
  recipeToolExpression: RecipeToolRender,
};

/** renders the recipe details */
export function RecipeDetailsViewer({
  detailsMd,
  mayEdit,
  goToEditAction,
}: IRecipeDetailsViewerProps) {
  const detailsContextModel = usePipeline(detailsMd, componentMap);

  return (
    <>
      <RecipeDetailsContext.Provider value={detailsContextModel}>
        <ToolList />
        <IngredientList />
        <Paper sx={{ p: 2, mt: 2 }}>
          <Typography variant="h5">Details</Typography>
          <DetailsMdRender mayEdit={mayEdit} goToEditAction={goToEditAction} />
        </Paper>
      </RecipeDetailsContext.Provider>
    </>
  );
}

/** props for  `RecipeDetailsViewer` */
export interface IRecipeDetailsViewerProps {
  detailsMd?: string;
  mayEdit: boolean;
  goToEditAction: () => void;
}
