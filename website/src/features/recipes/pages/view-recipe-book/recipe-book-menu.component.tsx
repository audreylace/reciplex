import { useNavigate } from "react-router";

import { SuccessButton } from "../../../core/components/success-button/success-button.component";
import { makeBookNameAndDescriptionState } from "../../components/book-information-banner/book-information-banner";
import { makeCreateRecipePath } from "../../route-utils";

import styles from "./recipe-book-menu.module.css";
import {
  Menu,
  MenuButton,
  MenuItem,
  MenuItems,
  MenuSeparator,
} from "@headlessui/react";
import { Fragment } from "preact/jsx-runtime";

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
  const bookNavState = makeBookNameAndDescriptionState(name, shortDescription);
  const navigate = useNavigate();

  if (!mayEdit && !mayDelete) {
    return null;
  }

  return (
    <div className={styles.menu}>
      <Menu>
        <MenuButton as={Fragment}>
          <div>
            <SuccessButton buttonType="dotted">
              <i className="bi bi-lightning-charge"></i> Actions
            </SuccessButton>
          </div>
        </MenuButton>
        <MenuItems anchor="bottom" className={styles.dropDownMenuContainer}>
          <ul className={styles.dropDownMenu}>
            {mayEdit && (
              <MenuItem
                as="li"
                onClick={() => navigate(makeCreateRecipePath(bookId))}
              >
                <i className="bi bi-plus-circle-dotted"></i> Add Recipe
              </MenuItem>
            )}
            <MenuSeparator className={styles.menuSeparator} />
            {mayDelete && (
              <MenuItem
                as="li"
                onClick={() => {
                  navigate(`/books/${bookId}/delete`, { state: bookNavState });
                }}
              >
                <i className="bi bi-trash"></i> Delete Book
              </MenuItem>
            )}
            {mayEdit && (
              <MenuItem
                as="li"
                onClick={() => {
                  navigate(`/books/${bookId}/edit`, { state: bookNavState });
                }}
              >
                <i className="bi bi-gear"></i> Book Settings
              </MenuItem>
            )}
          </ul>
        </MenuItems>
      </Menu>
    </div>
  );
}
