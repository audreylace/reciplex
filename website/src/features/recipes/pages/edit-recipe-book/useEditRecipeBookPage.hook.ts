import { useState } from "preact/hooks";
import { useForm } from "react-hook-form";
import { useNavigate, useParams } from "react-router";
import { useGetRecipeBookById } from "../../hooks/useGetRecipeBookById.hook";
import { makeViewRecipeBookPath } from "../../route-utils";
import { useUpdateRecipeBookMutation } from "../../hooks/useUpdateRecipeBookMutation.hook";
import type { RecipeBookMetaFormModel } from "../../components/recipe-book-meta-fields/recipe-book-meta-fields.component";

type FormFields = {} & RecipeBookMetaFormModel;

export function useEditRecipeBookPage() {
  const navigate = useNavigate();
  const { bookId } = useParams<{ bookId: string }>();
  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<FormFields>();

  const [versionTag, setVersionTag] = useState<string | null>(null);

  const recipeBookQuery = useGetRecipeBookById(bookId ?? "", {
    noCache: true,
    refetchInterval: 10000,
  });

  const recipeBookMutation = useUpdateRecipeBookMutation();
  const recipeBookData = recipeBookQuery.data;

  let state: UseDeleteRecipeBookPageState = "loading";
  if (!bookId) {
    state = "bad-path";
  } else if (recipeBookMutation.status === "pending") {
    state = "save-in-progress";
  } else if (recipeBookMutation.status === "error") {
    state = "save-failed";
  } else if (recipeBookMutation.status === "success") {
    state = "save-complete";
  } else if (recipeBookQuery.status === "error") {
    state = "error";
  } else if (
    recipeBookQuery.status === "success" &&
    recipeBookQuery.isFetchedAfterMount
  ) {
    if (recipeBookQuery.data) {
      const data = recipeBookQuery.data;
      if (data.canEditBookInformation) {
        setVersionTag((tag) => {
          if (!tag) {
            state = "loaded";
            reset(
              {
                bookName: data.name,
                bookDescription: data.shortDescription,
              },
              {
                keepDirty: false,
                keepTouched: false,
              },
            );
            return data.versionTag;
          } else if (data.versionTag !== tag) {
            state = "conflict";
            return tag;
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
      !versionTag ||
      state !== "loaded" ||
      !recipeBookMutation.isIdle
    ) {
      return;
    }
    await recipeBookMutation.mutateAsync({
      recipeBookId: bookId,
      name: data.bookName,
      shortDescription: data.bookDescription,
      versionTag: versionTag,
    });

    navigate(makeViewRecipeBookPath(bookId));
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
  | "save-in-progress"
  | "save-failed"
  | "save-complete";
