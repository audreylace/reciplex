import { createBrowserRouter } from "react-router";
import { ErrorBoundary } from "./pages/error-boundary/error-boundary.component";

export const router = createBrowserRouter([
  {
    ErrorBoundary: ErrorBoundary,
    path: "/books",
    lazy: async () => {
      const Component =
        await import("./features/recipes/layouts/book-layout/book-layout.component");
      return { Component: Component.BookLayout };
    },
    children: [
      {
        path: "",
        lazy: async () => {
          const Component =
            await import("./features/recipes/pages/recipe-book-list/recipe-book-list-page");
          return { Component: Component.RecipeBookListPage };
        },
      },
    ],
  },
  {
    ErrorBoundary: ErrorBoundary,
    path: "/books/-/create",
    lazy: async () => {
      const Component =
        await import("./features/recipes/layouts/book-layout/book-layout.component");
      return { Component: Component.BookLayout };
    },
    children: [
      {
        path: "",
        lazy: async () => {
          const Component =
            await import("./features/recipes/pages/create-recipe-book/create-recipe-book-page.component");
          return { Component: Component.CreateRecipeBookPage };
        },
      },
    ],
  },
  {
    ErrorBoundary: ErrorBoundary,
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
        path: "settings",
        lazy: async () => {
          const Component =
            await import("./features/recipes/pages/book-setting-landing/book-setting-landing-page.component");
          return { Component: Component.BookSettingLandingPage };
        },
      },
      {
        path: "settings/delete",
        lazy: async () => {
          const Component =
            await import("./features/recipes/pages/delete-recipe-book/delete-recipe-book-page.component");
          return { Component: Component.DeleteRecipeBookPage };
        },
      },
      {
        path: "settings/details",
        lazy: async () => {
          const Component =
            await import("./features/recipes/pages/edit-recipe-book/edit-recipe-book-page.component");
          return { Component: Component.EditRecipeBookPage };
        },
      },
      {
        path: "settings/invitation",
        lazy: async () => {
          const Component =
            await import("./features/recipes/pages/share-recipe-book/share-recipe-book-page");
          return { Component: Component.ShareRecipeBookPage };
        },
      },
      {
        path: "settings/manage-access",
        lazy: async () => {
          const Component =
            await import("./features/recipes/pages/manage-user-access/manage-user-access-page.component");
          return { Component: Component.ManageUserAccessPage };
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
    ErrorBoundary: ErrorBoundary,
    path: "/accounts",
    lazy: async () => {
      const Component = await import("./layouts/default/default-layout");
      return { Component: Component.DefaultLayout };
    },
    children: [
      {
        path: ":accountKey/delete",
        lazy: async () => {
          const Component =
            await import("./features/auth/pages/delete-account/delete-account-page.component");
          return { Component: Component.DeleteAccountPage };
        },
      },
      {
        path: "-/select",
        lazy: async () => {
          const Component =
            await import("./features/auth/pages/select-account/select-account-page.component");
          return { Component: Component.SelectAccountPage };
        },
      },
      {
        path: "-/sign-in",
        lazy: async () => {
          const Component =
            await import("./features/auth/pages/sign-in/sign-in-page.component");
          return { Component: Component.SignInPage };
        },
      },
      {
        path: "-/sign-up",
        lazy: async () => {
          const Component =
            await import("./features/auth/pages/sign-up/sign-up-page.component");
          return { Component: Component.SignUpPage };
        },
      },
      {
        path: ":accountKey/settings",
        lazy: async () => {
          const Component =
            await import("./features/auth/pages/account-settings/account-settings-page.component");
          return { Component: Component.AccountSettingsPage };
        },
      },
    ],
  },
  {
    ErrorBoundary: ErrorBoundary,
    path: "/",
    lazy: async () => {
      const Component = await import("./layouts/default/default-layout");
      return { Component: Component.DefaultLayout };
    },
    children: [
      {
        path: "books/:bookId/invitation/:shareKey",
        lazy: invitePageComponent,
      },
      {
        path: "books/:bookId/invitation",
        lazy: invitePageComponent,
      },
      {
        path: "/",
        lazy: async () => {
          const Component = await import("./pages/home/home-page.component");
          return { Component: Component.HomePage };
        },
      },
    ],
  },
  {
    ErrorBoundary: ErrorBoundary,
    path: "*",
    lazy: async () => {
      const Component = await import("./layouts/default/default-layout");
      return { Component: Component.DefaultLayout };
    },
    children: [
      {
        path: "*",
        lazy: async () => {
          const Component =
            await import("./pages/not-found/not-found.component");
          return { Component: Component.NotFound };
        },
      },
    ],
  },
]);

async function invitePageComponent() {
  const Component =
    await import("./features/recipes/pages/accept-book-invite/accept-book-invite-page.component");
  return { Component: Component.AcceptBookInvitePage };
}
