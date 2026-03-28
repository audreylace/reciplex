import { DangerButton } from "../../../core/components/buttons/danger-button.component";
import { SuccessButton } from "../../../core/components/buttons/success-button.component";
import { RecipeDetailsFieldSet } from "../recipe-details-field-set/recipe-details-field-set.component";
import {
  RecipeMetaFieldSet,
  type RecipeMetaFormModel,
} from "../recipe-meta-field-set/recipe-meta-field-set.component";
import editRecipeFormStylesModule from "./edit-recipe-form.module.css";
import { FormButtons } from "../../../core/components/form-buttons/form-buttons.component";
import { useState } from "preact/hooks";
import type { IRecipeModel } from "../../services/recipe-types";
import { useForm } from "react-hook-form";
import { useUpdateRecipeMutation } from "../../hooks/useUpdateRecipeMutation.hook";
import { makeViewRecipePath } from "../../route-utils";
import { RecipeIsReadonlyBanner } from "../recipe-banners/recipe-is-readonly-banner.component";
import {
  ErrorBanner,
  InformationBanner,
} from "../../../core/components/banner/banner.component";
import { PrimaryButton } from "../../../core/components/buttons/primary-button.component";
import { useNavigate } from "react-router";

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
  const navigate = useNavigate();
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
        name: newData.name,
        shortDescription: newData.shortDescription,
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
  const enableForm = !conflict && recipeMutation.status === "idle";
  const disableCancel = recipeMutation.status === "pending" || conflict;
  const navigateToViewRecipe = () => {
    navigate(makeViewRecipePath(recipe.bookId, recipe.id));
  };

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
        <ErrorBanner
          title="Conflict"
          message="Another user has changed this recipe. Existing changes must be discarded and the recipe data reloaded."
        >
          <FormButtons notInForm>
            <SuccessButton onClick={() => navigate(0)}>
              Reload Recipe
            </SuccessButton>
            <PrimaryButton onClick={onCancel}>End Editing</PrimaryButton>
          </FormButtons>
        </ErrorBanner>
      )}
      {recipeMutation.isError && (
        <ErrorBanner
          title="Save Failed"
          message="The save failed. Existing changes must be discarded and the recipe data reloaded."
        >
          <FormButtons notInForm>
            <SuccessButton onClick={() => navigate(0)}>
              Reload Recipe
            </SuccessButton>
            <PrimaryButton onClick={onCancel}>End Editing</PrimaryButton>
          </FormButtons>
        </ErrorBanner>
      )}
      {recipeMutation.isPending && (
        <InformationBanner
          title="Saving Changes"
          message="New changes are being published to the cloud. Do not leave or close this window."
        ></InformationBanner>
      )}
      {recipeMutation.isSuccess && (
        <InformationBanner
          title="Recipe saved"
          message="Changes published to the cloud."
        >
          <SuccessButton onClick={navigateToViewRecipe}>
            View Recipe
          </SuccessButton>
        </InformationBanner>
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
