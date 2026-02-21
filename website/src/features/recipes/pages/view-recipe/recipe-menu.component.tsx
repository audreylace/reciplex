import { useNavigate } from "react-router";
import { makeDeleteRecipePath, makeEditRecipePath } from "../../route-utils";
import {
  DropDownMenu,
  type MenuSpecs,
} from "../../../core/components/drop-down-menu/drop-down-menu.component";
import { useMemo } from "preact/hooks";
import { makeRecipeNameAndDescriptionState } from "../../components/recipe-title-and-description/recipe-title-and-description.component";

import recipeMenuStylesModule from "./recipe-menu.module.css";

/** menu for recipe book actions */
export function RecipeMenu({
  bookId,
  name,
  shortDescription,
  mayEdit,
  recipeId,
}: {
  /** the id of the book */
  bookId: string;
  recipeId: string;
  /** the name of the recipe */
  name?: string;
  /** the short description of the recipe */
  shortDescription: string;
  /** if the user has edit privileges */
  mayEdit?: boolean;
}) {
  const navigate = useNavigate();
  const menuSettings: MenuSpecs = useMemo(() => {
    if (!mayEdit) {
      return [];
    }

    return [
      {
        key: "edit",
        caption: "Edit Recipe",
        icon: "bi bi-pencil",
        type: "entry",
        onClick: () => {
          navigate(makeEditRecipePath(bookId, recipeId), {
            state: makeRecipeNameAndDescriptionState(name, shortDescription),
          });
        },
      },
      {
        key: "delete",
        caption: "Delete Recipe",
        type: "entry",
        onClick: () => {
          navigate(makeDeleteRecipePath(bookId, recipeId), {
            state: makeRecipeNameAndDescriptionState(name, shortDescription),
          });
        },
        icon: "bi bi-trash",
      },
    ] as MenuSpecs;
  }, [bookId, mayEdit, name, navigate, recipeId, shortDescription]);

  if (!mayEdit) {
    return null;
  }

  return (
    <div className={recipeMenuStylesModule.menu}>
      <DropDownMenu
        menu={menuSettings}
        icon="bi bi-lightning-charge"
        caption="Actions"
      />
    </div>
  );
}
