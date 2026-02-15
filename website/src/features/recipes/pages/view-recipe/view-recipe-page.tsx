import { NavLink, useNavigate, useParams } from "react-router";
import { useGetRecipeByIdQuery } from "../../hooks/useGetRecipeByIdQuery.hook";
import { makeEditRecipePath } from "../../route-utils";
import Markdown from "react-markdown";
import rehypeSanitize from "rehype-sanitize";
import { RecipeNotFoundBanner } from "../../components/recipe-not-found-banner/recipe-not-found-banner.component";
import { FetchingRecipeBanner } from "../../components/fetching-recipe-banner/fetching-recipe-banner.component";
import { FetchingRecipeFailedBanner } from "../../components/fetching-recipe-failed-banner/fetching-recipe-failed-banner.component";
import { BadPathBanner } from "../../components/bad-path-banner/bad-path-banner.component";
import { DangerButton } from "../../../core/danger-button/danger-button.component";
import { PrimaryButton } from "../../../core/primary-button/primary-button.component";
import styles from "./view-recipe-page.module.css";

/**
 * page for viewing a recipe
 */
export function ViewRecipePage({}: {}) {
  const { recipeId } = useParams<{ recipeId: string }>();
  const recipeQuery = useGetRecipeByIdQuery(recipeId, {
    refetchInterval: 60000, // refresh every 60 seconds
  });
  const navigate = useNavigate();

  const notFound = recipeQuery.isSuccess && !recipeQuery.data;
  const recipeData = recipeQuery.data;

  return (
    <main className="pageMain">
      {!recipeId && <BadPathBanner />}
      {recipeId && (
        <>
          {recipeQuery.isLoading && <FetchingRecipeBanner />}
          {recipeQuery.isError && <FetchingRecipeFailedBanner />}
          {notFound && <RecipeNotFoundBanner />}
          {recipeData && (
            <>
              <div className={styles.header}>
                <h1>{recipeData.recipe.name}</h1>
                <p>{recipeData.recipe.shortDescription}</p>
                <div className={styles.actionButtonsBar}>
                  {recipeData.recipe.canEditRecipe && (
                    <PrimaryButton
                      onClick={() => {
                        navigate(makeEditRecipePath(recipeData.recipe.id));
                      }}
                    >
                      Edit Recipe
                    </PrimaryButton>
                  )}
                  {recipeData.recipe.canDeleteRecipe && (
                    <DangerButton
                      onClick={() => {
                        navigate(`/delete-recipe/${recipeId}`);
                      }}
                    >
                      Delete Recipe
                    </DangerButton>
                  )}
                </div>
              </div>
              <h4>Recipe Details</h4>
              <div className={styles.detailsWrapper}>
                <Markdown rehypePlugins={[rehypeSanitize]}>
                  {recipeData.recipe.details}
                </Markdown>
              </div>
            </>
          )}
        </>
      )}
    </main>
  );
}
