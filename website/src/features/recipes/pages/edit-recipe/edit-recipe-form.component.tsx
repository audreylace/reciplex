import {
  type IRecipeBookModel,
  type IRecipeModel,
} from "../../../../services/recipe-store";

import { RecipeConcurrentEditBanner } from "./recipe-concurrent-edit-banner.component";
import { useEditRecipeForm } from "./useEditRecipeForm.hook";
import { SuccessButton } from "../../../core/success-button/success-button.component";
import { DangerButton } from "../../../core/danger-button/danger-button.component";
import styles from "./edit-recipe-form.module.css";
import formStyles from "../../../core/form-common/form-common.module.css";
import { RecipeMetaFieldSet } from "../../components/recipe-meta-field-set/recipe-meta-field-set.component";
import { RecipeDetailsFieldSet } from "../../components/recipe-details-field-set/recipe-details-field-set.component";

export function EditRecipeForm({
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
    enableForm,
    onSubmit,
    recipeName,
    cancelHandler,
    recipeDetailsValue,
  } = useEditRecipeForm(recipe, conflicted);

  return (
    <>
      <h1 className={styles.topHeader}>Editing recipe - {recipeName}</h1>
      <form onSubmit={onSubmit}>
        {conflicted && <RecipeConcurrentEditBanner />}
        <div className={styles.recipeInfoFields}>
          <RecipeMetaFieldSet
            register={register}
            disabled={!enableForm}
            legendText="Recipe Information"
            errors={errors}
          />
        </div>
        <RecipeDetailsFieldSet
          value={recipeDetailsValue}
          showMaxLengthError={errors["recipeDetails"]?.type === "maxLength"}
          onChange={(s) => setValue("recipeDetails", s)}
        />
        <div className={formStyles.formButtonRow}>
          <SuccessButton className={styles.actionButton} type="submit">
            Save
          </SuccessButton>
          <DangerButton className={styles.actionButton} onClick={cancelHandler}>
            Cancel
          </DangerButton>
        </div>
      </form>
    </>
  );
}
