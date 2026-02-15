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
import { FormButtons } from "../../../core/components/form-buttons/form-buttons.component";

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
  const goToEditAction = () => {
    if (!recipeData) {
      return;
    }
    navigate(
      makeEditRecipePath(recipeData.recipe.bookId, recipeData.recipe.id),
      {
        state: makeRecipeNameAndDescriptionState(
          recipeData.recipe.name,
          recipeData.recipe.shortDescription,
        ),
      },
    );
  };

  return (
    <main className="pageMain">
      <RecipeNameAndDescription
        name={recipeData?.recipe?.name}
        shortDescription={recipeData?.recipe?.shortDescription}
      >
        {recipeData?.recipe.mayEdit && (
          <FormButtons notInForm>
            <PrimaryButton onClick={goToEditAction} buttonType="dotted">
              Edit Recipe
            </PrimaryButton>
            <DangerButton
              buttonType="dotted"
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
          </FormButtons>
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
                {recipeData.recipe.details && (
                  <Markdown rehypePlugins={[rehypeSanitize]}>
                    recipeData.recipe.details
                  </Markdown>
                )}
                {!recipeData.recipe.details && recipeData.recipe.mayEdit && (
                  <>
                    <p className={styles.emptyDetails} onClick={goToEditAction}>
                      <i>Click to edit and add details</i>
                    </p>
                  </>
                )}
                {!recipeData.recipe.details && !recipeData.recipe.mayEdit && (
                  <>
                    <p>
                      <i>No details</i>
                    </p>
                  </>
                )}
              </div>
            </>
          )}
        </>
      )}
    </main>
  );
}
