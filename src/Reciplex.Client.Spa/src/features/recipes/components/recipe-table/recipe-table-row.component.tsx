import { useNavigate } from "react-router";
import { makeViewRecipePath } from "../../route-utils";
import type { IRecipeModel } from "../../services/recipe-types";
import { RecipeMenuButtonCell } from "./recipe-menu-button-cell.component";
import TableCell from "@mui/material/TableCell";
import TableRow from "@mui/material/TableRow";

export function RecipeTableRow({ recipe }: { recipe: IRecipeModel }) {
  const navigate = useNavigate();
  const onClick = () => {
    navigate(makeViewRecipePath(recipe.bookId, recipe.id));
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
        recipeId={recipe.id}
        bookId={recipe.bookId}
        mayEdit={recipe.mayEdit}
      />
    </TableRow>
  );
}
