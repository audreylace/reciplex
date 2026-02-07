import { NavLink } from "react-router";
import styles from "./create-recipe-book-button.module.css";

export function CreateRecipeBookButton() {
  return (
    <NavLink to="/create-recipe-book" className={styles.createButton}>
      <span className={styles.dottedCircle}>
        <i class="bi bi-plus-circle-dotted"></i>
      </span>
      <span className={styles.solidCircle}>
        <i class="bi bi-plus-circle-fill"></i>
      </span>{" "}
      Add
    </NavLink>
  );
}
