import { useForm } from "react-hook-form";
import { useParams, useNavigate } from "react-router";
import { useCreateRecipeMutation } from "../../hooks/useCreateRecipeMutation.hook";
import { useGetRecipeBookById } from "../../hooks/useGetRecipeBookById.hook";
import { makeViewRecipeBookPath, makeViewRecipePath } from "../../route-utils";

/**
 * Fields in the form
 */
type FormFields = {
  recipeName: string;
  recipeDescription: string;
};

export function useCreateRecipePage() {
  const { bookId } = useParams<{
    bookId: string;
  }>();
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<FormFields>();
  const navigate = useNavigate();
  const bookQuery = useGetRecipeBookById(bookId);

  const createRecipeMutation = useCreateRecipeMutation();
  const onSubmit = handleSubmit(async (data) => {
    if (createRecipeMutation.status !== "idle" || !bookId) {
      return;
    }
    const result = await createRecipeMutation.mutateAsync({
      name: data.recipeName,
      shortDescription: data.recipeDescription,
      bookId: bookId,
    });

    navigate(makeViewRecipePath(result.recipe.id));
  });

  let state: UseCreateRecipePageState = "loading";
  if (!bookId) {
    state = "bad-path";
  } else if (createRecipeMutation.status === "pending") {
    state = "creating";
  } else if (createRecipeMutation.status === "error") {
    state = "create-failed";
  } else if (createRecipeMutation.status === "success") {
    state = "create-complete";
  } else if (bookQuery.status === "error") {
    state = "error";
  } else if (bookQuery.status === "success") {
    if (bookQuery.data) {
      state = bookQuery.data.canAddRecipesToBook ? "loaded" : "read-only";
    } else {
      state = "not-found";
    }
  } else {
    if (bookQuery.isPaused) {
      state = "offline";
    }
  }

  return {
    register,
    onSubmit,
    bookName: bookQuery.data?.name ?? "",
    errors,
    state,
    bookId,
    reloadAction: () => {
      navigate(0);
    },
    cancelAction: () => {
      if (bookId) {
        navigate(makeViewRecipeBookPath(bookId));
      }
    },
  };
}

type UseCreateRecipePageState =
  | "loading"
  | "loaded"
  | "error"
  | "offline"
  | "read-only"
  | "not-found"
  | "creating"
  | "bad-path"
  | "create-failed"
  | "create-complete";
