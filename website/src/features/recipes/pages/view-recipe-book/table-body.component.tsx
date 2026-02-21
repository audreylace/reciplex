import type { FetchStatus } from "@tanstack/react-query";
import { FetchingRecipeFailedBanner } from "../../components/fetching-recipe-failed-banner/fetching-recipe-failed-banner.component";
import { RecipeNotFoundBanner } from "../../components/recipe-not-found-banner/recipe-not-found-banner.component";
import type { IGetRecipesInBookResult } from "../../services/recipe-types";
import { RecipeListRow } from "./recipe-list-row.component";
import { RecipeListTableMessage } from "./recipe-list-table-message.component";
import { OfflineBanner } from "../../components/offline-banner/offline-banner.component";
import { RecipeListTableRowsSkeleton } from "./recipe-list-table-rows-skeleton.component";
import { useRecipeListTableContext } from "./useRecipeListTableContext.hook";

export /**
 * Renders the table body
 */
function TableBody({
  loadingStatus,
  recipeData,
  fetchStatus,
}: {
  loadingStatus: "pending" | "error" | "success";
  recipeData: IGetRecipesInBookResult | undefined | null;
  /** the status of the fetch */
  fetchStatus: FetchStatus;
}) {
  return (
    <tbody>
      {(() => {
        switch (loadingStatus) {
          case "success": {
            if (!recipeData) {
              return (
                <RecipeListTableMessage>
                  <RecipeNotFoundBanner />
                </RecipeListTableMessage>
              );
            }

            if (recipeData.recipes.length === 0) {
              return (
                <RecipeListTableMessage>
                  <p>No recipe in this book</p>
                </RecipeListTableMessage>
              );
            }

            return recipeData.recipes.map((r, idx) => (
              <RecipeListRow key={r.id} recipe={r} focus={idx === 0} />
            ));
          }

          case "error": {
            return (
              <RecipeListTableMessage>
                <FetchingRecipeFailedBanner />
              </RecipeListTableMessage>
            );
          }

          case "pending":
            return <FetchingStatus fetchStatus={fetchStatus} />;
        }
      })()}
    </tbody>
  );
}

/**
 * Renders UI when the component is fetching
 */
function FetchingStatus({
  fetchStatus,
}: {
  /** the status of the fetch */
  fetchStatus: FetchStatus;
}) {
  const size = useRecipeListTableContext((state) => state.size);
  switch (fetchStatus) {
    case "fetching":
      return <RecipeListTableRowsSkeleton count={size} />;

    case "idle":
      return <FetchingRecipeFailedBanner />;

    case "paused":
      return (
        <RecipeListTableMessage>
          <OfflineBanner />
        </RecipeListTableMessage>
      );
  }
}
