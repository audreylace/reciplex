import TableBody from "@mui/material/TableBody";
import type { IRecipeListEntryJsonResponse } from "../../services/recipe-types";
import TableRow from "@mui/material/TableRow";
import TableCell from "@mui/material/TableCell";
import { RecipeTableRow } from "./recipe-table-row.component";

export function RecipeTableBody({ recipes, mayEdit }: IRecipeTableBodyProps) {
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
        <RecipeTableRow
          key={recipe.recipeKey}
          recipe={recipe}
          mayEdit={mayEdit}
        />
      ))}
    </TableBody>
  );
}

/**
 * props for `<RecipeTableBody />`
 */
export interface IRecipeTableBodyProps {
  /** list of recipes */
  recipes: IRecipeListEntryJsonResponse[] | undefined;
  /** if the user can edit the recipes */
  mayEdit: boolean;
}
