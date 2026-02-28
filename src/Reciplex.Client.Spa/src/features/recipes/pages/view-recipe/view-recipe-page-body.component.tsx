import type { FetchStatus } from "@tanstack/react-query";
import { useNavigate } from "react-router";
import { FetchingStatusDispatch } from "../../../core/components/fetch-status-dispatch/fetch-status-dispatch.component";
import {
  QueryStatusDispatch,
  type LoadingStatusValues,
} from "../../../core/components/query-status-dispatch/query-status-dispatch.component";
import { FetchingRecipeBanner } from "../../components/recipe-banners/fetching-recipe-banner.component";
import { FetchingRecipeFailedBanner } from "../../components/recipe-banners/fetching-recipe-failed-banner.component";
import { OfflineBanner } from "../../../core/components/banner/offline-banner.component";
import { RecipeNotFoundBanner } from "../../components/recipe-banners/recipe-not-found-banner.component";
import { makeRecipeNameAndDescriptionState } from "../../components/recipe-title-and-description/recipe-title-and-description.component";
import { makeEditRecipePath } from "../../route-utils";
import type { IGetRecipeByIdResult } from "../../services/recipe-types";
import { DetailsRender } from "./details-render.component";

export function ViewRecipePageBody({
  loadingStatus,
  data,
  fetchStatus,
}: {
  /** overall status of the load */
  loadingStatus: LoadingStatusValues;
  /** data fetched by the load */
  data: IGetRecipeByIdResult | undefined | null;
  /** the status of the fetch when `loadingStatus` is `pending` */
  fetchStatus: FetchStatus;
}) {
  return (
    <QueryStatusDispatch
      loadingStatus={loadingStatus}
      error={<FetchingRecipeFailedBanner />}
      pending={
        <FetchingStatusDispatch
          fetchStatus={fetchStatus}
          fetching={<FetchingRecipeBanner />}
          paused={<OfflineBanner />}
          idle={<FetchingRecipeFailedBanner />}
        />
      }
      success={<SuccessRender data={data} />}
    />
  );
}

function SuccessRender({
  data,
}: {
  /** data fetched by the load */
  data: IGetRecipeByIdResult | undefined | null;
}) {
  const navigate = useNavigate();
  const goToEditAction = () => {
    if (!data) {
      return;
    }
    navigate(makeEditRecipePath(data.recipe.bookId, data.recipe.id), {
      state: makeRecipeNameAndDescriptionState(
        data.recipe.name,
        data.recipe.shortDescription,
      ),
    });
  };

  if (!data) {
    return <RecipeNotFoundBanner />;
  }

  return (
    <DetailsRender
      detailsMd={data.recipe.details}
      mayEdit={data.recipe.mayEdit}
      goToEditAction={goToEditAction}
    />
  );
}
