import { useNavigate, useParams } from "react-router";
import { useCallback, useEffect, useState } from "preact/hooks";
import rehypeSanitize from "rehype-sanitize";
import MDEditor from "@uiw/react-md-editor";
import {
  useForm,
  type FieldErrors,
  type SubmitHandler,
  type UseFormRegister,
  type UseFormSetValue,
  type UseFormWatch,
} from "react-hook-form";
import { RecipeDetailsMaxLength } from "../../services/recipe-store";
import {
  RecipeMetaFields,
  type RecipeMetaFormModel,
} from "../../features/recipes/components/recipe-meta-fields/recipe-meta-fields.component";
import { useGetRecipeByIdQuery } from "../../features/recipes/hooks/useGetRecipeByIdQuery.hook";
import { useUpdateRecipeMutation } from "../../features/recipes/hooks/useUpdateRecipeMutation.hook";
import { makeViewRecipePath } from "../../features/recipes/route-utils";
import { FetchingRecipeBanner } from "../../features/recipes/components/fetching-recipe-banner/fetching-recipe-banner.component";
import { RecipeNotFoundBanner } from "../../features/recipes/components/recipe-not-found-banner/recipe-not-found-banner.component";
import { ActionBanner } from "../../features/recipes/components/action-banner/action-banner.component";

export function EditRecipePage({}: {}) {
  const { recipeId } = useParams<{ recipeId: string }>();
  const navigate = useNavigate();
  const {
    register,
    handleSubmit,
    formState: { errors },
    setValue,
    watch,
    reset,
  } = useForm<FormFields>();

  const recipeQuery = useGetRecipeByIdQuery(recipeId ?? "", {
    refetchInterval: 10000, // refresh every 10 seconds to ensure user knows right away that there changes will be lost
    staleTime: 0,
  });
  const recipeMutation = useUpdateRecipeMutation();

  const [higherLevelState, setHigherLevelState] = useState<{
    isConflicted: boolean;
    version: string;
    hasWriteAccess: boolean;
  } | null>(null);

  useEffect(() => {
    if (recipeQuery.isFetchedAfterMount && recipeQuery.data) {
      const data = recipeQuery.data;
      setHigherLevelState((prev) => {
        if (!prev) {
          if (data.hasWriteAccess) {
            reset({
              recipeName: data.name,
              recipeDescription: data.shortDescription,
              recipeInstructions: data.details,
            });
          }
          return {
            isConflicted: false,
            version: data.versionTag,
            hasWriteAccess: data.hasWriteAccess,
          };
        }

        if (prev.isConflicted || !prev.hasWriteAccess) {
          return prev;
        }

        if (prev.version != data.versionTag) {
          return {
            isConflicted: true,
            version: prev.version,
            hasWriteAccess: true,
          };
        }

        return prev;
      });
    }
  }, [reset, recipeQuery.data, recipeQuery.isFetchedAfterMount]);

  const onSubmit: SubmitHandler<FormFields> = async (data) => {
    if (
      recipeMutation.status !== "idle" ||
      !higherLevelState ||
      higherLevelState.isConflicted ||
      !recipeId
    ) {
      return;
    }

    const result = await recipeMutation.mutateAsync({
      recipeId,
      name: data.recipeName,
      shortDescription: data.recipeDescription,
      details: data.recipeInstructions,
      versionTag: higherLevelState.version,
    });

    navigate(makeViewRecipePath(result.id));
  };

  const recipeName = watch("recipeName");
  return (
    <main>
      {!higherLevelState && !recipeQuery.isFetchedAfterMount && (
        <FetchingRecipeBanner />
      )}

      {recipeQuery.isFetchedAfterMount && !higherLevelState && (
        <RecipeNotFoundBanner />
      )}
      {higherLevelState && !higherLevelState.hasWriteAccess && recipeId && (
        <>
          <ActionBanner
            to={makeViewRecipePath(recipeId)}
            message="You may not edit this recipe"
            linkText="View recipe"
          />
        </>
      )}
      {higherLevelState && higherLevelState.hasWriteAccess && (
        <>
          <h1>Editing Recipe {recipeName}</h1>
          {higherLevelState?.isConflicted && (
            <h2>
              Another user has made changes to this recipe. Attempting to save
              now will fail. Recommend opening this recipe in another window or
              tab and copying over your changes.
            </h2>
          )}
          <form onSubmit={handleSubmit(onSubmit)}>
            <RecipeMetaFields
              register={register}
              disabled={false}
              legendText="Top Level Recipe Information"
              errors={errors}
            />
            <RecipeDetailsEditor
              setValue={setValue}
              watch={watch}
              disabled={false}
              legendText="Recipe Ingredients and Directions"
              register={register}
              errors={errors}
            />
            <button disabled={higherLevelState?.isConflicted}>
              {higherLevelState?.isConflicted
                ? "Recipe Edit Conflict"
                : "Save Changes"}
            </button>
          </form>
        </>
      )}
    </main>
  );
}

/**
 * markdown recipe detail editor
 * @param param0 react props array
 * @returns react jsx tree for rendering
 */
function RecipeDetailsEditor({
  setValue,
  legendText,
  disabled,
  register,
  errors,
  watch,
}: {
  /**
   * text for the legend
   */
  legendText: string;
  /**
   * controls if the form is disabled
   */
  disabled: boolean;
  /**
   * form register method
   */
  register: UseFormRegister<FormFields>;
  /**
   * validation errors if any
   */
  errors: FieldErrors<FormFields>;
  /**
   * form method for setting values in response to input changes
   */
  setValue: UseFormSetValue<FormFields>;
  /**
   * form hook for triggering a rerender on value change
   */
  watch: UseFormWatch<FormFields>;
}) {
  const value = watch("recipeInstructions");
  const updateRecipeInstructions = useCallback(
    (value: string | undefined | null) =>
      setValue("recipeInstructions", value ?? "", {
        shouldDirty: true,
        shouldTouch: true,
        shouldValidate: true,
      }),
    [setValue],
  );
  return (
    <fieldset disabled={disabled}>
      <legend>{legendText}</legend>
      {errors.recipeInstructions?.type === "maxLength" && (
        <span>
          Recipe Directions has a max length of {RecipeDetailsMaxLength}
        </span>
      )}
      <label>
        Recipe Directions
        <MDEditor
          value={value}
          onChange={updateRecipeInstructions}
          autoFocus={true}
          preview="edit"
          previewOptions={{
            rehypePlugins: [[rehypeSanitize]],
          }}
          textareaProps={{
            ...register("recipeInstructions", {
              maxLength: RecipeDetailsMaxLength,
            }),
          }}
        />
      </label>
    </fieldset>
  );
}

/**
 * Fields in the form
 */
type FormFields = {
  recipeInstructions: string;
} & RecipeMetaFormModel;
