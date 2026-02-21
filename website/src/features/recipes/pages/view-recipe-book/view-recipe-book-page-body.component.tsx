import type { FetchStatus } from "@tanstack/react-query";
import { FetchingRecipeBookBanner } from "../../components/fetching-recipe-book-banner/fetching-recipe-book-banner.component";
import { FetchingRecipeBookFailedBanner } from "../../components/fetching-recipe-book-failed-banner/fetching-recipe-book-failed-banner.component";
import { OfflineBanner } from "../../components/offline-banner/offline-banner.component";
import { RecipeBookNotFoundBanner } from "../../components/recipe-book-not-found-banner/recipe-book-not-found-banner.component";
import type { IRecipeBookModel } from "../../services/recipe-types";
import { RecipeListTable } from "./recipe-list-table.component";
import { RecipeBookMenu } from "./recipe-book-menu.component";
import { QueryStatusDispatch } from "../../../core/components/query-status-dispatch/query-status-dispatch.component";
import { FetchingStatusDispatch } from "../../../core/components/fetch-status-dispatch/fetch-status-dispatch.component";

/** the body of the page */
export function ViewRecipeBookPageBody({
  loadingStatus,
  bookData,
  fetchStatus,
}: {
  /** overall status of the load */
  loadingStatus: "pending" | "error" | "success";
  /** data fetched by the load */
  bookData: IRecipeBookModel | undefined | null;
  /** the status of the fetch when `loadingStatus` is `pending` */
  fetchStatus: FetchStatus;
}) {
  return (
    <QueryStatusDispatch
      loadingStatus={loadingStatus}
      error={<FetchingRecipeBookFailedBanner />}
      pending={
        <FetchingStatusDispatch
          fetchStatus={fetchStatus}
          fetching={<FetchingRecipeBookBanner />}
          idle={<FetchingRecipeBookFailedBanner />}
          paused={<OfflineBanner />}
        />
      }
      success={() => {
        if (!bookData) {
          return <RecipeBookNotFoundBanner />;
        }

        return (
          <>
            <RecipeBookMenu
              bookId={bookData.id}
              name={bookData.name}
              shortDescription={bookData.shortDescription}
              mayEdit={bookData.mayEdit}
              mayDelete={bookData.mayDelete}
            />
            <RecipeListTable bookId={bookData.id} />
          </>
        );
      }}
    />
  );
}
