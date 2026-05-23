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
  h1: ({ children }: { children?: React.ReactNode | undefined }) => (
    <Typography variant="h1">{children}</Typography>
  ),
  h2: ({ children }: { children?: React.ReactNode | undefined }) => (
    <Typography variant="h2">{children}</Typography>
  ),
  h3: ({ children }: { children?: React.ReactNode | undefined }) => (
    <Typography variant="h3">{children}</Typography>
  ),
  h4: ({ children }: { children?: React.ReactNode | undefined }) => (
    <Typography variant="h4">{children}</Typography>
  ),
  h5: ({ children }: { children?: React.ReactNode | undefined }) => (
    <Typography variant="h5">{children}</Typography>
  ),
  h6: ({ children }: { children?: React.ReactNode | undefined }) => (
    <Typography variant="h6">{children}</Typography>
  ),
  p: ({ children }: { children?: React.ReactNode | undefined }) => (
    <Typography variant="body1" gutterBottom>
      {children}
    </Typography>
  ),
};

/** renders the recipe details */
export function RecipeDetailsViewer({
  detailsMd,
  mayEdit,
  goToEditAction,
}: IRecipeDetailsViewerProps) {
  const detailsContextModel = usePipeline(
    detailsMd,
    //@ts-expect-error ts not able to handle the dynamic component types
    componentMap,
  );

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
