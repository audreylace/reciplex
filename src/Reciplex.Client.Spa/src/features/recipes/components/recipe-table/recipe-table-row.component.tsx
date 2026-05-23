import { useNavigate } from "react-router";
import { makeViewRecipePath } from "../../route-utils";
import type { IRecipeListEntryJsonResponse } from "../../services/recipe-types";
import { RecipeMenuButtonCell } from "./recipe-menu-button-cell.component";
import TableCell from "@mui/material/TableCell";
import TableRow from "@mui/material/TableRow";

/** a single row in a recipe list */
export function RecipeTableRow({ recipe, mayEdit }: IRecipeTableRowProps) {
  const navigate = useNavigate();
  const onClick = () => {
    navigate(makeViewRecipePath(recipe.bookKey, recipe.recipeKey));
  };
  return (
    <TableRow
      hover
      sx={{
        ":hover": {
          cursor: "pointer",
        },
      }}
    >
      <TableCell onClick={onClick} role="button">
        {recipe.name}
      </TableCell>
      <TableCell onClick={onClick} role="button">
        {recipe.shortDescription}
      </TableCell>
      <RecipeMenuButtonCell
        recipeName={recipe.name}
        recipeId={recipe.recipeKey}
        bookId={recipe.bookKey}
        mayEdit={mayEdit}
      />
    </TableRow>
  );
}

/** props for `<RecipeTableRow />` */
export interface IRecipeTableRowProps {
  /** recipe model for this row */
  recipe: IRecipeListEntryJsonResponse;
  /** if the user can edit this recipe */
  mayEdit: boolean;
}
