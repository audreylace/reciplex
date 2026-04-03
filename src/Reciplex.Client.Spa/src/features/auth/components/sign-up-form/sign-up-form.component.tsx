import { useForm, type UseFormReturn } from "react-hook-form";
import { useNavigate } from "react-router";
import { makeCreateRecipeBookPath } from "../../../recipes/route-utils";
import { useActiveUser } from "../../hooks/useActiveUser.hook";
import { useCreateAccountMutation } from "../../hooks/useCreateAccountMutation.hook";
import TextField from "@mui/material/TextField";
import Button from "@mui/material/Button";
import Stack from "@mui/material/Stack";
import Typography from "@mui/material/Typography";
import Alert from "@mui/material/Alert";

export function SignUpForm() {
  const setActiveUser = useActiveUser((s) => s.setActiveUser);
  const navigate = useNavigate();
  const createAccountMutation = useCreateAccountMutation();
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<IFormValues>();

  const onSubmit = handleSubmit(async (data) => {
    const newAccount = await createAccountMutation.mutateAsync(data);
    setActiveUser({
      userKey: newAccount.userKey,
      displayName: newAccount.displayName,
    });
    navigate(makeCreateRecipeBookPath());
  });

  const formDisabled = createAccountMutation.status !== "idle";

  return (
    <>
      <Typography variant="h4">Welcome</Typography>
      <Typography variant="subtitle1" gutterBottom>
        Create an account and get cooking with Reciplex!
      </Typography>
      <form onSubmit={onSubmit}>
        <Stack spacing={2} marginTop={3}>
          {createAccountMutation.status === "success" && (
            <Alert
              severity="success"
              variant="filled"
              action={
                <Button color="inherit" size="small">
                  Create First Book
                </Button>
              }
            >
              Account created
            </Alert>
          )}
          {isErrorState(createAccountMutation.status) && (
            <Alert
              severity="error"
              variant="filled"
              action={
                <Button
                  color="inherit"
                  size="small"
                  onClick={() => {
                    createAccountMutation.reset();
                  }}
                >
                  Retry
                </Button>
              }
            >
              Account creation failed
            </Alert>
          )}

          <TextField
            label="Display Name"
            helperText={displayNameHelpText(errors)}
            error={!!errors.displayName}
            fullWidth
            variant="filled"
            disabled={formDisabled}
            {...register("displayName", {
              required: true,
              maxLength: 64,
            })}
          />
          <Stack direction="row" spacing={1}>
            <Button
              variant="contained"
              type="submit"
              loading={createAccountMutation.status === "pending"}
              disabled={formDisabled}
            >
              Create Account
            </Button>
          </Stack>
        </Stack>
      </form>
    </>
  );
}

function isErrorState(mutationState: string) {
  switch (mutationState) {
    case "pending":
    case "idle":
      return false;
    default:
      return true;
  }
}

interface IFormValues {
  displayName: string;
}

function displayNameHelpText(
  errors: UseFormReturn<IFormValues>["formState"]["errors"],
) {
  if (errors.displayName) {
    if (errors.displayName.type === "required") {
      return "Required to provide a display name to use this app";
    } else if (errors.displayName.type === "maxLength") {
      return "Display name must be no longer than 64 characters";
    }
  }

  return "Name visible to others using the app";
}
