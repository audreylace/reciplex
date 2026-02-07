import { useForm, type SubmitHandler } from "react-hook-form";
import { useCreateRecipeBookMutation } from "../../hooks/useCreateRecipeBookMutation";
import { RecipeBookMetaFields } from "../recipe-book-meta-fields/recipe-book-meta-fields.component";
import type { IRecipeBookModel } from "../../../../services/recipe-store";
import styles from "./create-recipe-book-form.module.css";
import { RetryBannerComponent } from "../retry-banner/retry-banner.component";

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
        <input className={styles.submitButton} type="submit" value="Create" />
        <button
          onClick={(e) => {
            e.preventDefault();
            onCancel();
          }}
          className={styles.cancelButton}
        >
          Cancel
        </button>
      </div>

      {mutation.status === "error" && (
        <div className={styles.errorBannerWrapper}>
          <RetryBannerComponent message="Creating recipe book failed." />
        </div>
      )}
    </form>
  );
}
