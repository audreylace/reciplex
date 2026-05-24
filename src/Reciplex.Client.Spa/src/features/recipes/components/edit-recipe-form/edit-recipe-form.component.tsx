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
import { useBlocker, useLocation, useNavigate } from "react-router";
import { makeViewRecipePath } from "../../route-utils";
import type { IFormModel } from "./form-model";
import { DetailsInput } from "./details-input.component";
import { NameInput } from "./name-input.component";
import { ShortDescriptionInput } from "./short-description-input.component";
import Dialog from "@mui/material/Dialog";
import { useEffect } from "react";
import Card from "@mui/material/Card";
import CardContent from "@mui/material/CardContent";
import CardActions from "@mui/material/CardActions";
import { SyntaxReference } from "../syntax-reference/syntax-reference-component.component";

export function EditRecipeForm({
  data,
  concurrencyConflict,
  concurrencyToken,
  onReset,
}: {
  data: IRecipeModel;
  concurrencyConflict?: boolean;
  onReset: () => void;
  concurrencyToken: string;
}) {
  const {
    handleSubmit,
    register,
    control,
    formState: { errors, isDirty },
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
  const blocker = useBlocker(isDirty && !recipeMutation.isSuccess);
  const showConflictBanner = concurrencyConflict && recipeMutation.isIdle;
  const location = useLocation();

  useEffect(() => {
    if (recipeMutation.isSuccess) {
      if (blocker.state === "blocked") {
        blocker.proceed();
      }
    }
    if (
      blocker.state === "blocked" &&
      location.pathname === blocker.location.pathname
    ) {
      blocker.proceed();
    }
  }, [blocker, location.pathname, recipeMutation.isSuccess]);

  return (
    <>
      <Dialog open={blocker.state === "blocked" && !recipeMutation.isSuccess}>
        <Card variant="outlined">
          <CardContent>
            <Typography variant="h5" gutterBottom>
              You have unsaved changed
            </Typography>
            <Typography variant="subtitle1">
              Continue and discard all in progress modifications?
            </Typography>
          </CardContent>
          <CardActions>
            <Stack sx={{ direction: "row", gap: 1 }}>
              <Button
                onClick={() => blocker.state === "blocked" && blocker.reset()}
              >
                Go Back
              </Button>
              <Button
                color="error"
                onClick={() => blocker.state === "blocked" && blocker.proceed()}
              >
                Continue
              </Button>
            </Stack>
          </CardActions>
        </Card>
      </Dialog>
      <Box
        sx={{
          mb: 3,
        }}
      >
        {showConflictBanner && (
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
          <OperationFailedAlert
            onRetry={() => {
              recipeMutation.reset();
              onReset();
            }}
          />
        )}
      </Box>
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
        <Stack sx={{ gap: 2 }}>
          <Paper sx={{ p: 2 }}>
            <Typography variant="h5">Metadata</Typography>
            <Typography variant="subtitle1">
              High level information used for searching and quick overview
            </Typography>
            <Stack sx={{ mt: 2, gap: 2 }}>
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
          <SyntaxReference />
          <Paper sx={{ p: 2 }}>
            <Typography variant="h5">Details</Typography>
            <Typography variant="subtitle1">
              Recipe instructions and other information
            </Typography>
            <Stack sx={{ mt: 2, gap: 2 }}>
              <DetailsInput control={control} disabled={formDisabled} />
            </Stack>
          </Paper>
          <Box>
            <Stack spacing={1} direction="row">
              <Button
                variant="contained"
                type="submit"
                loading={recipeMutation.isPending}
                disabled={formDisabled && !recipeMutation.isPending}
              >
                Save
              </Button>
              <Button
                variant="outlined"
                color="error"
                onClick={() =>
                  navigate(makeViewRecipePath(data.bookId, data.id))
                }
              >
                Discard
              </Button>
            </Stack>
          </Box>
        </Stack>
      </form>
    </>
  );
}
