import { useForm, type SubmitHandler } from "react-hook-form";
import { useCreateRecipeBookMutation } from "../../hooks/useCreateRecipeBookMutation";
import { RecipeBookMetaFields } from "../recipe-book-meta-fields/recipe-book-meta-fields.component";
import type { IRecipeBookModel } from "../../../../services/recipe-store";

export function CreateRecipeBookForm({
  onCreated,
}: {
  onCreated: (recipe: IRecipeBookModel) => void;
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
      <input type="submit" value="Create" />
    </form>
  );
}
