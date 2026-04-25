import { NotFoundAlert } from "../../../core/components/not-found-alert/not-found-alert.component";
import {
  useGetRecipeByIdQuery,
  useGetRecipeByIdQueryKey,
} from "../../hooks/useGetRecipeByIdQuery.hook";
import { useMutationFormState } from "../../../core/hooks/useMutationFormState.hook";
import { LoadingFailedAlert } from "../../../core/components/loading-failed-alert/loading-failed-alert.component";
import { RecipeDetailsEditor } from "../../components/recipe-details-editor/recipe-details-editor.component";
import Paper from "@mui/material/Paper";

import { useController, useForm } from "react-hook-form";
import {
  RecipeDetailsMaxLength,
  RecipeNameMaxLength,
  RecipeShortDescriptionMaxLength,
} from "../../services/recipe-types";
import { ConcurrencyConflictAlert } from "../../../core/components/concurrency-conflict-alert/concurrency-conflict-alert.component";
import { OperationFailedAlert } from "../../../core/components/operation-failed-alert/operation-failed-alert.component";
import { useUpdateRecipeMutation } from "../../hooks/useUpdateRecipeMutation.hook";
import { useEffect, useState } from "preact/hooks";
import Box from "@mui/material/Box";
import Typography from "@mui/material/Typography";
import LinearProgress from "@mui/material/LinearProgress";
import Stack from "@mui/material/Stack";
import TextField from "@mui/material/TextField";
import Button from "@mui/material/Button";
import { LoadingIndicator } from "../../../core/components/loading-indicator/loading-indicator.component";
import Alert from "@mui/material/Alert";
import { useNavigate } from "react-router";
import { makeViewRecipePath } from "../../route-utils";

export function EditRecipePageBody({ recipeId }: { recipeId: string }) {
  const navigate = useNavigate();
  const recipeMutation = useUpdateRecipeMutation();
  const recipeQueryKey = useGetRecipeByIdQueryKey(recipeId);
  const recipeQuery = useGetRecipeByIdQuery(recipeId, { alwaysFresh: true });
  const [formReady, setFormReady] = useState(false);
  const {
    handleSubmit,
    register,
    control,
    formState: { errors },
    reset,
  } = useForm<IFormModel>({
    defaultValues: {
      name: "",
      shortDescription: "",
      details: "",
    },
  });
  const { resetState, concurrencyConflict, concurrencyToken } =
    useMutationFormState({
      concurrencyTokenProvider: (b) => b.versionTag,
      queryKey: recipeQueryKey,
      query: recipeQuery,
      onReset: () => {
        recipeMutation.reset();
        setFormReady(false);
      },
    });

  const { isError, isSuccess, isPending, data: recipe } = recipeQuery;

  useEffect(() => {
    if (!recipe || !isSuccess || concurrencyConflict || formReady) {
      return;
    }
    setFormReady(true);

    reset({
      name: recipe.name,
      shortDescription: recipe.shortDescription,
      details: recipe.details,
    });
  }, [concurrencyConflict, formReady, isSuccess, recipe, reset]);

  const {
    field: {
      onChange: shortDescriptionOnChange,
      onBlur: shortDescriptionOnBlur,
      value: shortDescriptionValue,
      ref: shortDescriptionRef,
    },
  } = useController({
    name: "shortDescription",
    control,
    rules: {
      maxLength: {
        value: RecipeShortDescriptionMaxLength,
        message: `Max length for short description is ${RecipeShortDescriptionMaxLength} characters`,
      },
    },
  });

  const {
    field: {
      onChange: detailsOnChange,
      onBlur: detailsOnBlur,
      value: detailsValue,
    },
  } = useController({
    name: "details",
    control,
    rules: {
      maxLength: {
        value: RecipeDetailsMaxLength,
        message: `A recipe can be at most ${RecipeDetailsMaxLength} characters`,
      },
    },
  });

  if (isError) {
    return <LoadingFailedAlert />;
  }

  if (isSuccess) {
    if (!recipe) {
      return <NotFoundAlert />;
    }
    if (!recipe.mayEdit) {
      return <Alert>May not edit this recipe</Alert>;
    }
  }

  if (isPending || !formReady) {
    return <LoadingIndicator />;
  }

  const formDisabled =
    recipeMutation.isError || recipeMutation.isPending || concurrencyConflict;

  return (
    <form
      onSubmit={handleSubmit(async (data) => {
        if (
          !concurrencyToken ||
          concurrencyConflict ||
          !recipeMutation.isIdle
        ) {
          return;
        }

        const newRecipe = await recipeMutation.mutateAsync({
          name: data.name,
          shortDescription: data.shortDescription,
          details: data.details,
          recipeId: recipeId,
          versionTag: concurrencyToken,
        });

        navigate(makeViewRecipePath(newRecipe.bookId, newRecipe.id));
      })}
    >
      <Box
        sx={{
          my: 2,
        }}
      >
        <Typography variant="h4" gutterBottom>
          Editing Recipe
        </Typography>
        {concurrencyConflict && !recipeMutation.isSuccess && (
          <ConcurrencyConflictAlert onReset={resetState} />
        )}
        {(recipeMutation.isPending || !formReady) && (
          <LinearProgress aria-label="Creating..." />
        )}
        {recipeMutation.isError && (
          <OperationFailedAlert onRetry={resetState} />
        )}
      </Box>
      <Stack gap={2}>
        <Paper sx={{ p: 2 }}>
          <Typography variant="h5">Metadata</Typography>
          <Typography variant="subtitle1">
            High level information used for searching and quick overview
          </Typography>
          <Stack gap={2} sx={{ mt: 2 }}>
            <TextField
              disabled={formDisabled}
              variant="filled"
              fullWidth
              label="Title"
              error={!!errors?.name}
              helperText={errors.name?.message ?? "Title of the recipe"}
              {...register("name", {
                required: "Recipe must have a title",
                maxLength: {
                  value: RecipeNameMaxLength,
                  message: `Max length is ${RecipeNameMaxLength} characters`,
                },
              })}
            />
            <TextField
              disabled={formDisabled}
              variant="filled"
              fullWidth
              multiline
              minRows={3}
              label="Short Description"
              error={!!errors?.shortDescription}
              helperText={
                errors.shortDescription ?? "Concise description of the recipe"
              }
              value={shortDescriptionValue}
              ref={shortDescriptionRef}
              onBlur={shortDescriptionOnBlur}
              onChange={shortDescriptionOnChange}
            />
          </Stack>
        </Paper>
        <Paper sx={{ p: 2 }}>
          <Typography variant="h5">Details</Typography>
          <Typography variant="subtitle1">
            Recipe instructions and other information
          </Typography>
          <Stack gap={2} sx={{ mt: 2 }}>
            <RecipeDetailsEditor
              value={detailsValue}
              onChange={detailsOnChange}
              onBlur={detailsOnBlur}
              minRows={8}
              maxRows={20}
              onSizeToggle={() => {}}
              disabled={formDisabled}
            />
          </Stack>
        </Paper>
        <Box>
          <Button
            variant="outlined"
            type="submit"
            loading={recipeMutation.isPending || !formReady}
            disabled={formDisabled && !recipeMutation.isPending && !formReady}
          >
            Save
          </Button>
        </Box>
      </Stack>
    </form>
  );
}

interface IFormModel {
  name: string;
  shortDescription: string;
  details: string;
}
