import { NavLink, useNavigate } from "react-router";
import styles from "./create-recipe-book-button.module.css";
import { SuccessButton } from "../../../../../core/components/success-button/success-button.component";

export function CreateRecipeBookButton() {
  const navigation = useNavigate();
  return (
    <SuccessButton
      className={styles.createButton}
      onClick={(e) => {
        e.preventDefault();
        navigation("/create-recipe-book", {
          state: { goBack: true },
        });
      }}
      buttonType="hidden"
    >
      <span className={styles.dottedCircle}>
        <i class="bi bi-plus-circle-dotted"></i>
      </span>
      <span className={styles.solidCircle}>
        <i class="bi bi-plus-circle-fill"></i>
      </span>{" "}
      Add
    </SuccessButton>
  );
}
