import { useForm, type SubmitHandler } from "react-hook-form";
import { useCreateRecipeBookMutation } from "../../../../features/recipes/hooks/useCreateRecipeBookMutation";
import { RecipeBookMetaFields } from "../../../../features/recipes/components/recipe-book-meta-fields/recipe-book-meta-fields.component";
import type { IRecipeBookModel } from "../../../../services/recipe-store";
import styles from "./create-recipe-book-form.module.css";
import { RetryBannerComponent } from "../../../../features/recipes/components/retry-banner/retry-banner.component";
import { DangerButton } from "../../../../features/core/danger-button/danger-button.component";
import { SuccessButton } from "../../../../features/core/success-button/success-button.component";

export function CreateRecipeBookForm({
  onCreated,
  onCancel,
}: {
  onCreated: (recipe: IRecipeBookModel) => void;
  onCancel: () => void;
}) {
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<FormFields>();
  const mutation = useCreateRecipeBookMutation();

  /**
   * Fields in the form
   */
  type FormFields = {
    /**
     * name of the book; max length is 127 characters
     * @see RecipeBookNameMaxLength
     */
    bookName: string;
    /**
     * description of the book; max length is 255 characters
     * @see RecipeBookShortDescriptionMaxLength
     */
    bookDescription: string;
  };

  const onSubmit: SubmitHandler<FormFields> = async (data) => {
    if (mutation.status !== "idle") {
      return;
    }

    const book = await mutation.mutateAsync({
      name: data.bookName,
      shortDescription: data.bookDescription,
    });

    onCreated(book);
  };
  return (
    <form onSubmit={handleSubmit(onSubmit)}>
      <RecipeBookMetaFields
        disabled={mutation.status !== "idle"}
        register={register}
        legend="Create New Recipe Book"
        errors={errors}
      />

      <div className={styles.buttonGroup}>
        <SuccessButton disabled={mutation.status !== "idle"} type="submit">
          <i class="bi bi-check2-circle"></i> Create
        </SuccessButton>
        <DangerButton
          disabled={mutation.status !== "idle"}
          onClick={(e) => {
            e.preventDefault();
            onCancel();
          }}
          className={styles.cancelButton}
        >
          <i class="bi bi-arrow-left-circle"></i> Cancel
        </DangerButton>
      </div>
      {mutation.status === "error" && (
        <div className={styles.errorBannerWrapper}>
          <RetryBannerComponent message="Creating recipe book failed." />
        </div>
      )}
    </form>
  );
}
