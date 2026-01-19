import { NavLink, useParams } from "react-router";
import { useGetRecipeByIdQuery } from "../../features/recipes/hooks/useGetRecipeByIdQuery.hook";
import { makeEditRecipePath } from "../../features/recipes/route-utils";
import Markdown from "react-markdown";
import rehypeSanitize from "rehype-sanitize";
import { RecipeNotFoundBanner } from "../../features/recipes/components/recipe-not-found-banner/recipe-not-found-banner.component";
import { FetchingRecipeBanner } from "../../features/recipes/components/fetching-recipe-banner/fetching-recipe-banner.component";
import { FetchingRecipeFailedBanner } from "../../features/recipes/components/fetching-recipe-failed-banner/fetching-recipe-failed-banner.component";
import { BadPathBanner } from "../../features/recipes/components/bad-path-banner/bad-path-banner.component";

/**
 * page for viewing a recipe
 * @param param0 react parameters
 * @returns JSX tree for react to render
 */
export function ViewRecipePage({}: {}) {
  const { recipeId } = useParams<{ recipeId: string }>();
  const recipeQuery = useGetRecipeByIdQuery(recipeId, {
    refetchInterval: 60000, // refresh every 60 seconds
  });

  const notFound = recipeQuery.isSuccess && !recipeQuery.data;
  const recipeData = recipeQuery.data;

  return (
    <main>
      {!recipeId && <BadPathBanner />}
      {recipeId && (
        <>
          {recipeQuery.isLoading && <FetchingRecipeBanner />}
          {recipeQuery.isError && <FetchingRecipeFailedBanner />}
          {notFound && <RecipeNotFoundBanner />}
          {recipeData && (
            <>
              <h1>{recipeData.name}</h1>
              <p>{recipeData.shortDescription}</p>
              {recipeData.hasWriteAccess && (
                <NavLink to={makeEditRecipePath(recipeData.id)}>
                  Edit Recipe
                </NavLink>
              )}
              <Markdown
                rehypePlugins={[rehypeSanitize]}
                components={{
                  code(props) {
                    return <code {...props} />;
                  },
                }}
              >
                {recipeData.details}
              </Markdown>
            </>
          )}
        </>
      )}
    </main>
  );
}
