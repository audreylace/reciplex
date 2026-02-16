import { useForm, type SubmitHandler } from "react-hook-form";
import type { RecipeMetaFormModel } from "../../components/recipe-meta-field-set/recipe-meta-field-set.component";
import { makeViewRecipePath } from "../../route-utils";
import { useNavigate, useParams } from "react-router";
import { useState } from "preact/hooks";
import { useEditRecipe } from "../../hooks/useEditRecipe.hook";
import { makeRecipeNameAndDescriptionState } from "../../components/recipe-title-and-description/recipe-title-and-description.component";

/**
 * logic for the edit recipe page
 * @returns state object for the component
 * @todo Add support for preventing accidental navigation away with incomplete changes
 */
export function useEditRecipePage() {
  const { recipeId } = useParams<{
    recipeId: string;
  }>();
  const recipeData = useEditRecipe(recipeId);
  const {
    register,
    handleSubmit,
    formState: { errors },
    setValue,
    watch,
    reset,
  } = useForm<FormFields>({
    defaultValues: {
      recipeName: recipeData.recipe?.name,
      recipeDescription: recipeData.recipe?.shortDescription,
      recipeDetails: recipeData.recipe?.details,
    },
  });

  // load form data if it is not loaded already
  const [formSet, setFormSet] = useState(recipeData.tag === "loaded");
  if (!formSet && recipeData.tag === "loaded") {
    setFormSet(true);
    reset(
      {
        recipeName: recipeData.recipe.name,
        recipeDescription: recipeData.recipe.shortDescription,
        recipeDetails: recipeData.recipe.details,
      },
      {
        keepDirty: false,
        keepTouched: false,
      },
    );
  }

  // Used to back navigate to recipe view page on completion or cancel
  const navigate = useNavigate();

  // form is only enabled when data is loaded and nothing is happening
  const enableForm = recipeData.tag === "loaded";

  // handles the submit from the form hook
  const submitHandler: SubmitHandler<FormFields> = async (data) => {
    if (enableForm && recipeData.tag === "loaded") {
      const newData = await recipeData.update({
        recipeName: data.recipeName,
        recipeDescription: data.recipeDescription,
        recipeDetails: data.recipeDetails,
      });
      navigate(makeViewRecipePath(recipeData.book.id, recipeData.recipe.id), {
        state: makeRecipeNameAndDescriptionState(
          newData.recipe.name,
          newData.recipe.shortDescription,
        ),
      });
    }
  };

  // recipe MD editor is controlled so we need to watch its value
  const recipeDetailsValue = watch("recipeDetails");

  return {
    register,
    setValue,
    errors,
    onSubmit: handleSubmit(submitHandler),
    enableForm,
    saveFailed: recipeData.tag === "mutate-error",
    saveInProgress: recipeData.tag === "saving",
    cancelHandler: () => {
      navigate(makeViewRecipePath(recipeData.book?.id ?? "", recipeId ?? ""), {
        state: makeRecipeNameAndDescriptionState(
          recipeData.recipe?.name,
          recipeData.recipe?.shortDescription,
        ),
      });
    },
    recipeDetailsValue,
    recipeData,
  };
}

/**
 * Fields in the form
 */
export type FormFields = {
  /** recipe instruction section */
  recipeDetails: string;
} & RecipeMetaFormModel;
