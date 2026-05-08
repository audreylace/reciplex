import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import { useSignUpNavigate } from "../../hooks/useSignUpNavigate.hook";
import { Link } from "react-router";

/** Navigates user to the sign-up page on click */
export function AddAccountButton() {
  const path = useSignUpNavigate()[0];
  return (
    <Box>
      <Button component={Link} to={path}>
        Add Account
      </Button>
    </Box>
  );
}
