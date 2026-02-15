import { useForm } from "react-hook-form";
import { useNavigate, useLocation } from "react-router";
import { useCreateRecipeBookMutation } from "../../hooks/useCreateRecipeBookMutation";
import { makeBookListPath, makeViewRecipeBookPath } from "../../route-utils";

/**
 * hook for managing the state of the create recipe page
 * @returns component state
 */
export function useCreateRecipeBookPage() {
  const navigate = useNavigate();
  const location = useLocation();
  const { goBack } = location.state || {};

  const onCancel = async () => {
    if (goBack) {
      navigate(-1);
    }
    navigate(makeBookListPath());
  };

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<FormFields>();

  const mutation = useCreateRecipeBookMutation();
  const onSubmit = handleSubmit(async (data) => {
    if (mutation.status !== "idle") {
      return;
    }

    const book = await mutation.mutateAsync({
      name: data.bookName,
      shortDescription: data.bookDescription,
    });

    navigate(makeViewRecipeBookPath(book.id));
  });

  return { state: mutation.status, register, onSubmit, errors, onCancel };
}

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
