/* eslint-disable react/react-in-jsx-scope */
import { render } from "preact";
import { createBrowserRouter } from "react-router";
import { RouterProvider } from "react-router/dom";
import { HomePage } from "./pages/home/home-page.tsx";
import { RecipeBookListPage } from "./pages/recipe-book-list/recipe-book-list-page.tsx";
import { ViewRecipeBookPage } from "./pages/view-recipe-book/view-recipe-book-page.tsx";
import { CreateRecipeBookPage } from "./pages/create-recipe-book/create-recipe-book-page.tsx";
import { CreateRecipePage } from "./pages/create-recipe/create-recipe-page.tsx";
import { ViewRecipePage } from "./pages/view-recipe/view-recipe-page.tsx";
import { EditRecipePage } from "./pages/edit-recipe/edit-recipe-page.tsx";
import { DefaultLayout } from "./layouts/default/default-layout.tsx";
import { LocalRecipeBookStoreImplementation } from "./services/recipe-store/LocalRecipeBookStoreImplementation.ts";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { RecipeStore } from "./features/recipes/hooks/useRecipeStoreContext.hook.ts";
import { DeleteRecipePage } from "./pages/delete-recipe/delete-recipe-page.tsx";

const router = createBrowserRouter([
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
        path: "/recipe-book/:bookId",
        element: <ViewRecipeBookPage />,
      },
      {
        path: "/create-recipe-book",
        element: <CreateRecipeBookPage />,
      },
      {
        path: "/create-recipe/:bookId",
        element: <CreateRecipePage />,
      },
      {
        path: "/view-recipe/:recipeId",
        element: <ViewRecipePage />,
      },
      {
        path: "/edit-recipe/:recipeId",
        element: <EditRecipePage />,
      },
      {
        path: "/delete-recipe/:recipeId",
        element: <DeleteRecipePage />,
      },
    ],
  },
]);

const queryClient = new QueryClient();
const localOnlyStore = new LocalRecipeBookStoreImplementation();
render(
  <RecipeStore.Provider value={localOnlyStore}>
    <QueryClientProvider client={queryClient}>
      <RouterProvider router={router} />
    </QueryClientProvider>
  </RecipeStore.Provider>,
  document.getElementById("app")!,
);
