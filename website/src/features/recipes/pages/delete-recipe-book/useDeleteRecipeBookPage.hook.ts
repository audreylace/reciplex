import { useState } from "preact/hooks";
import { useForm } from "react-hook-form";
import { useNavigate, useParams } from "react-router";
import { useDeleteRecipeBookMutation } from "../../hooks/useDeleteRecipeBookMutation";
import { useGetRecipeBookById } from "../../hooks/useGetRecipeBookById.hook";
import { makeBookListPath, makeViewRecipeBookPath } from "../../route-utils";

type FormFields = {
  bookTitle: string;
};

export function useDeleteRecipeBookPage() {
  const navigate = useNavigate();
  const { bookId } = useParams<{ bookId: string }>();
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<FormFields>();

  const [versionTag, setVersionTag] = useState<string | null>(null);

  const recipeBookQuery = useGetRecipeBookById(bookId ?? "", {
    noCache: true,
    refetchInterval: 10000,
  });
  const deleteRecipeBookMutation = useDeleteRecipeBookMutation();

  const recipeBookData = recipeBookQuery.data;

  let state: UseDeleteRecipeBookPageState = "loading";
  if (!bookId) {
    state = "bad-path";
  } else if (deleteRecipeBookMutation.status === "pending") {
    state = "delete-in-progress";
  } else if (deleteRecipeBookMutation.status === "error") {
    state = "delete-failed";
  } else if (deleteRecipeBookMutation.status === "success") {
    state = "delete-complete";
  } else if (recipeBookQuery.status === "error") {
    state = "error";
  } else if (
    recipeBookQuery.status === "success" &&
    recipeBookQuery.isFetchedAfterMount
  ) {
    if (recipeBookQuery.data) {
      const data = recipeBookQuery.data;
      if (data.canDeleteBook) {
        setVersionTag((tag) => {
          if (!tag) {
            state = "loaded";
            return data.versionTag;
          } else if (data.versionTag !== tag) {
            state = "conflict";
          }
          state = "loaded";
          return tag;
        });
      } else {
        state = "read-only";
      }
    } else {
      state = "not-found";
    }
  } else {
    if (recipeBookQuery.isPaused) {
      state = "offline";
    }
  }

  const onSubmit = handleSubmit(async (data) => {
    if (
      !bookId ||
      !recipeBookData ||
      !deleteRecipeBookMutation.isIdle ||
      !versionTag ||
      state !== "loaded"
    ) {
      return;
    }

    if (data.bookTitle != recipeBookData.name) {
      return;
    }
    await deleteRecipeBookMutation.mutateAsync({
      bookId: bookId,
      versionTag: versionTag,
    });
    navigate(makeBookListPath());
  });

  return {
    onSubmit,
    register,
    errors,
    // convince TS flow control that all values in the union are set
    state: state as UseDeleteRecipeBookPageState,
    bookId,
    bookName: recipeBookData?.name,
    onCancel: () => navigate(makeViewRecipeBookPath(bookId ?? "")),
  };
}

type UseDeleteRecipeBookPageState =
  | "loading"
  | "loaded"
  | "conflict"
  | "error"
  | "offline"
  | "read-only"
  | "not-found"
  | "bad-path"
  | "conflict"
  | "delete-in-progress"
  | "delete-failed"
  | "delete-complete";
