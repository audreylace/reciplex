import { useNavigate } from "react-router";
import { useCallback } from "preact/hooks";
import { makeViewRecipeBookPath } from "../../features/recipes/route-utils";
import { CreateRecipeBookForm } from "../../features/recipes/components/create-recipe-book-form/create-recipe-book-form.component";
import type { IRecipeBookModel } from "../../services/recipe-store";
import { useSetTitle } from "../../layouts/default/default-layout.state";

/**
 * Entry point for create recipe book page component
 * @param param0 react props
 * @returns jsx tree for rendering by react
 */
export function CreateRecipeBookPage({}: {}) {
  const navigate = useNavigate();

  useSetTitle("Create Recipe Book");

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

  return (
    <main>
      <CreateRecipeBookForm onCreated={onCreated} />
    </main>
  );
}
