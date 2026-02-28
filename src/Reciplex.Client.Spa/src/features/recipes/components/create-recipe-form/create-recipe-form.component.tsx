import { useState } from "preact/hooks";
import { useForm } from "react-hook-form";
import { DangerButton } from "../../../core/components/buttons/danger-button.component";
import { FormButtons } from "../../../core/components/form-buttons/form-buttons.component";
import { SuccessButton } from "../../../core/components/buttons/success-button.component";
import { useCreateRecipeMutation } from "../../hooks/useCreateRecipeMutation.hook";
import { makeViewRecipePath } from "../../route-utils";
import type { IRecipeBookModel } from "../../services/recipe-types";
import { ActionFailedTryAgainCancel } from "../action-failed-try-again-cancel/action-failed-try-again-cancel.component";
import { BookIsReadonlyBanner } from "../book-banners/book-is-readonly-banner.component";
import { RecipeMetaFieldSet } from "../recipe-meta-field-set/recipe-meta-field-set.component";
import {
  InformationBanner,
  SuccessBanner,
} from "../../../core/components/banner/banner.component";
import { ApplicationErrorBanner } from "../../../core/components/banner/application-error-banner.component";

/** form for creating a recipe */
export function CreateRecipeForm({
  book,
  onAfterCreated,
  onCancel,
}: CreateRecipeFormProps) {
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<FormFields>();

  const { id: bookId, name: bookName } = book;
  const [recipeId, setRecipeId] = useState("");
  const { status: mutationStatus, mutateAsync: createAsync } =
    useCreateRecipeMutation();

  const onSubmit = handleSubmit(async (data) => {
    if (mutationStatus !== "idle") {
      return;
    }
    const result = await createAsync({
      name: data.recipeName,
      shortDescription: data.recipeDescription,
      bookId: bookId,
    });

    setRecipeId(result.recipe.id);
    onAfterCreated(result.recipe.id);
  });

  // short circuit to a read-only banner
  // if the user does not have edit access
  if (!book.mayEdit) {
    return <BookIsReadonlyBanner bookId={book.id} />;
  }

  switch (mutationStatus) {
    case "idle":
      return (
        <form onSubmit={onSubmit}>
          <RecipeMetaFieldSet
            disabled={false}
            register={register}
            legendText={`Add recipe to ${bookName}`}
            errors={errors}
          />
          <FormButtons>
            <SuccessButton type="submit">
              <i className="bi bi-plus-circle-dotted"></i> Create Recipe
            </SuccessButton>
            <DangerButton onClick={onCancel}>Cancel</DangerButton>
          </FormButtons>
        </form>
      );
    case "error":
      return (
        <ActionFailedTryAgainCancel
          message="Something went wrong while creating recipe..."
          cancelCaption="Return to Book"
          cancelAction={onCancel}
        />
      );
    case "pending":
      return (
        <InformationBanner
          title="Creating Recipe"
          message="Publishing new recipe to the cloud. Do not leave or close this window."
        />
      );
    case "success":
      return (
        <SuccessBanner
          to={makeViewRecipePath(bookId, recipeId)}
          title="Success"
          message="Your new recipe was created"
          buttonCaption="open recipe"
        />
      );
    default:
      return <ApplicationErrorBanner />;
  }
}

/** properties for `CreateRecipeForm` */
export interface CreateRecipeFormProps {
  /**
   * book that the recipe will be added to.
   */
  book: IRecipeBookModel;
  /**
   * Invoked when the user presses the cancel button
   */
  onCancel: () => void;
  /**
   * Invoked after the user creates the recipe passing the ID of the new recipe to the callback
   * @param recipeId the id of the new recipe
   */
  onAfterCreated: (recipeId: string) => void;
}

/**
 * Fields in the form
 */
type FormFields = {
  recipeName: string;
  recipeDescription: string;
};
