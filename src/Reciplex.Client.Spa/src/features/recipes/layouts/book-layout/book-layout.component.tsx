import { NavLink, Outlet, useNavigate, useParams } from "react-router";
import { useGetRecipeBookById } from "../../hooks/useGetRecipeBookById.hook";
import { useGetRecipeByIdQuery } from "../../hooks/useGetRecipeByIdQuery.hook";
import {
  makeBookListPath,
  makeViewRecipeBookPath,
  makeViewRecipePath,
} from "../../route-utils";
import { AuthenticatedRouteGuard } from "../../../auth/components/authenticated-route-guard/authenticated-route-guard.component";
import AppBar from "@mui/material/AppBar";
import Toolbar from "@mui/material/Toolbar";
import Typography from "@mui/material/Typography";
import Button from "@mui/material/Button";
import IconButton from "@mui/material/IconButton";
import AccountCircle from "@mui/icons-material/AccountCircle";
import Box from "@mui/material/Box";
import useScrollTrigger from "@mui/material/useScrollTrigger";
import Slide from "@mui/material/Slide";
import styles from "./book-layout.module.css";
import { useSignInNavigate } from "../../../auth/hooks/useSignInNavigate.hook";

export function BookLayout() {
  return (
    <AuthenticatedRouteGuard>
      <BookLayoutBar2 fixed />
      <Box sx={{ visibility: "hidden" }}>
        <BookLayoutBar2 fixed={false} />
      </Box>
      <main className="pageMain">
        <Outlet />
      </main>
    </AuthenticatedRouteGuard>
  );
}

function BookLayoutBar2({ fixed }: { fixed: boolean }) {
  const navigate = useNavigate();
  const { bookId, recipeId } = useParams<{
    bookId?: string;
    recipeId?: string;
  }>();
  const bookQuery = useGetRecipeBookById(bookId);
  const recipeQuery = useGetRecipeByIdQuery(recipeId);
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

          <ul className={styles.crumbTrail}>
            <li className={styles.crumbElement}>
              <NavLink to={makeBookListPath()}>
                <span className={styles.bookCrumb}>
                  <Typography variant="subtitle1">Books</Typography>
                </span>
              </NavLink>
            </li>
            {bookId && (
              <>
                <CrumbDivider />
                <li className={styles.crumbElement}>
                  <NavLink to={makeViewRecipeBookPath(bookId ?? "")}>
                    <span className={styles.bookCrumb}>
                      <Typography variant="subtitle1">
                        {bookQuery.data?.name ?? ""}
                      </Typography>
                    </span>
                  </NavLink>
                </li>
              </>
            )}
            {recipeId && bookId && recipeQuery.data && (
              <>
                <CrumbDivider />
                <li className={styles.crumbElement}>
                  <NavLink to={makeViewRecipePath(bookId, recipeId)}>
                    <span className={styles.recipeCrumb}>
                      <Typography variant="subtitle1">
                        {recipeQuery.data?.name ?? ""}
                      </Typography>
                    </span>
                  </NavLink>
                </li>
              </>
            )}
          </ul>

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

function CrumbDivider() {
  return <li className={styles.crumbDivider}>/</li>;
}
