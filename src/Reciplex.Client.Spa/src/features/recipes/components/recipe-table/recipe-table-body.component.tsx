import TableBody from "@mui/material/TableBody";
import type { IRecipeModel } from "../../services/recipe-types";
import TableRow from "@mui/material/TableRow";
import TableCell from "@mui/material/TableCell";
import { useNavigate } from "react-router";
import { makeViewRecipePath } from "../../route-utils";
import { RecipeMenuButtonCell } from "./recipe-menu-button-cell.component";

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

function RecipeTableRow({ recipe }: { recipe: IRecipeModel }) {
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
