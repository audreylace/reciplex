import { useNavigate, useParams } from "react-router";
import { makeViewRecipePath } from "../../route-utils";
import { useGetRecipeBookById } from "../../hooks/useGetRecipeBookById.hook";
import { BookIsReadonlyBanner } from "../../components/book-is-readonly-banner/book-is-readonly-banner.component";
import { RecipeBookNotFoundBanner } from "../../components/recipe-book-not-found-banner/recipe-book-not-found-banner.component";
import { FetchingRecipeBookBanner } from "../../components/fetching-recipe-book-banner/fetching-recipe-book-banner.component";
import { FetchingRecipeBookFailedBanner } from "../../components/fetching-recipe-book-failed-banner/fetching-recipe-book-failed-banner.component";
import { BadPathBanner } from "../../components/bad-path-banner/bad-path-banner.component";
import type { IRecipeModel } from "../../../../services/recipe-store";
import { CreateRecipeForm } from "../../components/create-recipe-form/create-recipe-form.component";
import { useCallback } from "preact/hooks";

/**
 * Entry point for create recipe page component
 * @param param0 react props
 * @returns jsx tree for rendering by react
 */
export function CreateRecipePage() {
  const { bookId } = useParams<{
    bookId: string;
  }>();
  const navigate = useNavigate();
  const bookQuery = useGetRecipeBookById(bookId);

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
    <main className="pageMain">
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
