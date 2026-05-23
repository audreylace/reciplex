import Typography from "@mui/material/Typography";
import { useForm } from "react-hook-form";
import Box from "@mui/material/Box";
import Stack from "@mui/material/Stack";
import TextField from "@mui/material/TextField";
import Button from "@mui/material/Button";
import { DeleteFailedAlert } from "./delete-failed-alert.component";
import { ConcurrencyConflictAlert } from "../concurrency-conflict-alert/concurrency-conflict-alert.component";
import Paper from "@mui/material/Paper";
import { StackedLabelValue } from "../stacked-label-value/stacked-label-value.component";

/** component providing the form for deleting an entity */
export function DeleteWithNameVerification({
  name,
  uniqueKey,
  showDeleteError,
  showConcurrencyError,
  pending,
  onDelete,
  onReset,
  entityType,
  description,
}: IDeleteWithNameVerificationProps) {
  const {
    register,
    formState: { errors },
    handleSubmit,
  } = useForm<IFormModel>();

  const onSubmit = handleSubmit(async () => {
    onDelete();
  });

  const formDisabled = showDeleteError || pending || showConcurrencyError;
  return (
    <>
      <Stack sx={{ gap: 3 }}>
        {showConcurrencyError && <ConcurrencyConflictAlert onReset={onReset} />}
        {showDeleteError && <DeleteFailedAlert onReset={onReset} />}
        <Paper sx={{ p: 2 }}>
          <Stack sx={{ gap: 1 }}>
            <Typography variant="h5" gutterBottom>
              What will be deleted
            </Typography>
            <StackedLabelValue label="Name" value={name} />
            {description && (
              <StackedLabelValue label="Description" value={description} />
            )}
            <StackedLabelValue label="Record Type" value={entityType} />
            <StackedLabelValue label="Key" value={uniqueKey} />
          </Stack>
        </Paper>

        <Paper sx={{ p: 2 }}>
          <form onSubmit={onSubmit}>
            <Stack spacing={2}>
              <legend>
                <Typography variant="h5" gutterBottom>
                  Type {`'${name}'`} to confirm permanent deletion
                </Typography>
              </legend>
              <TextField
                label={`Type '${name}'`}
                helperText={errors.name?.message}
                error={!!errors.name}
                fullWidth
                variant="outlined"
                disabled={formDisabled}
                {...register("name", {
                  validate: (value) => {
                    return name === value || `type "${name}"`;
                  },
                })}
              />
              <Box>
                <Button
                  variant="contained"
                  type="submit"
                  color="error"
                  disabled={formDisabled}
                  loading={pending}
                >
                  Delete
                </Button>
              </Box>
            </Stack>
          </form>
        </Paper>
      </Stack>
    </>
  );
}

/** properties for the main form component */
interface IDeleteWithNameVerificationProps {
  /** the name */
  name: string;
  /** the entities unique key */
  uniqueKey: string;
  /** invoked at deletion */
  onDelete: () => void;
  /** invoked by the form as part of error recovery. Indicates likely that the user data should be reloaded. */
  onReset: () => void;
  /** controls if the pending deletion UI is active */
  pending: boolean;
  /** shows the delete failed banner */
  showDeleteError: boolean;
  /** shows the concurrency conflict banner */
  showConcurrencyError: boolean;
  /** the entity type */
  entityType: string;
  /** additional description */
  description?: string;
}

/** model use by the form control inside of `DeleteWithNameVerification` */
interface IFormModel {
  /** the name */
  name: string;
}
