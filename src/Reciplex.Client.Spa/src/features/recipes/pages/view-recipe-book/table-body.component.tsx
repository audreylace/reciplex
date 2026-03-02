import type { FetchStatus } from "@tanstack/react-query";
import { FetchingRecipeFailedBanner } from "../../components/recipe-banners/fetching-recipe-failed-banner.component";
import { RecipeNotFoundBanner } from "../../components/recipe-banners/recipe-not-found-banner.component";
import type { IGetRecipesInBookResult } from "../../services/recipe-types";
import { RecipeListRow } from "./recipe-list-row.component";
import { RecipeListTableMessage } from "./recipe-list-table-message.component";
import { OfflineBanner } from "../../../core/components/banner/offline-banner.component";
import { RecipeListTableRowsSkeleton } from "./recipe-list-table-rows-skeleton.component";
import { useRecipeListTableContext } from "./useRecipeListTableContext.hook";
import { FetchingStatusDispatch } from "../../../core/components/fetch-status-dispatch/fetch-status-dispatch.component";
import { QueryStatusDispatch } from "../../../core/components/query-status-dispatch/query-status-dispatch.component";

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
  const size = useRecipeListTableContext((state) => state.size);
  return (
    <tbody>
      <QueryStatusDispatch
        loadingStatus={loadingStatus}
        error={
          <RecipeListTableMessage>
            <FetchingRecipeFailedBanner />
          </RecipeListTableMessage>
        }
        pending={
          <FetchingStatusDispatch
            fetchStatus={fetchStatus}
            idle={<FetchingRecipeFailedBanner />}
            paused={
              <RecipeListTableMessage>
                <OfflineBanner />
              </RecipeListTableMessage>
            }
            fetching={<RecipeListTableRowsSkeleton count={size} />}
          />
        }
        success={<SuccessRender recipeData={recipeData} />}
      />
    </tbody>
  );
}

function SuccessRender({
  recipeData,
}: {
  recipeData: IGetRecipesInBookResult | undefined | null;
}) {
  if (!recipeData) {
    return (
      <RecipeListTableMessage>
        <RecipeNotFoundBanner />
      </RecipeListTableMessage>
    );
  }

  if (recipeData.recipes.length === 0) {
    return (
      <RecipeListTableMessage>No recipes in this book</RecipeListTableMessage>
    );
  }

  return recipeData.recipes.map((r, idx) => (
    <RecipeListRow key={r.id} recipe={r} focus={idx === 0} />
  ));
}
