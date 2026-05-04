import { NavLink, Outlet, useNavigate } from "react-router";
import AccountCircle from "@mui/icons-material/AccountCircle";
import {
  useScrollTrigger,
  Slide,
  AppBar,
  Toolbar,
  Typography,
  Button,
  IconButton,
  Box,
} from "@mui/material";
import { useSignInNavigate } from "../../features/auth/hooks/useSignInNavigate.hook";
import { makeBookListPath } from "../../features/recipes/route-utils";
import styles from "./default-layout.module.css";

export function DefaultLayout() {
  return (
    <>
      <LayoutBar fixed />
      <LayoutBar />
      <main className="pageMain">
        <Outlet />
      </main>
    </>
  );
}
function LayoutBar({ fixed }: { fixed?: boolean }) {
  const navigate = useNavigate();

  const signInNavigate = useSignInNavigate()[1];

  const trigger = useScrollTrigger({
    target: window,
  });

  return (
    <Slide appear={false} direction="down" in={!trigger}>
      <AppBar position={fixed ? "fixed" : "static"}>
        <Toolbar>
          <Typography variant="h6" component="div" sx={{ mr: 3 }}>
            <Button
              onClick={() => navigate("/")}
              sx={{
                color: "white",
                display: "block",
                textDecoration: "none",
              }}
            >
              <Typography variant="subtitle1">Reciplex</Typography>
            </Button>
          </Typography>
          <NavLink to={makeBookListPath()} className={styles.navLink}>
            <Typography variant="subtitle1">Books</Typography>
          </NavLink>

          <Box sx={{ flex: "1 0 auto" }}></Box>
          <IconButton
            size="large"
            aria-label="account of current user"
            onClick={() => {
              signInNavigate();
            }}
            color="inherit"
          >
            <AccountCircle />
          </IconButton>
        </Toolbar>
      </AppBar>
    </Slide>
  );
}
