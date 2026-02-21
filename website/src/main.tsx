/* eslint-disable react/react-in-jsx-scope */
import { render } from "preact";
import { createBrowserRouter } from "react-router";
import { RouterProvider } from "react-router/dom";
import { HomePage } from "./pages/home/home-page.tsx";
import { RecipeBookListPage } from "./features/recipes/pages/recipe-book-list/recipe-book-list-page.tsx";
import { ViewRecipeBookPage } from "./features/recipes/pages/view-recipe-book/view-recipe-book-page.tsx";
import { CreateRecipeBookPage } from "./features/recipes/pages/create-recipe-book/create-recipe-book-page.tsx";
import { CreateRecipePage } from "./features/recipes/pages/create-recipe/create-recipe-page.tsx";
import { ViewRecipePage } from "./features/recipes/pages/view-recipe/view-recipe-page.component.tsx";
import { EditRecipePage } from "./features/recipes/pages/edit-recipe/edit-recipe-page.component.tsx";
import { DefaultLayout } from "./layouts/default/default-layout.tsx";
import { LocalRecipeBookStoreImplementation } from "./services/recipe-store/LocalRecipeBookStoreImplementation.ts";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { RecipeStore } from "./features/recipes/hooks/useRecipeStoreContext.hook.ts";
import { DeleteRecipePage } from "./features/recipes/pages/delete-recipe/delete-recipe-page.component.tsx";
import { DeleteRecipeBookPage } from "./features/recipes/pages/delete-recipe-book/delete-recipe-book-page.tsx";
import { EditRecipeBookPage } from "./features/recipes/pages/edit-recipe-book/edit-recipe-book-page.tsx";
import { RecipeHttpBookStore } from "./features/recipes/services/recipe-http-book-store.service.ts";

import "./index.css";
import { BookLayout } from "./layouts/book-layout/book-layout.component.tsx";

const router = createBrowserRouter([
  {
    path: "/books/:bookId",
    Component: BookLayout,
    children: [
      {
        path: "recipes/-/create",
        element: <CreateRecipePage />,
      },
      {
        path: "recipes/:recipeId",
        element: <ViewRecipePage />,
      },
      {
        path: "recipes/:recipeId/edit",
        element: <EditRecipePage />,
      },
      {
        path: "recipes/:recipeId/delete",
        element: <DeleteRecipePage />,
      },
      {
        path: "delete",
        element: <DeleteRecipeBookPage />,
      },
      {
        path: "edit",
        element: <EditRecipeBookPage />,
      },
      {
        path: "",
        element: <ViewRecipeBookPage />,
      },
    ],
  },
  {
    path: "/",
    Component: DefaultLayout,
    children: [
      {
        path: "/",
        element: <HomePage />,
      },
      {
        path: "/recipe-books",
        element: <RecipeBookListPage />,
      },
      {
        path: "/recipe-books/:source/:index",
        element: <RecipeBookListPage />,
      },
      {
        path: "/recipe-books/:source",
        element: <RecipeBookListPage />,
      },
      {
        path: "/create-recipe-book",
        element: <CreateRecipeBookPage />,
      },
    ],
  },
]);

const queryClient = new QueryClient();
//const localOnlyStore = new LocalRecipeBookStoreImplementation();
const serverStore = new RecipeHttpBookStore("/api");
render(
  <RecipeStore.Provider value={serverStore}>
    <QueryClientProvider client={queryClient}>
      <RouterProvider router={router} />
    </QueryClientProvider>
  </RecipeStore.Provider>,
  document.getElementById("app")!,
);

// Flip theme based on system state. Bootstrap uses a global attribute
// instead of a media query. Hence, we will simulate the behavior of the media query with
// javascript.
const getPreferredScheme = () =>
  window.matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light";

// the window is by default in dark mode to prevent a flash
if (getPreferredScheme() === "light") {
  document.documentElement.setAttribute("data-bs-theme", "");
}

document.body.style.background = "";

window
  .matchMedia("(prefers-color-scheme: dark)")
  .addEventListener("change", () => {
    if (getPreferredScheme() === "light") {
      document.documentElement.setAttribute("data-bs-theme", "");
    } else {
      document.documentElement.setAttribute("data-bs-theme", "dark");
    }
  });
