import { createBrowserRouter } from "react-router";
import { CreateRecipeBookPage } from "./features/recipes/pages/create-recipe-book/create-recipe-book-page.hook";
import { CreateRecipePage } from "./features/recipes/pages/create-recipe/create-recipe-page.component";
import { DeleteRecipeBookPage } from "./features/recipes/pages/delete-recipe-book/delete-recipe-book-page.component";
import { DeleteRecipePage } from "./features/recipes/pages/delete-recipe/delete-recipe-page.component";
import { EditRecipeBookPage } from "./features/recipes/pages/edit-recipe-book/edit-recipe-book-page.component";
import { EditRecipePage } from "./features/recipes/pages/edit-recipe/edit-recipe-page.component";
import { RecipeBookListPage } from "./features/recipes/pages/recipe-book-list/recipe-book-list-page";
import { ViewRecipeBookPage } from "./features/recipes/pages/view-recipe-book/view-recipe-book-page.component";
import { ViewRecipePage } from "./features/recipes/pages/view-recipe/view-recipe-page.component";
import { DefaultLayout } from "./layouts/default/default-layout";
import { HomePage } from "./pages/home/home-page.component";
import { BookLayout } from "./features/recipes/layouts/book-layout/book-layout.component";

export const router = createBrowserRouter([
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
