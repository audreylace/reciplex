import { FetchingRecipeBanner } from "../../components/fetching-recipe-banner/fetching-recipe-banner.component";
import { FetchingRecipeFailedBanner } from "../../components/fetching-recipe-failed-banner/fetching-recipe-failed-banner.component";
import { RecipeNotFoundBanner } from "../../components/recipe-not-found-banner/recipe-not-found-banner.component";
import { BadPathBanner } from "../../components/bad-path-banner/bad-path-banner.component";
import { EditRecipeLoadingState } from "../../hooks/useEditRecipe.hook";
import { OfflineBanner } from "../../components/offline-banner/offline-banner.component";
import { RecipeIsReadonlyBanner } from "../../components/recipe-is-readonly-banner/RecipeIsReadonlyBanner.component";
import { PrimaryButton } from "../../../core/components/primary-button/primary-button.component";
import formStyles from "../../../core/form-common/form-common.module.css";
import { DangerButton } from "../../../core/components/danger-button/danger-button.component";
import { FormButtons } from "../../../core/components/form-buttons/form-buttons.component";
import { Field, Fieldset, Input, Label, Legend } from "@headlessui/react";
import { RecipeNameAndDescription } from "../../components/recipe-title-and-description/recipe-title-and-description.component";
import { useDeleteRecipePage } from "./useDeleteRecipePage.hook";
import { ActionFailedTryAgainCancel } from "../../components/action-failed-try-again-cancel/action-failed-try-again-cancel.component";

/**
 * Page for deleting a recipe
 */
export function DeleteRecipePage() {
  const { loadState, onSubmit, cancelAction, register, errors } =
    useDeleteRecipePage();
  return (
    <main className="pageMain">
      <RecipeNameAndDescription
        name={loadState.recipe?.name}
        shortDescription={loadState.recipe?.shortDescription}
      />
      {loadState.tag === EditRecipeLoadingState.badRoute && <BadPathBanner />}
      {loadState.tag === EditRecipeLoadingState.notFound && (
        <RecipeNotFoundBanner />
      )}
      {loadState.tag === EditRecipeLoadingState.loadingFailed && (
        <FetchingRecipeFailedBanner />
      )}
      {loadState.tag === EditRecipeLoadingState.loading && (
        <FetchingRecipeBanner />
      )}
      {loadState.tag === EditRecipeLoadingState.readonly && (
        <RecipeIsReadonlyBanner
          bookId={loadState.book.id}
          recipeId={loadState.recipe.id}
        />
      )}
      {loadState.tag === EditRecipeLoadingState.offline && <OfflineBanner />}
      {loadState.tag === EditRecipeLoadingState.deleting && <p>Deleting...</p>}
      {loadState.tag === EditRecipeLoadingState.mutateError && (
        <ActionFailedTryAgainCancel
          message="Something went wrong while deleting."
          cancelCaption="View Recipe"
          cancelAction={cancelAction}
        />
      )}
      {loadState.tag === EditRecipeLoadingState.conflict && (
        <ActionFailedTryAgainCancel
          message="Someone else changed the recipe."
          tryAgainCaption="Continue Delete?"
          cancelCaption="View Recipe"
          cancelAction={cancelAction}
        />
      )}
      {loadState.tag === EditRecipeLoadingState.loaded && (
        <form onSubmit={onSubmit}>
          <Fieldset className={formStyles.fieldSet}>
            <Legend className={formStyles.formLegend}>
              Confirm Recipe Deletion
            </Legend>
            <Field className={formStyles.inputGroup}>
              <Label className={formStyles.label}>
                {`Type "${loadState.recipe.name}" to delete recipe`}
              </Label>
              <Input
                className={formStyles.fieldControl}
                type="text"
                {...register("recipeName", {
                  required: true,
                  validate: (value) => {
                    return (
                      loadState.recipe.name === value ||
                      `type "${loadState.recipe.name}"`
                    );
                  },
                })}
              />
              {errors.recipeName && <span>{errors.recipeName.message}</span>}
              {errors.recipeName?.type === "required" && (
                <span>{`type "${loadState.recipe.name}"`}</span>
              )}
            </Field>
          </Fieldset>
          <FormButtons>
            <DangerButton type="submit">Delete</DangerButton>
            <PrimaryButton onClick={cancelAction}>Cancel</PrimaryButton>
          </FormButtons>
        </form>
      )}
    </main>
  );
}
