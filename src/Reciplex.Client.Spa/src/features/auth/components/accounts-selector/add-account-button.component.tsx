import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import { useSignUpNavigate } from "../../hooks/useSignUpNavigate.hook";

/** Navigates user to the sign-up page on click */
export function AddAccountButton() {
  const [path, action] = useSignUpNavigate();
  return (
    <Box>
      <Button
        href={path}
        onClick={(e) => {
          e.preventDefault();
          action();
        }}
      >
        Add Account
      </Button>
    </Box>
  );
}
