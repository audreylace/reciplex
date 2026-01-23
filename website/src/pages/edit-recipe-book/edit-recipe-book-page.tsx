import { useParams } from "react-router";
import { useGetRecipeBookById } from "../../features/recipes/hooks/useGetRecipeBookById.hook";
import { RecipeBookNotFoundBanner } from "../../features/recipes/components/recipe-book-not-found-banner/recipe-book-not-found-banner.component";
import { FetchingRecipeBookBanner } from "../../features/recipes/components/fetching-recipe-book-banner/fetching-recipe-book-banner.component";
import { FetchingRecipeBookFailedBanner } from "../../features/recipes/components/fetching-recipe-book-failed-banner/fetching-recipe-book-failed-banner.component";
import { BadPathBanner } from "../../features/recipes/components/bad-path-banner/bad-path-banner.component";
import { ActionBanner } from "../../features/recipes/components/action-banner/action-banner.component";
import { makeViewRecipeBookPath } from "../../features/recipes/route-utils";
import { RecipeBookMetaFields } from "../../features/recipes/components/recipe-book-meta-fields/recipe-book-meta-fields.component";
import { useForm, type SubmitHandler } from "react-hook-form";

/**
 * Entry point for editing a recipe book
 * @param param0 react props
 * @returns jsx tree for rendering by react
 */
export function EditRecipeBookPage({}: {}) {
  const { bookId } = useParams<{
    bookId: string;
  }>();
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<FormFields>();
  const bookQuery = useGetRecipeBookById(bookId); // TODO: Need to fetch the same way as we do for edit recipe and then lock state
  const notFound = bookQuery.isSuccess && !bookQuery.data;
  const onSubmit: SubmitHandler<FormFields> = async (data) => {
    // todo - save changes on submit
  };
  return (
    <main>
      {!bookId && <BadPathBanner />}
      {bookId && (
        <>
          {bookQuery.isLoading && <FetchingRecipeBookBanner />}
          {notFound && <RecipeBookNotFoundBanner />}
          {bookQuery.isError && <FetchingRecipeBookFailedBanner />}
          {bookQuery.isSuccess && bookQuery.data && (
            <>
              {!bookQuery.data.canEditBookInformation && (
                <ActionBanner
                  to={makeViewRecipeBookPath(bookId)}
                  message="You may not edit this recipe book"
                  linkText="View recipe book"
                />
              )}
              {bookQuery.data.canEditBookInformation && (
                <>
                  <form onSubmit={handleSubmit(onSubmit)}>
                    <RecipeBookMetaFields
                      disabled={false /* TODO -- disable on submit */}
                      register={register}
                      legend={`Editing Recipe Book ${bookQuery.data.name}`}
                      errors={errors}
                    />
                    <input type="submit" value="Save" />
                  </form>
                </>
              )}
            </>
          )}
        </>
      )}
    </main>
  );
}

/**
 * Fields in the form
 */
type FormFields = {
  /**
   * name of the book; max length is 127 characters
   * @see RecipeBookNameMaxLength
   */
  bookName: string;
  /**
   * description of the book; max length is 255 characters
   * @see RecipeBookShortDescriptionMaxLength
   */
  bookDescription: string;
};
