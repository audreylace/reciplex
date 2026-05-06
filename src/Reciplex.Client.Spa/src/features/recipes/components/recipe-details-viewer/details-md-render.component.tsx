import { useContext } from "preact/hooks";
import type { IRecipeDetailsViewerProps } from "./recipe-details-viewer.component";
import Typography from "@mui/material/Typography";
import { RecipeDetailsContext } from "./recipe-details-context.component";

/** inner md render */
export function DetailsMdRender({
  mayEdit,
  goToEditAction,
}: IDetailsMdRenderProps) {
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

export interface IDetailsMdRenderProps {
  mayEdit: IRecipeDetailsViewerProps["mayEdit"];
  goToEditAction: IRecipeDetailsViewerProps["goToEditAction"];
}
