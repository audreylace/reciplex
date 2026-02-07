import { NavLink, Outlet } from "react-router";
import styles from "./default-layout.module.css";
import { useQuery } from "@tanstack/react-query";
import { useContext, useEffect, useLayoutEffect, useState } from "preact/hooks";
import { RecipeStore } from "../../features/recipes/hooks/useRecipeStoreContext.hook";
import { useLayoutTitle } from "./default-layout.state";

export function DefaultLayout({}: {}) {
  return (
    <div className={styles.mainWrapper}>
      <Outlet />
    </div>
  );
}

function User() {
  const recipeStore = useContext(RecipeStore);
  const query = useQuery({
    queryKey: ["session-info"],
    enabled: !!recipeStore,
    queryFn: async () => {
      const sessionInfo = await recipeStore?.getSessionInformation();
      return sessionInfo;
    },
  });

  return (
    <>
      <li className={styles.grow}></li>
      {query.data?.isAuthenticated && (
        <li>{query.data.userData?.displayName ?? "unknown user"}</li>
      )}
      {query.isSuccess && !query.data?.isAuthenticated && <li>Sign in</li>}
      {query.isLoading && <li>Loading...</li>}
      {query.isError && <li>Failed to resolve user</li>}
    </>
  );
}

function AppNavigation() {
  const title = useLayoutTitle((state) => state.title);
  const [laggedTitle, setLaggedTitle] = useState("");

  useLayoutEffect(() => {
    const handle = setTimeout(() => setLaggedTitle(title), 20); // reduce flicker on page change
    return () => {
      clearTimeout(handle);
    };
  }, [title]);

  return (
    <div
      className={`navbar navbar-expand-lg text-bg-danger shadow ${styles.appNavigationDiv}`}
    >
      <div class="container-fluid">
        <div class="navbar-brand">
          <span className={styles.appTitle}>Reciplex</span>
          <span className={styles.pageTitle}>
            {laggedTitle ? laggedTitle : "Reciplex"}
          </span>
        </div>
        <button
          class="navbar-toggler"
          type="button"
          data-bs-toggle="collapse"
          data-bs-target="#navbarSupportedContent"
          aria-controls="navbarSupportedContent"
          aria-expanded="false"
          aria-label="Toggle navigation"
        >
          <span class="navbar-toggler-icon"></span>
        </button>
        <div class="collapse navbar-collapse" id="navbarSupportedContent">
          <ul className={`navbar-nav ${styles.navBar}`}>
            <li>
              <NavLink to="/">Home</NavLink>
            </li>
            <li>
              <NavLink to="/create-recipe-book">Create Recipe Book</NavLink>
            </li>
            <li>
              <NavLink to="/recipe-books">List Recipe Books</NavLink>
            </li>
            <User />
          </ul>
        </div>
      </div>
    </div>
  );
}
