import { useNavigate, useParams } from "react-router";
import { useGetRecipeByIdQuery } from "../../hooks/useGetRecipeByIdQuery.hook";
import { makeDeleteRecipePath, makeEditRecipePath } from "../../route-utils";
import Markdown from "react-markdown";
import rehypeSanitize from "rehype-sanitize";
import { RecipeNotFoundBanner } from "../../components/recipe-not-found-banner/recipe-not-found-banner.component";
import { FetchingRecipeBanner } from "../../components/fetching-recipe-banner/fetching-recipe-banner.component";
import { FetchingRecipeFailedBanner } from "../../components/fetching-recipe-failed-banner/fetching-recipe-failed-banner.component";
import { BadPathBanner } from "../../components/bad-path-banner/bad-path-banner.component";
import { DangerButton } from "../../../core/components/danger-button/danger-button.component";
import { PrimaryButton } from "../../../core/components/primary-button/primary-button.component";
import styles from "./view-recipe-page.module.css";
import {
  makeRecipeNameAndDescriptionState,
  RecipeNameAndDescription,
} from "../../components/recipe-title-and-description/recipe-title-and-description.component";

/**
 * page for viewing a recipe
 */
export function ViewRecipePage() {
  const { recipeId } = useParams<{
    recipeId: string;
  }>();
  const recipeQuery = useGetRecipeByIdQuery(recipeId, {
    refetchInterval: 60000, // refresh every 60 seconds
  });
  const navigate = useNavigate();

  const notFound = recipeQuery.isSuccess && !recipeQuery.data;
  const recipeData = recipeQuery.data;

  return (
    <main className="pageMain">
      <RecipeNameAndDescription
        name={recipeData?.recipe?.name}
        shortDescription={recipeData?.recipe?.shortDescription}
      >
        {recipeData?.recipe.mayEdit && (
          <div className={styles.actionButtonsBar}>
            <PrimaryButton
              onClick={() => {
                navigate(
                  makeEditRecipePath(
                    recipeData.recipe.bookId,
                    recipeData.recipe.id,
                  ),
                  {
                    state: makeRecipeNameAndDescriptionState(
                      recipeData.recipe.name,
                      recipeData.recipe.shortDescription,
                    ),
                  },
                );
              }}
            >
              Edit Recipe
            </PrimaryButton>
            <DangerButton
              onClick={() => {
                navigate(
                  makeDeleteRecipePath(
                    recipeData.recipe.bookId,
                    recipeData.recipe.id,
                  ),
                  {
                    state: makeRecipeNameAndDescriptionState(
                      recipeData.recipe.name,
                      recipeData.recipe.shortDescription,
                    ),
                  },
                );
              }}
            >
              Delete Recipe
            </DangerButton>
          </div>
        )}
      </RecipeNameAndDescription>
      {!recipeId && <BadPathBanner />}
      {recipeId && (
        <>
          {recipeQuery.isLoading && <FetchingRecipeBanner />}
          {recipeQuery.isError && <FetchingRecipeFailedBanner />}
          {notFound && <RecipeNotFoundBanner />}
          {recipeData && (
            <>
              <div className={styles.header}></div>
              <h4>Recipe Details</h4>
              <div className={styles.detailsWrapper}>
                <Markdown rehypePlugins={[rehypeSanitize]}>
                  {recipeData.recipe.details || "*edit to add details*"}
                </Markdown>
              </div>
            </>
          )}
        </>
      )}
    </main>
  );
}
