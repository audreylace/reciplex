import TableBody from "@mui/material/TableBody";
import type { IRecipeModel } from "../../services/recipe-types";
import TableRow from "@mui/material/TableRow";
import TableCell from "@mui/material/TableCell";
import { useNavigate } from "react-router";
import { makeViewRecipePath } from "../../route-utils";
import IconButton from "@mui/material/IconButton";
import { MoreVert } from "@mui/icons-material";

export function RecipeTableBody({
  recipes,
}: {
  recipes: IRecipeModel[] | undefined;
}) {
  return (
    <TableBody>
      <TableRow></TableRow>
      {recipes && recipes.length <= 0 && (
        <TableRow>
          <TableCell colspan={3} align="center">
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
  return (
    <TableRow
      hover
      onClick={() => {
        navigate(makeViewRecipePath(recipe.bookId, recipe.id));
      }}
    >
      <TableCell>{recipe.name}</TableCell>
      <TableCell>{recipe.details}</TableCell>
      <TableCell scope="row" component="th">
        <IconButton onClick={(e) => e.stopPropagation()}>
          <MoreVert />
        </IconButton>
      </TableCell>
    </TableRow>
  );
}
