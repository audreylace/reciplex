import TableBody from "@mui/material/TableBody";
import type { IRecipeModel } from "../../services/recipe-types";
import TableRow from "@mui/material/TableRow";
import TableCell from "@mui/material/TableCell";
import { RecipeTableRow } from "./recipe-table-row.component";

export function RecipeTableBody({
  recipes,
}: {
  recipes: IRecipeModel[] | undefined;
}) {
  return (
    <TableBody>
      {recipes && recipes.length <= 0 && (
        <TableRow>
          <TableCell colSpan={3} align="center">
            No Recipes
          </TableCell>
        </TableRow>
      )}
      {recipes?.map((recipe) => (
        <RecipeTableRow key={recipe.id} recipe={recipe} />
      ))}
    </TableBody>
  );
}
