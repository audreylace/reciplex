import { NavLink, Outlet } from "react-router";
import styles from "./default-layout.module.css";
import { useQuery } from "@tanstack/react-query";
import { useContext } from "preact/hooks";
import { RecipeStore } from "../../features/recipes/hooks/useRecipeStoreContext.hook";

export function DefaultLayout({}: {}) {
  return (
    <>
      <ul class={styles.navBar}>
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
      <Outlet />
    </>
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
