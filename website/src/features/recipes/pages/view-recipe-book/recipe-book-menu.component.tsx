import { useNavigate } from "react-router";
import { makeBookNameAndDescriptionState } from "../../components/book-information-banner/book-information-banner";
import { makeCreateRecipePath } from "../../route-utils";

import recipeBookMenuStylesModule from "./recipe-book-menu.module.css";

import {
  DropDownMenu,
  type MenuSpecs,
} from "../../../core/components/drop-down-menu/drop-down-menu.component";
import { useMemo } from "preact/hooks";

/** menu for recipe book actions */
export function RecipeBookMenu({
  bookId,
  name,
  shortDescription,
  mayEdit,
  mayDelete,
}: {
  /** the id of the book */
  bookId: string;
  /** the name of the book */
  name?: string;
  /** the short description of the book */
  shortDescription: string;
  /** if the user has edit privileges */
  mayEdit?: boolean;
  /** if the user has delete privileges */
  mayDelete?: boolean;
}) {
  const navigate = useNavigate();
  const menuSettings: MenuSpecs = useMemo(() => {
    const bookNavState = makeBookNameAndDescriptionState(
      name,
      shortDescription,
    );
    if (!mayEdit || !mayDelete) {
      return [];
    }

    return [
      {
        key: "add",
        caption: "Add Recipe",
        type: "entry",
        onClick: () => navigate(makeCreateRecipePath(bookId)),
        icon: "bi bi-plus-circle-dotted",
        hidden: !mayEdit,
      },
      { key: "separator", type: "separator", hidden: !mayEdit },
      {
        key: "settings",
        caption: "Book Settings",
        type: "entry",
        onClick: () => {
          navigate(`/books/${bookId}/edit`, { state: bookNavState });
        },
        icon: "bi bi-gear",
        hidden: !mayEdit,
      },
      {
        key: "delete",
        caption: "Delete Book",
        type: "entry",
        onClick: () => {
          navigate(`/books/${bookId}/delete`, { state: bookNavState });
        },
        icon: "bi bi-trash",
        hidden: !mayDelete,
      },
    ] as MenuSpecs;
  }, [bookId, mayDelete, mayEdit, name, navigate, shortDescription]);

  if (!mayEdit && !mayDelete) {
    return null;
  }

  return (
    <div className={recipeBookMenuStylesModule.menu}>
      <DropDownMenu
        menu={menuSettings}
        icon="bi bi-lightning-charge"
        caption="Actions"
      />
    </div>
  );
}
