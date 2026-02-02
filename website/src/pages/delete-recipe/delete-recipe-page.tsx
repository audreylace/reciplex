import { useNavigate, useParams } from "react-router";
import { useGetRecipeByIdQuery } from "../../features/recipes/hooks/useGetRecipeByIdQuery.hook";
import { FetchingRecipeBanner } from "../../features/recipes/components/fetching-recipe-banner/fetching-recipe-banner.component";
import { FetchingRecipeFailedBanner } from "../../features/recipes/components/fetching-recipe-failed-banner/fetching-recipe-failed-banner.component";
import { RecipeNotFoundBanner } from "../../features/recipes/components/recipe-not-found-banner/recipe-not-found-banner.component";
import { BadPathBanner } from "../../features/recipes/components/bad-path-banner/bad-path-banner.component";
import { ActionBanner } from "../../features/recipes/components/action-banner/action-banner.component";
import {
  makeViewRecipeBookPath,
  makeViewRecipePath,
} from "../../features/recipes/route-utils";
import { useForm, type SubmitHandler } from "react-hook-form";
import { useDeleteRecipeMutation } from "../../features/recipes/hooks/useDeleteRecipeMutation";

export function DeleteRecipePage({}: {}) {
  const navigate = useNavigate();
  const { recipeId } = useParams<{ recipeId: string }>();
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<FormFields>();

  const recipeQuery = useGetRecipeByIdQuery(recipeId ?? "");
  const deleteRecipeMutation = useDeleteRecipeMutation();
  const notFound = recipeQuery.isSuccess && !recipeQuery.data;
  const recipeData = recipeQuery.data;

  const onSubmit: SubmitHandler<FormFields> = async (data) => {
    if (
      !recipeData?.bookId ||
      !recipeId ||
      !deleteRecipeMutation.isIdle ||
      !recipeData.canDeleteRecipe
    ) {
      return;
    }

    if (data.recipeTitle != recipeData.name) {
      return;
    }
    await deleteRecipeMutation.mutateAsync({
      id: recipeId,
      versionTag: recipeData.versionTag,
    });
    navigate(makeViewRecipeBookPath(recipeData.bookId));
  };

  return (
    <main>
      {!recipeId && <BadPathBanner />}
      {recipeId && (
        <>
          {recipeQuery.isLoading && <FetchingRecipeBanner />}
          {recipeQuery.isError && <FetchingRecipeFailedBanner />}
          {notFound && <RecipeNotFoundBanner />}
          {recipeData && !recipeData.canDeleteRecipe && (
            <>
              <ActionBanner
                to={makeViewRecipePath(recipeId)}
                message="You may not edit this recipe"
                linkText="View recipe"
              />
            </>
          )}
          {recipeData && recipeData.canDeleteRecipe && (
            <>
              <form onSubmit={handleSubmit(onSubmit)}>
                <fieldset disabled={!deleteRecipeMutation.isIdle}>
                  <legend>Delete Recipe {recipeData.name}?</legend>
                  <label>
                    Type: {recipeData.name}
                    <input
                      type="text"
                      {...register("recipeTitle", {
                        required: true,
                        validate: (value) => {
                          return (
                            recipeData.name === value ||
                            `please type ${recipeData.name}`
                          );
                        },
                      })}
                    />
                  </label>
                  {errors.recipeTitle && (
                    <span>{errors.recipeTitle.message}</span>
                  )}
                  {errors.recipeTitle?.type === "required" && (
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
  recipeTitle: string;
};
