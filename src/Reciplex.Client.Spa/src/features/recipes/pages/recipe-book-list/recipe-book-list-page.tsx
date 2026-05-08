import { useSearchParams } from "react-router";
import { useRecipeBookListQuery } from "../../hooks/useRecipeBookListQuery.hook";
import { LoadingFailedAlert } from "../../../core/components/loading-failed-alert/loading-failed-alert.component";
import { BookTable } from "../../components/book-table/book-table.component";
import { useRecipeClientStateContext } from "../../hooks/useRecipeClientStateContext.hook";
import { useEffect, useRef } from "preact/hooks";
import { PageHeader } from "../../../core/components/page-header/page-header.component";

/**
 * Entry point for recipe book list page component
 */
export function RecipeBookListPage() {
  const bookSize = useRecipeClientStateContext((s) => s.bookListPageSize);
  const searchParams = useSearchParams()[0];
  const source =
    searchParams.get("source") === "previous" ? "previous" : "next";
  const index = searchParams.get("at") ?? undefined;
  const { isPending, isError, isSuccess, data } = useRecipeBookListQuery(
    source,
    index,
    bookSize,
  );

  let nextQueryPos;
  let previousQueryPos;
  if (isSuccess && data) {
    if (source !== "next" || data.length > 0) {
      nextQueryPos = data[data.length - 1]?.id ?? index;
    }
    if (source !== "previous" || data.length > 0) {
      previousQueryPos = data[0]?.id ?? index;
    }
  }

  const {
    data: nextBookPage,
    isSuccess: nextBookPageSuccess,
    isPending: nextBookPagePending,
    isEnabled: nextBookPageEnabled,
  } = useRecipeBookListQuery("next", nextQueryPos, bookSize, !!nextQueryPos);
  const {
    data: previousBookPage,
    isSuccess: previousBookPageSuccess,
    isPending: previousBookPagePending,
    isEnabled: previousBookPageEnabled,
  } = useRecipeBookListQuery(
    "previous",
    previousQueryPos,
    bookSize,
    !!previousQueryPos,
  );

  const tablePaperRef = useRef<HTMLDivElement>(null);
  const firstMountRef = useRef<boolean>(true);
  useEffect(() => {
    if (firstMountRef.current) {
      if (isSuccess) {
        firstMountRef.current = false;
      }
      return;
    }
    if (tablePaperRef.current) {
      tablePaperRef.current.scrollIntoView({
        behavior: "smooth",
      });
    }
  }, [source, index, isSuccess]);

  if (isError || (isSuccess && !data)) {
    return <LoadingFailedAlert />;
  }

  return (
    <>
      <PageHeader title="Recipe Books" />
      <BookTable
        ref={tablePaperRef}
        pending={isPending}
        books={data}
        previousLoading={previousBookPagePending && previousBookPageEnabled}
        previous={
          previousBookPageEnabled &&
          previousBookPageSuccess &&
          previousBookPage &&
          previousBookPage.length > 0
            ? previousQueryPos
            : undefined
        }
        nextLoading={nextBookPageEnabled && nextBookPagePending}
        next={
          nextBookPageEnabled &&
          nextBookPageSuccess &&
          nextBookPage &&
          nextBookPage.length > 0
            ? nextQueryPos
            : undefined
        }
      />
    </>
  );
}
