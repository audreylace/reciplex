import { useNavigate } from "react-router";
import { makeRecipeNameAndDescriptionState } from "../../components/recipe-title-and-description/recipe-title-and-description.component";
import { makeViewRecipePath } from "../../route-utils";
import type { IRecipeModel } from "../../services/recipe-types";

import styles from "./recipe-list-table.module.css";

export function RecipeListRow({ recipe }: { recipe: IRecipeModel }) {
  const navigate = useNavigate();
  const path = makeViewRecipePath(recipe.bookId, recipe.id);
  const state = makeRecipeNameAndDescriptionState(
    recipe.name,
    recipe.shortDescription,
  );
  return (
    <tr
      onClick={() => navigate(path, { state })}
      className={styles.recipeListRow}
    >
      <td className={styles.tableColumnRecipeName}>{recipe.name}</td>
      <td className={styles.tableColumnRecipeDescription}>
        {recipe.shortDescription}
      </td>
    </tr>
  );
}
