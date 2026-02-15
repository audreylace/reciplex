import { FetchingRecipeBanner } from "../../components/fetching-recipe-banner/fetching-recipe-banner.component";
import { RecipeNotFoundBanner } from "../../components/recipe-not-found-banner/recipe-not-found-banner.component";
import { FetchingRecipeFailedBanner } from "../../components/fetching-recipe-failed-banner/fetching-recipe-failed-banner.component";
import { EditRecipeLoadingState } from "../../hooks/useEditRecipe.hook";
import { BadPathBanner } from "../../components/bad-path-banner/bad-path-banner.component";
import { RecipeIsReadonlyBanner } from "../../components/recipe-is-readonly-banner/RecipeIsReadonlyBanner.component";
import { OfflineBanner } from "../../components/offline-banner/offline-banner.component";
import { DangerButton } from "../../../core/components/danger-button/danger-button.component";
import { SuccessButton } from "../../../core/components/success-button/success-button.component";
import { RecipeDetailsFieldSet } from "../../components/recipe-details-field-set/recipe-details-field-set.component";
import { RecipeMetaFieldSet } from "../../components/recipe-meta-field-set/recipe-meta-field-set.component";
import { RecipeConcurrentEditBanner } from "./recipe-concurrent-edit-banner.component";
import { useEditRecipePage } from "./useEditRecipePage.hook";
import styles from "./edit-recipe-page.module.css";
import formStyles from "../../../core/form-common/form-common.module.css";
import { PrimaryButton } from "../../../core/components/primary-button/primary-button.component";
import { RecipeNameAndDescription } from "../../components/recipe-title-and-description/recipe-title-and-description.component";
import { FormButtons } from "../../../core/components/form-buttons/form-buttons.component";
import { useRef } from "preact/hooks";

/** Page for editing a recipe */
export function EditRecipePage() {
  const {
    register,
    errors,
    setValue,
    enableForm,
    onSubmit,
    cancelHandler,
    recipeDetailsValue,
    recipeData,
    reloadSaveFailure,
  } = useEditRecipePage();

  const conflicted = recipeData.tag === "conflict";
  const formRef = useRef<HTMLFormElement | null>(null);
  return (
    <main className="pageMain">
      <RecipeNameAndDescription
        name={recipeData.recipe?.name}
        shortDescription={recipeData.recipe?.shortDescription}
      >
        {recipeData.tag === EditRecipeLoadingState.loaded && (
          <FormButtons>
            <SuccessButton
              disabled={!enableForm}
              onClick={() => {
                formRef.current?.requestSubmit();
              }}
            >
              Save
            </SuccessButton>
            <DangerButton onClick={cancelHandler}>Cancel</DangerButton>
          </FormButtons>
        )}
      </RecipeNameAndDescription>
      {recipeData.tag === EditRecipeLoadingState.badRoute && <BadPathBanner />}
      {recipeData.tag === EditRecipeLoadingState.notFound && (
        <RecipeNotFoundBanner />
      )}
      {recipeData.tag === EditRecipeLoadingState.loadingFailed && (
        <FetchingRecipeFailedBanner />
      )}
      {recipeData.tag === EditRecipeLoadingState.loading && (
        <FetchingRecipeBanner />
      )}
      {recipeData.tag === EditRecipeLoadingState.readonly && (
        <RecipeIsReadonlyBanner recipeId={recipeData.recipe.id} />
      )}
      {recipeData.tag === EditRecipeLoadingState.offline && <OfflineBanner />}
      {recipeData.tag === EditRecipeLoadingState.saving && <p>Saving...</p>}
      {recipeData.tag === EditRecipeLoadingState.mutateError && (
        <>
          <p>Something went wrong while saving...</p>
          <div className={formStyles.formButtonRow}>
            <SuccessButton onClick={reloadSaveFailure}>
              Reload and try again?
            </SuccessButton>
            <PrimaryButton onClick={cancelHandler}>View Recipe</PrimaryButton>
          </div>
        </>
      )}
      {(recipeData.tag === EditRecipeLoadingState.conflict ||
        recipeData.tag === EditRecipeLoadingState.loaded) && (
        <>
          <form ref={formRef} onSubmit={onSubmit}>
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
              disabled={!enableForm}
            />
            <FormButtons>
              <SuccessButton disabled={!enableForm} type="submit">
                Save
              </SuccessButton>
              <DangerButton onClick={cancelHandler}>Cancel</DangerButton>
            </FormButtons>
          </form>
        </>
      )}
    </main>
  );
}
