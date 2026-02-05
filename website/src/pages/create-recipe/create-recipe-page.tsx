import { useNavigate, useParams } from "react-router";
import { makeViewRecipePath } from "../../features/recipes/route-utils";
import { useGetRecipeBookById } from "../../features/recipes/hooks/useGetRecipeBookById.hook";
import { BookIsReadonlyBanner } from "../../features/recipes/components/book-is-readonly-banner/book-is-readonly-banner.component";
import { RecipeBookNotFoundBanner } from "../../features/recipes/components/recipe-book-not-found-banner/recipe-book-not-found-banner.component";
import { FetchingRecipeBookBanner } from "../../features/recipes/components/fetching-recipe-book-banner/fetching-recipe-book-banner.component";
import { FetchingRecipeBookFailedBanner } from "../../features/recipes/components/fetching-recipe-book-failed-banner/fetching-recipe-book-failed-banner.component";
import { BadPathBanner } from "../../features/recipes/components/bad-path-banner/bad-path-banner.component";
import type { IRecipeModel } from "../../services/recipe-store";
import { CreateRecipeForm } from "../../features/recipes/components/create-recipe-form/create-recipe-form.component";
import { useCallback } from "preact/hooks";
import { useSetTitle } from "../../layouts/default/default-layout.state";

/**
 * Entry point for create recipe page component
 * @param param0 react props
 * @returns jsx tree for rendering by react
 */
export function CreateRecipePage({}: {}) {
  const { bookId } = useParams<{
    bookId: string;
  }>();
  const navigate = useNavigate();
  const bookQuery = useGetRecipeBookById(bookId);
  useSetTitle("Adding Recipe");
  /**
   * Runs action on form submit creating a new recipe
   * @param data form data
   * @returns void promise
   */
  const onRecipeCreated = useCallback(
    (recipe: IRecipeModel) => {
      navigate(makeViewRecipePath(recipe.id));
    },
    [navigate],
  );

  const bookData = bookQuery.data;
  const notFound = bookQuery.isSuccess && !bookData;
  const canCreateRecipe = bookData && bookData.canAddRecipesToBook;
  const showReadonlyBanner =
    bookQuery.isSuccess && bookData && !bookData.canAddRecipesToBook;

  return (
    <main>
      {!bookId && <BadPathBanner />}
      {bookId && (
        <>
          {bookQuery.isError && <FetchingRecipeBookFailedBanner />}
          {bookQuery.isLoading && <FetchingRecipeBookBanner />}
          {notFound && <RecipeBookNotFoundBanner />}
          {showReadonlyBanner && <BookIsReadonlyBanner bookId={bookData.id} />}
          {canCreateRecipe && (
            <CreateRecipeForm
              bookId={bookId}
              bookName={bookData.name}
              onCreated={onRecipeCreated}
            />
          )}
        </>
      )}
    </main>
  );
}
