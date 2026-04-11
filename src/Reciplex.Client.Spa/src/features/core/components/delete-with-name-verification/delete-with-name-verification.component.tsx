import Typography from "@mui/material/Typography";
import { useForm } from "react-hook-form";
import Box from "@mui/material/Box";
import Stack from "@mui/material/Stack";
import TextField from "@mui/material/TextField";
import Button from "@mui/material/Button";
import { DeleteFailedAlert } from "./delete-failed-alert.component";
import { DeleteWithNameVerificationSkeleton } from "./delete-with-name-verification-skeleton.component";
import { ConcurrencyConflictAlert } from "../concurrency-conflict-alert/concurrency-conflict-alert.component";

/** component providing the form for deleting an entity */
export function DeleteWithNameVerification({
  name,
  uniqueKey,
  showDeleteError,
  showSkeleton,
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

  if (showSkeleton) {
    return <DeleteWithNameVerificationSkeleton entityType={entityType} />;
  }

  const formDisabled = showDeleteError || pending || showConcurrencyError;
  return (
    <>
      <Typography variant="h5" gutterBottom>
        <Stack direction={"row"} gap={2}>
          <span>Deleting {entityType}</span>
        </Stack>
      </Typography>
      <Typography variant="h6">
        <Stack direction="row" gap={1}>
          <span>{name}</span>
        </Stack>
      </Typography>
      {description && (
        <Typography variant="body1">
          <Stack direction={"row"} gap={1}>
            <span>{description}</span>
          </Stack>
        </Typography>
      )}
      <Typography variant="body2" gutterBottom>
        <Stack direction="row" gap={1}>
          <span>Key: </span>
          <span>{uniqueKey}</span>
        </Stack>
      </Typography>
      {showConcurrencyError && <ConcurrencyConflictAlert onReset={onReset} />}
      {showDeleteError && <DeleteFailedAlert onReset={onReset} />}
      <form onSubmit={onSubmit}>
        <Stack spacing={2} marginTop={3}>
          <Typography variant="body1" gutterBottom>
            Type {`'${name}'`} to confirm permanent deletion
          </Typography>
          <TextField
            label={`Type '${name}'`}
            helperText={errors.name?.message}
            error={!!errors.name}
            fullWidth
            variant="filled"
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
    </>
  );
}

/** properties for the main form component */
interface IDeleteWithNameVerificationProps {
  /** the name */
  name: string;
  /** the entities unique key */
  uniqueKey: string;
  /** when true the form appears as a loading skeleton */
  showSkeleton: boolean;
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
