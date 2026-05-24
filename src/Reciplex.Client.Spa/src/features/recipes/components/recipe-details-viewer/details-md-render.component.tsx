import { useContext } from "react";
import type { IRecipeDetailsViewerProps } from "./recipe-details-viewer.component";
import Typography from "@mui/material/Typography";
import { RecipeDetailsContext } from "./recipe-details-context.component";
import Box from "@mui/material/Box";

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

  return (
    <Box
      sx={{
        ul: {
          pl: 3,
          mb: 1,
          mt: 0,
          pt: 0,
        },
        ol: {
          pl: 3,
          mb: 1,
          mt: 0,
          pt: 0,
        },
      }}
    >
      {detailsContext?.component}
    </Box>
  );
}

export interface IDetailsMdRenderProps {
  mayEdit: IRecipeDetailsViewerProps["mayEdit"];
  goToEditAction: IRecipeDetailsViewerProps["goToEditAction"];
}
