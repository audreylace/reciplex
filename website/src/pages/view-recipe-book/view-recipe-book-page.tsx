import { NavLink, useParams } from "react-router";
import { makeCreateRecipePath } from "../../features/recipes/route-utils";
import { useGetRecipeBookById } from "../../features/recipes/hooks/useGetRecipeBookById.hook";
import { useGetUserById } from "../../features/users/hooks/getUserById.hook";
import { RecipeBookNotFoundBanner } from "../../features/recipes/components/recipe-book-not-found-banner/recipe-book-not-found-banner.component";
import { FetchingRecipeBookBanner } from "../../features/recipes/components/fetching-recipe-book-banner/fetching-recipe-book-banner.component";
import { FetchingRecipeBookFailedBanner } from "../../features/recipes/components/fetching-recipe-book-failed-banner/fetching-recipe-book-failed-banner.component";
import { BadPathBanner } from "../../features/recipes/components/bad-path-banner/bad-path-banner.component";

/**
 * Entry point for viewing a recipe book
 * @param param0 react props
 * @returns jsx tree for rendering by react
 */
export function ViewRecipeBookPage({}: {}) {
  const { bookId } = useParams<{
    bookId: string;
  }>();
  const bookQuery = useGetRecipeBookById(bookId);
  const userQuery = useGetUserById(bookQuery.data?.ownerId, {
    enabled: bookQuery.isSuccess,
  });

  const notFound = bookQuery.isSuccess && !bookQuery.data;
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
              <h1>{bookQuery.data.name}</h1>
              <h4>
                {userQuery.isSuccess && userQuery.data && (
                  <>Owned by: {userQuery.data?.displayName}</>
                )}
                {userQuery.isLoading && <>...</>}
              </h4>
              <p>{bookQuery.data.shortDescription}</p>
              {bookQuery.data.hasWriteAccess && (
                <>
                  <NavLink to={makeCreateRecipePath(bookId)}>
                    Add Recipe
                  </NavLink>
                  <NavLink to={`/delete-recipe-book/${bookId}`}>
                    Delete Recipe Book
                  </NavLink>
                </>
              )}
              {bookQuery.isFetching && (
                <p>Checking the cloud for updates ...</p>
              )}
            </>
          )}
        </>
      )}
    </main>
  );
}
