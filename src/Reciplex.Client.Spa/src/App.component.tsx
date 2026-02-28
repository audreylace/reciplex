import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { RecipeHttpBookStore } from "./features/recipes/services/recipe-http-book-store.service";
import { RouterProvider } from "react-router";
import { RecipeStore } from "./features/recipes/hooks/useRecipeStoreContext.hook";
import { router } from "./routes";
import "./index.css";

const queryClient = new QueryClient();
const serverStore = new RecipeHttpBookStore("/api");
export function App() {
  return (
    <RecipeStore.Provider value={serverStore}>
      <QueryClientProvider client={queryClient}>
        <RouterProvider router={router} />
      </QueryClientProvider>
    </RecipeStore.Provider>
  );
}
