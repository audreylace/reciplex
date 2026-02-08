import { useLocation, useNavigate } from "react-router";
import { useCallback } from "preact/hooks";
import {
  makeBookListPath,
  makeViewRecipeBookPath,
} from "../../features/recipes/route-utils";
import { CreateRecipeBookForm } from "./components/create-recipe-book-form/create-recipe-book-form.component";
import type { IRecipeBookModel } from "../../services/recipe-store";
import styles from "./create-recipe-book.module.css";

/**
 * Entry point for create recipe book page component
 * @param param0 react props
 * @returns jsx tree for rendering by react
 */
export function CreateRecipeBookPage({}: {}) {
  const navigate = useNavigate();
  const location = useLocation();
  const { goBack } = location.state || {};

  /**
   * Runs action on form submit creating a new recipe book
   * @param data form data
   * @returns void promise
   */
  const onCreated = useCallback(
    async (data: IRecipeBookModel) => {
      navigate(makeViewRecipeBookPath(data.id));
    },
    [navigate],
  );

  const onCancel = useCallback(async () => {
    if (goBack) {
      navigate(-1);
    }
    navigate(makeBookListPath());
  }, [navigate, goBack]);

  return (
    <main className={styles.pageWrapper}>
      <CreateRecipeBookForm onCreated={onCreated} onCancel={onCancel} />
    </main>
  );
}
