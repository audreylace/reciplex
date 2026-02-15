import { useForm, type SubmitHandler } from "react-hook-form";
import type { IRecipeModel } from "../../../../services/recipe-store";
import type { RecipeMetaFormModel } from "../../components/recipe-meta-field-set/recipe-meta-field-set.component";
import { useUpdateRecipeMutation } from "../../hooks/useUpdateRecipeMutation.hook";
import { makeViewRecipePath } from "../../route-utils";
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
      recipeDetails: recipe.details,
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
        details: data.recipeDetails,
        versionTag: recipe.versionTag,
      });

      navigate(makeViewRecipePath(result.recipe.id));
    }
  };

  const recipeDetailsValue = watch("recipeDetails");

  return {
    register,
    setValue,
    errors,
    onSubmit: handleSubmit(submitHandler),
    recipeName,
    enableForm,
    saveFailed: recipeMutation.status === "error",
    saveInProgress: recipeMutation.status === "pending",
    cancelHandler: () => navigate(makeViewRecipePath(recipe.id)),
    recipeDetailsValue,
  };
}

/**
 * Fields in the form
 */
export type FormFields = {
  /** recipe instruction section */
  recipeDetails: string;
} & RecipeMetaFormModel;
