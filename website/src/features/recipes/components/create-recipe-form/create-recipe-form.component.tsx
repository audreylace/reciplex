import { useForm, type SubmitHandler } from "react-hook-form";
import type { IRecipeModel } from "../../../../services/recipe-store";
import { useCreateRecipeMutation } from "../../hooks/useCreateRecipeMutation.hook";
import { RecipeMetaFieldSet } from "../recipe-meta-field-set/recipe-meta-field-set.component";
import { FormButtons } from "../../../core/components/form-buttons/form-buttons.component";
import { SuccessButton } from "../../../core/components/success-button/success-button.component";
import { DangerButton } from "../../../core/components/danger-button/danger-button.component";

/**
 * Fields in the form
 */
type FormFields = {
  recipeName: string;
  recipeDescription: string;
};

export function CreateRecipeForm({
  bookName,
  bookId,
  onCreated,
}: {
  bookName: string;
  bookId: string;
  onCreated: (recipe: IRecipeModel) => void;
}) {
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<FormFields>();
  const createRecipeMutation = useCreateRecipeMutation();
  /**
   * Runs action on form submit creating a new recipe
   * @param data form data
   * @returns void promise
   */
  const onSubmit: SubmitHandler<FormFields> = async (data) => {
    if (createRecipeMutation.status !== "idle") {
      return;
    }
    const recipe = await createRecipeMutation.mutateAsync({
      name: data.recipeName,
      shortDescription: data.recipeDescription,
      bookId: bookId,
    });

    onCreated(recipe.recipe);
  };
  return (
    <form onSubmit={handleSubmit(onSubmit)}>
      <RecipeMetaFieldSet
        register={register}
        disabled={createRecipeMutation.status !== "idle"}
        legendText={`Add recipe to ${bookName}`}
        errors={errors}
      />
      <FormButtons>
        <SuccessButton type="submit">Create Recipe</SuccessButton>
        <DangerButton onClick={() => }>Cancel</DangerButton>
      </FormButtons>
    </form>
  );
}
