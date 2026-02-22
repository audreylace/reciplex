import { createBrowserRouter } from "react-router";

export const router = createBrowserRouter([
  {
    path: "/books/:bookId",
    lazy: async () => {
      const Component =
        await import("./features/recipes/layouts/book-layout/book-layout.component");
      return { Component: Component.BookLayout };
    },
    children: [
      {
        path: "recipes/-/create",
        lazy: async () => {
          const Component =
            await import("./features/recipes/pages/create-recipe/create-recipe-page.component");
          return { Component: Component.CreateRecipePage };
        },
      },
      {
        path: "recipes/:recipeId",
        lazy: async () => {
          const Component =
            await import("./features/recipes/pages/view-recipe/view-recipe-page.component");
          return { Component: Component.ViewRecipePage };
        },
      },
      {
        path: "recipes/:recipeId/edit",
        lazy: async () => {
          const Component =
            await import("./features/recipes/pages/edit-recipe/edit-recipe-page.component");
          return { Component: Component.EditRecipePage };
        },
      },
      {
        path: "recipes/:recipeId/delete",
        lazy: async () => {
          const Component =
            await import("./features/recipes/pages/delete-recipe/delete-recipe-page.component");
          return { Component: Component.DeleteRecipePage };
        },
      },
      {
        path: "delete",
        lazy: async () => {
          const Component =
            await import("./features/recipes/pages/delete-recipe-book/delete-recipe-book-page.component");
          return { Component: Component.DeleteRecipeBookPage };
        },
      },
      {
        path: "edit",
        lazy: async () => {
          const Component =
            await import("./features/recipes/pages/edit-recipe-book/edit-recipe-book-page.component");
          return { Component: Component.EditRecipeBookPage };
        },
      },
      {
        path: "",
        lazy: async () => {
          const Component =
            await import("./features/recipes/pages/view-recipe-book/view-recipe-book-page.component");
          return { Component: Component.ViewRecipeBookPage };
        },
      },
    ],
  },
  {
    path: "/",
    lazy: async () => {
      const Component = await import("./layouts/default/default-layout");
      return { Component: Component.DefaultLayout };
    },
    children: [
      {
        path: "/",
        lazy: async () => {
          const Component = await import("./pages/home/home-page.component");
          return { Component: Component.HomePage };
        },
      },
      {
        path: "/recipe-books",
        lazy: RecipeBooksLazy,
      },
      {
        path: "/recipe-books/:source/:index",
        lazy: RecipeBooksLazy,
      },
      {
        path: "/recipe-books/:source",
        lazy: RecipeBooksLazy,
      },
      {
        path: "/create-recipe-book",
        lazy: async () => {
          const Component =
            await import("./features/recipes/pages/create-recipe-book/create-recipe-book-page.component");
          return { Component: Component.CreateRecipeBookPage };
        },
      },
    ],
  },
]);

async function RecipeBooksLazy() {
  const Component =
    await import("./features/recipes/pages/recipe-book-list/recipe-book-list-page");
  return { Component: Component.RecipeBookListPage };
}
