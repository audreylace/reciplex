import { useNavigate } from "react-router";
import { makeRecipeNameAndDescriptionState } from "../../components/recipe-title-and-description/recipe-title-and-description.component";
import { makeViewRecipePath } from "../../route-utils";
import type { IRecipeModel } from "../../services/recipe-types";

import styles from "./recipe-list-table.module.css";
import { useEffect, useRef } from "preact/hooks";

export function RecipeListRow({
  recipe,
  focus,
}: {
  recipe: IRecipeModel;
  focus?: boolean;
}) {
  const navigate = useNavigate();
  const path = makeViewRecipePath(recipe.bookId, recipe.id);
  const state = makeRecipeNameAndDescriptionState(
    recipe.name,
    recipe.shortDescription,
  );
  const rowRef = useRef<HTMLTableRowElement | null>(null);
  useEffect(() => {
    if (rowRef.current && focus) {
      rowRef.current.focus();
    }
  }, [focus]);

  return (
    <tr
      onClick={() => navigate(path, { state })}
      onKeyDown={(e) => {
        if (e.key === " ") {
          navigate(path, { state });
          e.preventDefault();
        }
      }}
      onKeyUp={(e) => {
        if (e.key === " " || e.key === "Enter") {
          navigate(path, { state });
          e.preventDefault();
        }
      }}
      className={styles.recipeListRow}
      tabIndex={0}
      ref={rowRef}
    >
      <td className={styles.tableColumnRecipeName}>{recipe.name}</td>
      <td className={styles.tableColumnRecipeDescription}>
        {recipe.shortDescription}
      </td>
    </tr>
  );
}
