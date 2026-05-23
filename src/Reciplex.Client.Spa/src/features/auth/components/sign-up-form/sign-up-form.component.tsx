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
import Fade from "@mui/material/Fade";
import { Link } from "react-router";
import Paper from "@mui/material/Paper";

/** Form for creating an account */
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
    navigate(makeCreateRecipeBookPath(), { replace: true });
  });

  const formDisabled = createAccountMutation.status !== "idle";

  return (
    <>
      <Paper sx={{ p: 2 }}>
        <Typography variant="h4" gutterBottom>
          Create an account and get cooking with Reciplex!
        </Typography>
        <form onSubmit={onSubmit}>
          <Stack spacing={2} sx={{ mt: 3 }}>
            {createAccountMutation.isSuccess && (
              <Fade in={true} timeout={500}>
                <Alert
                  severity="success"
                  variant="filled"
                  action={
                    <Button
                      component={Link}
                      to={makeCreateRecipeBookPath()}
                      color="inherit"
                      size="small"
                    >
                      Create First Book
                    </Button>
                  }
                >
                  Account created
                </Alert>
              </Fade>
            )}
            {createAccountMutation.isError && (
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
              variant="outlined"
              disabled={formDisabled}
              {...register("displayName", {
                required: "Required to provide a display name to use this app",
                maxLength: {
                  value: 64,
                  message: "Display name must be no longer than 64 characters",
                },
              })}
            />
            <Stack direction="row" spacing={1}>
              <Button
                variant="contained"
                type="submit"
                loading={createAccountMutation.isPending}
                disabled={formDisabled}
              >
                Create Account
              </Button>
            </Stack>
          </Stack>
        </form>
      </Paper>
    </>
  );
}

/** form model */
interface IFormValues {
  /** backing model for the display name field  */
  displayName: string;
}

/**
 * Computes the help text for the display name
 * @param errors input errors
 * @returns the help text to show for the input field
 */
function displayNameHelpText(
  errors: UseFormReturn<IFormValues>["formState"]["errors"],
) {
  if (errors.displayName) {
    return errors.displayName.message;
  }

  return "Name visible to others using the app";
}
