import { useForm, type SubmitHandler } from "react-hook-form";
import { useNavigate, useParams } from "react-router";
import { makeRecipeNameAndDescriptionState } from "../../components/recipe-title-and-description/recipe-title-and-description.component";
import { useEditRecipe } from "../../hooks/useEditRecipe.hook";
import { makeViewRecipeBookPath, makeViewRecipePath } from "../../route-utils";

/** */
type FormFields = {
  /** field that the user will type the whole recipe name into to confirm deletion */
  recipeName: string;
};

/** Logic for the delete recipe page */
export function useDeleteRecipePage() {
  const navigate = useNavigate();
  const { recipeId } = useParams<{ recipeId: string }>();
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<FormFields>();

  const editRecipe = useEditRecipe(recipeId);

  const onSubmit: SubmitHandler<FormFields> = async (formData) => {
    if (editRecipe.tag !== "loaded") {
      return;
    }
    if (formData.recipeName != editRecipe.recipe.name) {
      return;
    }
    await editRecipe.delete();
    navigate(makeViewRecipeBookPath(editRecipe.book.id));
  };

  return {
    register,
    onSubmit: handleSubmit(onSubmit),
    errors,
    loadState: editRecipe,
    reloadAction: () => navigate(0),
    cancelAction: () =>
      navigate(makeViewRecipePath(recipeId ?? ""), {
        state: makeRecipeNameAndDescriptionState(
          editRecipe.recipe?.name,
          editRecipe.recipe?.shortDescription,
        ),
      }),
  };
}
