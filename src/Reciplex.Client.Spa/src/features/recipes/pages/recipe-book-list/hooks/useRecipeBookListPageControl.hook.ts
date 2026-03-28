import { useSearchParams } from "react-router";
import type { BookListNavigationAction } from "../../../route-utils";
import { useFakeLoading } from "./useFakeLoading.hook";
import { useRecipeBookListQuery } from "./useRecipeBookListQuery.hook";
import type { IGetRecipeBooksResult } from "../../../services/recipe-types";

export function useRecipeBookListPageControl(): {
  data?: IGetRecipeBooksResult;
  isPending?: boolean;
  isPaused?: boolean;
} {
  const searchParams = useSearchParams()[0];
  const source = searchParams.get("source") as
    | BookListNavigationAction
    | undefined;
  const index = searchParams.get("index") as string | undefined;

  const query = useRecipeBookListQuery(source, index);
  const fakeLoading = useFakeLoading(source, index);

  if (fakeLoading) {
    return {
      isPending: true,
    };
  }

  if (query.isSuccess && query.data) {
    return { data: query.data };
  }

  if (query.isPending) {
    return { isPending: true };
  }

  if (query.isPaused) {
    return { isPaused: true };
  }

  return {};
}
