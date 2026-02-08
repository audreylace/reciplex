import { useNavigate, useParams } from "react-router";
import { BadPathBanner } from "../../components/bad-path-banner/bad-path-banner.component";
import { ActionBanner } from "../../components/action-banner/action-banner.component";
import { makeBookListPath, makeViewRecipeBookPath } from "../../route-utils";
import { useForm, type SubmitHandler } from "react-hook-form";
import { useGetRecipeBookById } from "../../hooks/useGetRecipeBookById.hook";
import { FetchingRecipeBookBanner } from "../../components/fetching-recipe-book-banner/fetching-recipe-book-banner.component";
import { FetchingRecipeBookFailedBanner } from "../../components/fetching-recipe-book-failed-banner/fetching-recipe-book-failed-banner.component";
import { RecipeBookNotFoundBanner } from "../../components/recipe-book-not-found-banner/recipe-book-not-found-banner.component";
import { useDeleteRecipeBookMutation } from "../../hooks/useDeleteRecipeBookMutation";

export function DeleteRecipeBookPage({}: {}) {
  const navigate = useNavigate();
  const { bookId } = useParams<{ bookId: string }>();
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<FormFields>();

  const recipeBookQuery = useGetRecipeBookById(bookId ?? "");
  const deleteRecipeBookMutation = useDeleteRecipeBookMutation();
  const notFound = recipeBookQuery.isSuccess && !recipeBookQuery.data;
  const recipeBookData = recipeBookQuery.data;

  const onSubmit: SubmitHandler<FormFields> = async (data) => {
    if (!bookId || !recipeBookData || !deleteRecipeBookMutation.isIdle) {
      return;
    }

    if (data.bookTitle != recipeBookData.name) {
      return;
    }
    await deleteRecipeBookMutation.mutateAsync({
      bookId: bookId,
      versionTag: recipeBookData.versionTag,
    });
    navigate(makeBookListPath());
  };

  return (
    <main>
      {!bookId && <BadPathBanner />}
      {bookId && (
        <>
          {recipeBookQuery.isLoading && <FetchingRecipeBookBanner />}
          {recipeBookQuery.isError && <FetchingRecipeBookFailedBanner />}
          {notFound && <RecipeBookNotFoundBanner />}
          {recipeBookData && !recipeBookData.canDeleteBook && (
            <>
              <ActionBanner
                to={makeViewRecipeBookPath(bookId)}
                message="You may not delete this recipe book"
                linkText="View recipe book"
              />
            </>
          )}
          {recipeBookData && recipeBookData.canDeleteBook && (
            <>
              <form onSubmit={handleSubmit(onSubmit)}>
                <fieldset disabled={!deleteRecipeBookMutation.isIdle}>
                  <legend>Delete Recipe Book {recipeBookData.name}?</legend>
                  <label>
                    Type: `{recipeBookData.name}`
                    <input
                      type="text"
                      {...register("bookTitle", {
                        required: true,
                        validate: (value) => {
                          return (
                            recipeBookData.name === value ||
                            `please type ${recipeBookData.name}`
                          );
                        },
                      })}
                    />
                  </label>
                  {errors.bookTitle && <span>{errors.bookTitle.message}</span>}
                  {errors.bookTitle?.type === "required" && (
                    <span>Field is Required</span>
                  )}
                </fieldset>
                <input type="submit" value="Delete" />
              </form>
            </>
          )}
        </>
      )}
    </main>
  );
}

type FormFields = {
  bookTitle: string;
};
