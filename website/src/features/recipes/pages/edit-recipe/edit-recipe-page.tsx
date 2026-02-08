import { useNavigate, useParams } from "react-router";
import { useEffect, useState } from "preact/hooks";
import { useForm, type SubmitHandler } from "react-hook-form";
import { RecipeDetailsMaxLength } from "../../../../services/recipe-store";
import {
  RecipeMetaFields,
  type RecipeMetaFormModel,
} from "../../components/recipe-meta-fields/recipe-meta-fields.component";
import { useGetRecipeByIdQuery } from "../../hooks/useGetRecipeByIdQuery.hook";
import { useUpdateRecipeMutation } from "../../hooks/useUpdateRecipeMutation.hook";
import { makeViewRecipePath } from "../../route-utils";
import { FetchingRecipeBanner } from "../../components/fetching-recipe-banner/fetching-recipe-banner.component";
import { RecipeNotFoundBanner } from "../../components/recipe-not-found-banner/recipe-not-found-banner.component";
import { ActionBanner } from "../../components/action-banner/action-banner.component";
import { RecipeMarkdownEditor } from "../../components/recipe-details-editor/recipe-details-editor.component";
import { FetchingRecipeFailedBanner } from "../../components/fetching-recipe-failed-banner/fetching-recipe-failed-banner.component";

export function EditRecipePage({}: {}) {
  const { recipeId } = useParams<{ recipeId: string }>();
  const navigate = useNavigate();
  const {
    register,
    handleSubmit,
    formState: { errors },
    setValue,
    watch,
    reset,
  } = useForm<FormFields>();

  const recipeQuery = useGetRecipeByIdQuery(recipeId ?? "", {
    refetchInterval: 10000, // refresh every 10 seconds to ensure user knows right away that there changes will be lost
    staleTime: 0,
  });
  const recipeMutation = useUpdateRecipeMutation();
  const [hasWriteAccess, setHasWriteAccess] = useState<boolean | null>(null);
  const [versionString, setVersionString] = useState<string | null>(null);
  const [dataLoaded, setDataLoaded] = useState<boolean>(false);
  const [notFound, setNotFound] = useState<boolean>(false);

  useEffect(() => {
    if (recipeQuery.isFetchedAfterMount && !dataLoaded) {
      const data = recipeQuery.data;
      setDataLoaded(true);
      if (data) {
        setVersionString(data.versionTag);
        setHasWriteAccess(data.canEditRecipe);
        if (data.canEditRecipe) {
          reset({
            recipeName: data.name,
            recipeDescription: data.shortDescription,
            recipeInstructions: data.details,
          });
        }
      } else {
        setNotFound(true);
      }
    }
  }, [reset, recipeQuery.data, recipeQuery.isFetchedAfterMount, dataLoaded]);

  const isConflicted = versionString !== recipeQuery.data?.versionTag;
  const enableForm =
    recipeMutation.status === "idle" &&
    recipeId &&
    dataLoaded &&
    hasWriteAccess === true &&
    !isConflicted;

  const onSubmit: SubmitHandler<FormFields> = async (data) => {
    if (enableForm) {
      const result = await recipeMutation.mutateAsync({
        recipeId,
        name: data.recipeName,
        shortDescription: data.recipeDescription,
        details: data.recipeInstructions,
        versionTag: versionString,
      });

      navigate(makeViewRecipePath(result.id));
    }
  };

  const recipeName = watch("recipeName");
  return (
    <main>
      {!dataLoaded &&
        !recipeQuery.isFetchedAfterMount &&
        !recipeQuery.isError && <FetchingRecipeBanner />}
      {!dataLoaded &&
        !recipeQuery.isFetchedAfterMount &&
        recipeQuery.isError && <FetchingRecipeFailedBanner />}
      {dataLoaded && (
        <>
          {notFound && <RecipeNotFoundBanner />}
          {hasWriteAccess === false && recipeId && (
            <>
              <ActionBanner
                to={makeViewRecipePath(recipeId)}
                message="You may not edit this recipe"
                linkText="View recipe"
              />
            </>
          )}
          {hasWriteAccess === true && (
            <>
              <h1>Editing Recipe {recipeName}</h1>
              {isConflicted && (
                <h2>
                  Another user has made changes to this recipe. Attempting to
                  save now will fail. Recommend opening this recipe in another
                  window or tab and copying over your changes.
                </h2>
              )}
              <form onSubmit={handleSubmit(onSubmit)}>
                <RecipeMetaFields
                  register={register}
                  disabled={!enableForm}
                  legendText="Recipe Information"
                  errors={errors}
                />
                <RecipeMarkdownEditor
                  label="Recipe Directions"
                  setValue={setValue}
                  watch={watch}
                  disabled={!enableForm}
                  legendText="Recipe Ingredients and Directions"
                  register={register}
                  errors={errors}
                  name="recipeInstructions"
                  maxLength={{
                    value: RecipeDetailsMaxLength,
                    message: ` Recipe Directions has a max length of ${RecipeDetailsMaxLength}`,
                  }}
                />
                <button disabled={!enableForm}>
                  {isConflicted ? "Recipe Edit Conflict" : "Save Changes"}
                </button>
              </form>
            </>
          )}
        </>
      )}
    </main>
  );
}

/**
 * Fields in the form
 */
type FormFields = {
  recipeInstructions: string;
} & RecipeMetaFormModel;
