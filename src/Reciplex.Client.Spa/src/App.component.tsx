import { QueryClientProvider } from "@tanstack/react-query";
import { RecipeHttpBookStore } from "./features/recipes/services/recipe-http-book-store.service";
import { RouterProvider } from "react-router";
import { RecipeStore } from "./features/recipes/hooks/useRecipeStoreContext.hook";
import { router } from "./routes";
import { ChallengeHttpClient } from "./features/auth/http-clients/challenge-http-client";
import { UsersHttpClient } from "./features/auth/http-clients/users-http-client";
import {
  AuthClients,
  type IAuthClients,
} from "./features/auth/hooks/useAuthClients.hook";

import "@fontsource/roboto/300.css";
import "@fontsource/roboto/400.css";
import "@fontsource/roboto/500.css";
import "@fontsource/roboto/700.css";
import { AppTheme } from "./features/core/components/app-theme/app-theme.component";
import { makeClient } from "./features/core/utils/react-query-config";
import CssBaseline from "@mui/material/CssBaseline";
import "./index.css";
import { PropagateBrowserTitle } from "./features/core/components/browser-title/propagate-browser-title.component";
import { XsrfHttpClient } from "./features/auth/http-clients/xsrf-http-client";
import { type IHttpClientArgs } from "./features/core/utils/http-client";
import { XsrfMiddleware } from "./features/auth/utils/xsrf-middleware";
import { SignOutClient } from "./features/auth/hooks/useSignOutClient.hook";
import { SignOutHttpClient } from "./features/auth/http-clients/sign-out-http-client";
import { HttpClientMiddlewarePipeline } from "./features/core/utils/http-client-middleware-pipeline";

const queryClient = makeClient();
const xsrfClient = new XsrfHttpClient("/api");
const pipeline = new HttpClientMiddlewarePipeline();
pipeline.addBeforeFetchHandler(
  new XsrfMiddleware(xsrfClient).onBeforeFetchHandle(),
);
const httpArgs: IHttpClientArgs = {
  pipeline: pipeline,
};
const serverStore = new RecipeHttpBookStore("/api", httpArgs);
const challengeClient = new ChallengeHttpClient("/api", httpArgs);
const userClient = new UsersHttpClient("/api", httpArgs);
const authStoreContext: IAuthClients = {
  challengeClient: challengeClient,
  usersClient: userClient,
};
const signOutClient = new SignOutHttpClient("/api", httpArgs);

export function App() {
  return (
    <>
      <AppTheme>
        <CssBaseline />
        <SignOutClient.Provider value={signOutClient}>
          <AuthClients.Provider value={authStoreContext}>
            <RecipeStore.Provider value={serverStore}>
              <QueryClientProvider client={queryClient}>
                <RouterProvider router={router} />
              </QueryClientProvider>
            </RecipeStore.Provider>
          </AuthClients.Provider>
        </SignOutClient.Provider>
      </AppTheme>
      <PropagateBrowserTitle />
    </>
  );
}
