import { ApplicationErrorBanner } from "../../../core/components/banner/application-error-banner.component";
import formCommonStylesModule from "../../../core/form-common/form-common.module.css";
import { useParams } from "react-router";
import { RecipeMutationLoader } from "../../components/recipe-loader/recipe-mutation-loader.component";
import { DeleteRecipeEditBody } from "./delete-recipe-edit-body.component";
import { FetchingRecipeBanner } from "../../components/recipe-banners/fetching-recipe-banner.component";
import { RecipeNameAndDescription } from "../../components/recipe-title-and-description/recipe-title-and-description.component";

/**
 * Page for deleting a recipe
 */
export function DeleteRecipePage() {
  const { recipeId } = useParams<{
    recipeId: string;
  }>();

  return (
    <main className={`${formCommonStylesModule.formMain}`}>
      {!recipeId && <ApplicationErrorBanner />}
      {recipeId && (
        <RecipeMutationLoader
          recipeId={recipeId}
          noCache
          refetchInterval={10000}
          fetchingRender={
            <>
              <RecipeNameAndDescription />
              <FetchingRecipeBanner />
            </>
          }
          dataLoaderRender={(book, recipe) => (
            <DeleteRecipeEditBody book={book} recipe={recipe} />
          )}
        />
      )}
    </main>
  );
}
