import { useParams } from "react-router";
import { BadPathBanner } from "../../components/bad-path-banner/bad-path-banner.component";
import { RecipeMutationLoader } from "../../components/recipe-loader/recipe-mutation-loader.component";
import { EditRecipeForm } from "../../components/edit-recipe-form/edit-recipe-form.component";
import { useNavigate } from "react-router";
import type { IRecipeModel } from "../../services/recipe-types";
import { makeViewRecipePath } from "../../route-utils";
import {
  makeRecipeNameAndDescriptionState,
  RecipeNameAndDescription,
} from "../../components/recipe-title-and-description/recipe-title-and-description.component";
import { FetchingRecipeBanner } from "../../components/recipe-banners/fetching-recipe-banner.component";

/** Page for editing a recipe */
export function EditRecipePage() {
  const { recipeId } = useParams<{
    recipeId: string;
  }>();

  return (
    <main className="pageMain">
      {!recipeId && <BadPathBanner />}
      {recipeId && (
        <RecipeMutationLoader
          recipeId={recipeId}
          fetchingRender={
            <>
              <RecipeNameAndDescription />
              <FetchingRecipeBanner />
            </>
          }
          noCache
          dataLoaderRender={(_, recipe) => {
            return <EditRecipeFormWrapper key={recipe.id} recipe={recipe} />;
          }}
        />
      )}
    </main>
  );
}

/** wraps  `EditRecipeForm` adding some page specific custom handling logic */
function EditRecipeFormWrapper({ recipe }: { recipe: IRecipeModel }) {
  const navigate = useNavigate();
  const cancelHandler = () => {
    navigate(makeViewRecipePath(recipe.bookId, recipe.id), {
      state: makeRecipeNameAndDescriptionState(
        recipe.name,
        recipe.shortDescription,
      ),
    });
  };

  const afterUpdateHandler = ({
    name,
    shortDescription,
  }: {
    name: string;
    shortDescription: string;
  }) => {
    navigate(makeViewRecipePath(recipe.bookId, recipe.id), {
      state: makeRecipeNameAndDescriptionState(name, shortDescription),
    });
  };

  return (
    <>
      <RecipeNameAndDescription
        name={recipe.name}
        shortDescription={recipe.shortDescription}
      />
      <EditRecipeForm
        recipe={recipe}
        onCancel={cancelHandler}
        onAfterUpdate={afterUpdateHandler}
      />
    </>
  );
}
