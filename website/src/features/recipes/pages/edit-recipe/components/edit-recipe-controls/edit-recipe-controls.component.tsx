import {
  RecipeDetailsMaxLength,
  type IRecipeBookModel,
  type IRecipeModel,
} from "../../../../../../services/recipe-store";
import { RecipeMarkdownEditor } from "../../../../components/recipe-details-editor/recipe-details-editor.component";
import { RecipeMetaFields } from "../../../../components/recipe-meta-fields/recipe-meta-fields.component";
import { RecipeConcurrentEditBanner } from "../recipe-concurrent-edit-banner.component";
import { useEditRecipeForm } from "../../hooks/useEditRecipeForm.hook";
import { SuccessButton } from "../../../../../core/success-button/success-button.component";
import { DangerButton } from "../../../../../core/danger-button/danger-button.component";
import styles from "./edit-recipe-controls.module.css";

export function EditRecipeControls({
  conflicted,
  recipe,
}: {
  book: IRecipeBookModel;
  recipe: IRecipeModel;
  conflicted: boolean;
}) {
  const {
    register,
    errors,
    setValue,
    watch,
    enableForm,
    onSubmit,
    recipeName,
    cancelHandler,
  } = useEditRecipeForm(recipe, conflicted);

  return (
    <>
      <h1 className={styles.topHeader}>Editing recipe "{recipeName}"</h1>
      <form onSubmit={onSubmit}>
        {conflicted && <RecipeConcurrentEditBanner />}
        <div className={styles.recipeInfoFields}>
          <RecipeMetaFields
            register={register}
            disabled={!enableForm}
            legendText="Recipe Information"
            errors={errors}
          />
        </div>
        <RecipeMarkdownEditor
          label="Recipe Directions"
          setValue={setValue}
          watch={watch}
          disabled={!enableForm}
          legendText="Ingredients and Directions"
          register={register}
          errors={errors}
          name="recipeInstructions"
          maxLength={{
            value: RecipeDetailsMaxLength,
            message: `Recipe Directions has a max length of ${RecipeDetailsMaxLength}`,
          }}
        />
        <SuccessButton className={styles.actionButton} type="submit">
          Save
        </SuccessButton>
        <DangerButton className={styles.actionButton} onClick={cancelHandler}>
          Cancel
        </DangerButton>
      </form>
    </>
  );
}
