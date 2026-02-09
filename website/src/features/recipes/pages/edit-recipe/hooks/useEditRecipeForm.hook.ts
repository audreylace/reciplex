import { useForm, type SubmitHandler } from "react-hook-form";
import type { IRecipeModel } from "../../../../../services/recipe-store";
import type { RecipeMetaFormModel } from "../../../components/recipe-meta-fields/recipe-meta-fields.component";
import { useUpdateRecipeMutation } from "../../../hooks/useUpdateRecipeMutation.hook";
import { makeViewRecipePath } from "../../../route-utils";
import { useNavigate } from "react-router";

/**
 * logic for the `EditRecipeControls` component
 * @param recipe the recipe model
 * @param conflicted if the recipe is in a conflicted state
 * @returns state object for the component
 */
export function useEditRecipeForm(recipe: IRecipeModel, conflicted: boolean) {
  const {
    register,
    handleSubmit,
    formState: { errors },
    setValue,
    watch,
  } = useForm<FormFields>({
    defaultValues: {
      recipeName: recipe.name,
      recipeDescription: recipe.shortDescription,
      recipeInstructions: recipe.details,
    },
  });

  const navigate = useNavigate();
  const recipeMutation = useUpdateRecipeMutation();
  const recipeName = watch("recipeName");
  const enableForm = recipeMutation.status === "idle";
  const submitHandler: SubmitHandler<FormFields> = async (data) => {
    if (enableForm && !conflicted) {
      const result = await recipeMutation.mutateAsync({
        recipeId: recipe.id,
        name: data.recipeName,
        shortDescription: data.recipeDescription,
        details: data.recipeInstructions,
        versionTag: recipe.versionTag,
      });

      navigate(makeViewRecipePath(result.id));
    }
  };

  return {
    register,
    setValue,
    watch,
    errors,
    onSubmit: handleSubmit(submitHandler),
    recipeName,
    enableForm,
    saveFailed: recipeMutation.status === "error",
    saveInProgress: recipeMutation.status === "pending",
    cancelHandler: () => navigate(makeViewRecipePath(recipe.id)),
  };
}

/**
 * Fields in the form
 */
export type FormFields = {
  /** recipe instruction section */
  recipeInstructions: string;
} & RecipeMetaFormModel;
