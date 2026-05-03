import Paper from "@mui/material/Paper";
import { useForm } from "react-hook-form";
import { type IRecipeModel } from "../../services/recipe-types";
import { ConcurrencyConflictAlert } from "../../../core/components/concurrency-conflict-alert/concurrency-conflict-alert.component";
import { OperationFailedAlert } from "../../../core/components/operation-failed-alert/operation-failed-alert.component";
import { useUpdateRecipeMutation } from "../../hooks/useUpdateRecipeMutation.hook";
import Box from "@mui/material/Box";
import Typography from "@mui/material/Typography";
import LinearProgress from "@mui/material/LinearProgress";
import Stack from "@mui/material/Stack";
import Button from "@mui/material/Button";
import { useNavigate } from "react-router";
import { makeViewRecipePath } from "../../route-utils";
import type { IFormModel } from "./form-model";
import { DetailsInput } from "./details-input.component";
import { NameInput } from "./name-input.component";
import { ShortDescriptionInput } from "./short-description-input.component";

export function EditRecipeForm({
  data,
  concurrencyConflict,
  concurrencyToken,
  onReset,
  onCancel,
}: {
  data: IRecipeModel;
  concurrencyConflict?: boolean;
  onReset: () => void;
  concurrencyToken: string;
  onCancel: () => void;
}) {
  const {
    handleSubmit,
    register,
    control,
    formState: { errors },
  } = useForm<IFormModel>({
    defaultValues: {
      name: data.name,
      shortDescription: data.shortDescription,
      details: data.details,
    },
  });
  const navigate = useNavigate();
  const recipeMutation = useUpdateRecipeMutation();
  const formDisabled = !recipeMutation.isIdle || concurrencyConflict;

  return (
    <form
      onSubmit={handleSubmit(async (newValues) => {
        if (formDisabled || !recipeMutation.isIdle) {
          return;
        }

        const newRecipe = await recipeMutation.mutateAsync({
          name: newValues.name,
          shortDescription: newValues.shortDescription,
          details: newValues.details,
          recipeId: data.id,
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
        {concurrencyConflict && (
          <ConcurrencyConflictAlert
            onReset={() => {
              onReset();
              recipeMutation.reset();
            }}
          />
        )}
        {recipeMutation.isPending && (
          <LinearProgress aria-label="Creating..." />
        )}
        {recipeMutation.isError && (
          <OperationFailedAlert onRetry={() => recipeMutation.reset()} />
        )}
      </Box>
      <Stack gap={2}>
        <Paper sx={{ p: 2 }}>
          <Typography variant="h5">Metadata</Typography>
          <Typography variant="subtitle1">
            High level information used for searching and quick overview
          </Typography>
          <Stack gap={2} sx={{ mt: 2 }}>
            <NameInput
              register={register}
              disabled={formDisabled}
              errorText={errors.name?.message}
            />
            <ShortDescriptionInput
              disabled={formDisabled}
              errorText={errors.shortDescription?.message}
              control={control}
            />
          </Stack>
        </Paper>
        <Paper sx={{ p: 2 }}>
          <Typography variant="h5">Details</Typography>
          <Typography variant="subtitle1">
            Recipe instructions and other information
          </Typography>
          <Stack gap={2} sx={{ mt: 2 }}>
            <DetailsInput control={control} disabled={formDisabled} />
          </Stack>
        </Paper>
        <Box>
          <Stack spacing={1} direction="row">
            <Button
              variant="outlined"
              type="submit"
              loading={recipeMutation.isPending}
              disabled={formDisabled && !recipeMutation.isPending}
            >
              Save
            </Button>
            <Button variant="outlined" color="error" onClick={onCancel}>
              Cancel
            </Button>
          </Stack>
        </Box>
      </Stack>
    </form>
  );
}
