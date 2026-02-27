import { useForm } from "react-hook-form";
import { useUpdateRecipeBookMutation } from "../../hooks/useUpdateRecipeBookMutation.hook";
import {
  RecipeBookMetaFields,
  type RecipeBookMetaFormModel,
} from "../recipe-book-meta-fields/recipe-book-meta-fields.component";
import { FormButtons } from "../../../core/components/form-buttons/form-buttons.component";
import { SuccessButton } from "../../../core/components/buttons/success-button.component";
import { DangerButton } from "../../../core/components/buttons/danger-button.component";
import type { IRecipeBookModel } from "../../services/recipe-types";
import { ActionFailedTryAgainCancel } from "../action-failed-try-again-cancel/action-failed-try-again-cancel.component";
import { BookIsReadonlyBanner } from "../book-banners/book-is-readonly-banner.component";
import { useState } from "preact/hooks";
import { InformationBanner } from "../../../core/components/banner/banner.component";
import { ApplicationErrorBanner } from "../../../core/components/banner/application-error-banner.component";

/**
 * Form for editing a recipe book.
 * Includes support for concurrent edit detection.
 */
export function EditRecipeBookForm({
  data,
  onCancel,
  onSaved,
}: EditRecipeBookFormProps) {
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<RecipeBookMetaFormModel>({
    defaultValues: {
      bookName: data.name,
      bookDescription: data.shortDescription,
    },
  });
  const recipeBookMutation = useUpdateRecipeBookMutation();
  const [versionTag, setVersionTag] = useState<string | null>(null);
  const onSubmit = handleSubmit(async (formData) => {
    if (!recipeBookMutation.isIdle || !versionTag) {
      return;
    }
    const newModel = await recipeBookMutation.mutateAsync({
      recipeBookId: data.id,
      name: formData.bookName,
      shortDescription: formData.bookDescription,
      versionTag: versionTag,
    });
    onSaved({
      bookId: newModel.id,
      name: newModel.name,
      shortDescription: newModel.shortDescription,
    });
  });

  const cancelProxy = () => {
    onCancel({
      bookId: data.id,
      name: data.name,
      shortDescription: data.shortDescription,
    });
  };

  if (data.versionTag && !versionTag) {
    setVersionTag(data.versionTag);
    return null;
  }

  if (data.versionTag !== versionTag) {
    return (
      <ActionFailedTryAgainCancel
        message="Someone else changed the recipe book."
        tryAgainCaption="Reload and try again?"
        cancelCaption="View Recipe Book"
        cancelAction={cancelProxy}
      />
    );
  }

  if (!data.mayEdit) {
    return <BookIsReadonlyBanner bookId={data.id} />;
  }

  switch (recipeBookMutation.status) {
    case "idle":
      return (
        <form onSubmit={onSubmit}>
          <RecipeBookMetaFields
            register={register}
            legend={`Editing Recipe Book ${data.name}`}
            errors={errors}
          />
          <FormButtons>
            <SuccessButton type="submit">Save</SuccessButton>
            <DangerButton onClick={cancelProxy}>Cancel</DangerButton>
          </FormButtons>
        </form>
      );

    case "pending":
      return (
        <InformationBanner
          title="Saving Changes"
          message="New changes are being published to the cloud. Do not leave or close this window."
        />
      );

    case "error":
      return (
        <ActionFailedTryAgainCancel
          message="Something went wrong while saving."
          cancelCaption="View Recipe"
          cancelAction={cancelProxy}
        />
      );
    default:
      return <ApplicationErrorBanner />;
  }
}

/**
 * Component properties for `EditRecipeBookForm`
 */
export interface EditRecipeBookFormProps {
  /**
   * The model to edit.
   *
   * Remount this component if new data has been
   * loaded and all the fields should be updated.
   *
   * This component monitors the `versionTag` property to
   * watch for concurrent edits. When it detects the
   * value has changed, it stops
   * all editing and shows an error banner.
   */
  data: IRecipeBookModel;
  /**
   * Invoked on cancel
   */
  onCancel: (info?: {
    name: string;
    shortDescription: string;
    bookId: string;
  }) => void;
  /**
   * Invoked once the user is done editing
   */
  onSaved: (info?: {
    name: string;
    shortDescription: string;
    bookId: string;
  }) => void;
}
