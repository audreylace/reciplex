import { Fieldset, Legend, Field, Label, Input } from "@headlessui/react";
import { useForm } from "react-hook-form";
import { DangerButton } from "../../../core/components/buttons/danger-button.component";
import { FormButtons } from "../../../core/components/form-buttons/form-buttons.component";
import { PrimaryButton } from "../../../core/components/buttons/primary-button.component";
import { useDeleteRecipeMutation } from "../../hooks/useDeleteRecipeMutation.hook";
import type { IRecipeModel } from "../../services/recipe-types";
import { useState } from "preact/hooks";

import formCommonStylesModule from "../../../core/form-common/form-common.module.css";
import { ActionFailedTryAgainCancel } from "../action-failed-try-again-cancel/action-failed-try-again-cancel.component";
import { RecipeIsReadonlyBanner } from "../recipe-banners/recipe-is-readonly-banner.component";
import { InformationBanner } from "../../../core/components/banner/banner.component";
import { ApplicationErrorBanner } from "../../../core/components/banner/application-error-banner.component";

/** form for deleting a recipe */
export function DeleteRecipeForm({
  onCancel,
  onDeleted,
  data,
}: DeleteRecipeFormProps) {
  //
  // TODO - this component is doing too much! Refactor into smaller pieces.

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<FormFields>();
  const deleteMutation = useDeleteRecipeMutation();
  const [versionTag, setVersionTag] = useState<string | null>(null);
  if (data.versionTag && !versionTag) {
    setVersionTag(data.versionTag);
    return null;
  }

  const onSubmit = handleSubmit(async (formData) => {
    if (!versionTag || versionTag !== data.versionTag) {
      return;
    }
    if (formData.recipeName != data.name) {
      return;
    }
    await deleteMutation.mutateAsync({
      id: data.id,
      versionTag,
    });
    onDeleted();
  });

  if (!data.mayEdit) {
    return <RecipeIsReadonlyBanner bookId={data.bookId} recipeId={data.id} />;
  }

  if (data.versionTag !== versionTag) {
    return (
      <ActionFailedTryAgainCancel
        message="Someone else changed the recipe."
        tryAgainCaption="Continue Delete?"
        cancelCaption="View Recipe"
        cancelAction={onCancel}
      />
    );
  }

  switch (deleteMutation.status) {
    case "idle":
      return (
        <form onSubmit={onSubmit}>
          <Fieldset className={formCommonStylesModule.fieldSet}>
            <Legend className={formCommonStylesModule.formLegend}>
              Confirm Recipe Deletion
            </Legend>
            <Field className={formCommonStylesModule.inputGroup}>
              <Label className={formCommonStylesModule.label}>
                {`Type "${data.name}" to delete recipe`}
              </Label>
              <Input
                className={formCommonStylesModule.fieldControl}
                placeholder={data.name}
                type="text"
                {...register("recipeName", {
                  required: true,
                  validate: (value) => {
                    return data.name === value || `type "${data.name}"`;
                  },
                })}
              />
              {errors.recipeName && <span>{errors.recipeName.message}</span>}
              {errors.recipeName?.type === "required" && (
                <span>{`type "${data.name}"`}</span>
              )}
            </Field>
          </Fieldset>
          <FormButtons>
            <DangerButton type="submit">
              <i className="bi bi-trash"></i> Delete
            </DangerButton>
            <PrimaryButton onClick={onCancel}>Cancel</PrimaryButton>
          </FormButtons>
        </form>
      );

    case "pending":
      return (
        <InformationBanner
          title="Deleting Recipe"
          message="Deleting recipe. Do not leave or close this window."
        />
      );

    case "error":
      return (
        <ActionFailedTryAgainCancel
          message="Something went wrong while deleting the recipe."
          cancelCaption="View Recipe"
          cancelAction={onCancel}
        />
      );

    default:
      return <ApplicationErrorBanner />;
  }
}

/** properties for `DeleteRecipeForm` */
export interface DeleteRecipeFormProps {
  /** the recipe to delete */
  data: IRecipeModel;
  /** invoked on cancel */
  onCancel: () => void;
  /** invoked after delete */
  onDeleted: () => void;
}

/** */
type FormFields = {
  /** field that the user will type the whole recipe name into to confirm deletion */
  recipeName: string;
};
