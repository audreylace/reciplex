import { NavLink, useParams } from "react-router";
import { useGetRecipeByIdQuery } from "../../hooks/useGetRecipeByIdQuery.hook";
import { makeEditRecipePath } from "../../route-utils";
import Markdown from "react-markdown";
import rehypeSanitize from "rehype-sanitize";
import { RecipeNotFoundBanner } from "../../components/recipe-not-found-banner/recipe-not-found-banner.component";
import { FetchingRecipeBanner } from "../../components/fetching-recipe-banner/fetching-recipe-banner.component";
import { FetchingRecipeFailedBanner } from "../../components/fetching-recipe-failed-banner/fetching-recipe-failed-banner.component";
import { BadPathBanner } from "../../components/bad-path-banner/bad-path-banner.component";
import { useSetTitle } from "../../../../layouts/default/default-layout.state";

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
  useSetTitle("Viewing Recipe");

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
              <h1>{recipeData.recipe.name}</h1>
              <p>{recipeData.recipe.shortDescription}</p>
              {recipeData.recipe.canEditRecipe && (
                <NavLink to={makeEditRecipePath(recipeData.recipe.id)}>
                  Edit Recipe
                </NavLink>
              )}
              {recipeData.recipe.canDeleteRecipe && (
                <NavLink to={`/delete-recipe/${recipeId}`}>
                  Delete Recipe
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
                {recipeData.recipe.details}
              </Markdown>
            </>
          )}
        </>
      )}
    </main>
  );
}
