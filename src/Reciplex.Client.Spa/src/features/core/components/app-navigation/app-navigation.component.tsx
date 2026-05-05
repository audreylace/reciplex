import { NavLink, useNavigate } from "react-router";
import styles from "./app-navigation.module.css";
import type { PropsWithChildren } from "preact/compat";
import { useSignInNavigate } from "../../../auth/hooks/useSignInNavigate.hook";
import Slide from "@mui/material/Slide";
import AppBar from "@mui/material/AppBar";
import Typography from "@mui/material/Typography";
import Toolbar from "@mui/material/Toolbar";
import Button from "@mui/material/Button";
import Box from "@mui/material/Box";
import IconButton from "@mui/material/IconButton";
import AccountCircle from "@mui/icons-material/AccountCircle";
import { makeBookListPath } from "../../../recipes/route-utils";
import useScrollTrigger from "@mui/material/useScrollTrigger";
import type { ComponentChildren } from "preact";

export function AppNavigation({
  children,
  fixed,
  invisible,
  removeLinks,
}: IAppNavigationProps) {
  const navigate = useNavigate();

  const signInNavigate = useSignInNavigate()[1];

  const trigger = useScrollTrigger({
    target: window,
  });

  return (
    <VisibilityBox invisible={invisible}>
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
            {!removeLinks && (
              <NavLink to={makeBookListPath()} className={styles.navLink}>
                <Typography variant="subtitle1">Books</Typography>
              </NavLink>
            )}
            {children}
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
    </VisibilityBox>
  );
}

function VisibilityBox({ children, invisible }: IVisibilityBoxProps) {
  if (!invisible) {
    return <Box>{children}</Box>;
  }
  return <Box sx={{ visibility: "hidden" }}>{children}</Box>;
}

export interface IAppNavigationProps extends PropsWithChildren {
  fixed?: boolean;
  invisible?: boolean;
  removeLinks?: boolean;
}

export interface IVisibilityBoxProps {
  invisible?: boolean;
  children: ComponentChildren;
}
