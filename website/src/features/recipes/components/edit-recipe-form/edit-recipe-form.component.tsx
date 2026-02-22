import { DangerButton } from "../../../core/components/danger-button/danger-button.component";
import { SuccessButton } from "../../../core/components/success-button/success-button.component";
import { RecipeDetailsFieldSet } from "../recipe-details-field-set/recipe-details-field-set.component";
import {
  RecipeMetaFieldSet,
  type RecipeMetaFormModel,
} from "../recipe-meta-field-set/recipe-meta-field-set.component";
import editRecipeFormStylesModule from "./edit-recipe-form.module.css";
import { FormButtons } from "../../../core/components/form-buttons/form-buttons.component";
import { useState } from "preact/hooks";
import { ActionFailedTryAgainCancel } from "../action-failed-try-again-cancel/action-failed-try-again-cancel.component";
import type { IRecipeModel } from "../../services/recipe-types";
import { useForm } from "react-hook-form";
import { useUpdateRecipeMutation } from "../../hooks/useUpdateRecipeMutation.hook";
import { ActionBanner } from "../action-banner/action-banner.component";
import { makeViewRecipePath } from "../../route-utils";
import { RecipeIsReadonlyBanner } from "../recipe-is-readonly-banner/RecipeIsReadonlyBanner.component";
import { RetryBannerComponent } from "../retry-banner/retry-banner.component";

/**
 * Fields in the form
 */
export type FormFields = {
  /** recipe instruction section */
  recipeDetails: string;
} & RecipeMetaFormModel;

export function EditRecipeForm({
  recipe,
  onAfterUpdate,
  onCancel,
}: {
  recipe: IRecipeModel;
  onAfterUpdate: (args: { name: string; shortDescription: string }) => void;
  onCancel: () => void;
}) {
  const [versionTag, setVersionTag] = useState<string | null>(null);
  const recipeMutation = useUpdateRecipeMutation();
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

  // handles the submit from the form hook
  const onSubmit = handleSubmit(async (formData) => {
    if (recipeMutation.status === "idle" && versionTag) {
      const newData = await recipeMutation.mutateAsync({
        recipeId: recipe.id,
        name: formData.recipeName,
        shortDescription: formData.recipeDescription,
        details: formData.recipeDetails,
        versionTag: versionTag,
      });
      onAfterUpdate({
        name: newData.recipe.name,
        shortDescription: newData.recipe.shortDescription,
      });
    }
  });

  // recipe MD editor is controlled so we need to watch its value
  // eslint-disable-next-line react-hooks/incompatible-library
  const recipeDetailsValue = watch("recipeDetails");

  if (recipe.versionTag && !versionTag) {
    setVersionTag(recipe.versionTag);
    return null;
  }

  // short circuit to read-only banner if the user does not have edit access
  if (!recipe.mayEdit) {
    return (
      <RecipeIsReadonlyBanner bookId={recipe.bookId} recipeId={recipe.id} />
    );
  }

  const conflict = versionTag !== recipe.versionTag;

  // unlike most other form components in this app,
  // we take a different approach here disabling the form
  // so that if something does go wrong users can
  // at least manually copy and paste the data
  // to a new form instance or save off locally.
  //
  const enableForm = conflict || recipeMutation.status === "idle";
  const disableCancel = recipeMutation.status === "pending";

  return (
    <form onSubmit={onSubmit}>
      <FormButtons>
        <SuccessButton disabled={!enableForm} type="submit">
          Save
        </SuccessButton>
        <DangerButton disabled={disableCancel} onClick={onCancel}>
          Cancel
        </DangerButton>
      </FormButtons>
      {conflict && (
        <RetryBannerComponent
          buttonCaption="Discard and Reload?"
          message="Another user has made changes to this recipe. Existing changes must be discarded and the recipe reloaded."
        />
      )}
      {recipeMutation.isError && (
        <ActionFailedTryAgainCancel
          message="Something went wrong while saving recipe..."
          cancelCaption="View Recipe"
          cancelAction={onCancel}
        />
      )}
      {recipeMutation.isPending && <p>Saving...</p>}
      {recipeMutation.isSuccess && (
        <ActionBanner
          to={makeViewRecipePath(recipe.bookId, recipe.id)}
          message="Recipe update"
          linkText="view recipe"
        ></ActionBanner>
      )}
      <div className={editRecipeFormStylesModule.recipeInfoFields}>
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
        <DangerButton disabled={disableCancel} onClick={onCancel}>
          Cancel
        </DangerButton>
      </FormButtons>
    </form>
  );
}
