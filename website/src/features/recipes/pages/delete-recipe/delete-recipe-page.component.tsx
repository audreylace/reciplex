import { BadPathBanner } from "../../components/bad-path-banner/bad-path-banner.component";
import formCommonStylesModule from "../../../core/form-common/form-common.module.css";
import { useParams } from "react-router";
import { RecipeMutationLoader } from "../../components/recipe-loader/recipe-mutation-loader.component";
import { DeleteRecipeEditBody } from "./delete-recipe-edit-body.component";
import { FetchingRecipeBanner } from "../../components/fetching-recipe-banner/fetching-recipe-banner.component";
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
      {!recipeId && <BadPathBanner />}
      {recipeId && (
        <RecipeMutationLoader
          recipeId={recipeId}
          noCache
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
