import { useNavigate, useParams, useSearchParams } from "react-router";
import { useGetRecipeBookById } from "../../hooks/useGetRecipeBookById.hook";
import { ApplicationErrorBanner } from "../../../core/components/banner/application-error-banner.component";
import { BookInformationHeader } from "../../components/book-information-header/book-information-header.component";
import { useRecipeForBookQuery } from "../../hooks/useRecipesForBookQuery.hook";
import type {
  IGetRecipesInBookResult,
  IPageRequestCursor,
  IRecipeBookModel,
} from "../../services/recipe-types";
import { FetchingStatusDispatch } from "../../../core/components/fetch-status-dispatch/fetch-status-dispatch.component";
import { OfflineBanner } from "../../../core/components/banner/offline-banner.component";
import { FetchingRecipeBookBanner } from "../../components/book-banners/fetching-recipe-book-banner.component";
import { FetchingRecipeBookFailedBanner } from "../../components/book-banners/fetching-recipe-book-failed-banner.component";
import { RecipeBookMenu } from "./recipe-book-menu.component";
import { RecipeBookNotFoundBanner } from "../../components/book-banners/recipe-book-not-found-banner.component";
import {
  PagedTable,
  type IPagedTableRow,
} from "../../components/paged-table/paged-table.component";
import { useMemo } from "preact/hooks";
import { useRecipeListTableContext } from "./useRecipeListTableContext.hook";
import {
  OptionSelector,
  type IOptionEntry,
} from "../../../core/components/option-selector/option-selector.component";
import { makeViewRecipePath } from "../../route-utils";

/**
 * Entry point for viewing a recipe book
 */
export function ViewRecipeBookPage() {
  const { bookId } = useParams<{
    bookId: string;
  }>();
  const selectedSize = useRecipeListTableContext((state) => state.size);
  const bookQuery = useGetRecipeBookById(bookId);
  const searchParams = useSearchParams()[0];

  const pageCursorType =
    searchParams.get("source") === "previous" ? "previous" : "next";
  const pageCursor: IPageRequestCursor = {
    position: searchParams.get("at") ?? undefined,
    type: pageCursorType,
  };
  const bookListQuery = useRecipeForBookQuery(bookId, pageCursor, selectedSize);

  const bodyData = bookQuery.data;
  let body;
  if (bookQuery.status === "success" && !bodyData) {
    body = <RecipeBookNotFoundBanner />;
  } else if (
    bodyData &&
    bookListQuery.data &&
    bookQuery.status === "success" &&
    bookListQuery.status === "success"
  ) {
    body = (
      <SuccessRender bookData={bodyData} bookListData={bookListQuery.data} />
    );
  } else if (
    bookQuery.status === "pending" ||
    bookListQuery.status === "pending"
  ) {
    let status = bookQuery.fetchStatus;
    if (bookListQuery.status === "pending") {
      status = bookQuery.fetchStatus;
    }

    body = (
      <FetchingStatusDispatch
        fetchStatus={status}
        fetching={<FetchingRecipeBookBanner />}
        idle={<FetchingRecipeBookBanner />}
        paused={<OfflineBanner />}
      />
    );
  } else {
    body = <FetchingRecipeBookFailedBanner />;
  }

  return (
    <main className="pageMain">
      {!bookId && <ApplicationErrorBanner />}
      {bookId && (
        <>
          <BookInformationHeader
            name={bookQuery.data?.name}
            shortDescription={bookQuery.data?.shortDescription}
          />
          {body}
        </>
      )}
    </main>
  );
}

/** predefined app page sizes */
const pageSize: IOptionEntryWithSize[] = [
  { size: 10, name: "10", key: "10" },
  { size: 20, name: "20", key: "20" },
  { size: 50, name: "50", key: "50" },
  { size: 100, name: "100", key: "100" },
];

interface IOptionEntryWithSize extends IOptionEntry {
  size: number;
}

function SuccessRender({
  bookListData,
  bookData,
}: {
  bookListData: IGetRecipesInBookResult;
  bookData: IRecipeBookModel;
}) {
  const navigate = useNavigate();
  const selectedSize = useRecipeListTableContext((state) => state.size);
  const setSearchParams = useSearchParams()[1];
  const updateSize = useRecipeListTableContext((state) => state.updateSize);
  const selectedValue = pageSize.find((entry) => entry.size === selectedSize);
  const nextCursor = bookListData.nextCursor;
  const navigateNext = nextCursor
    ? () =>
        setSearchParams({
          source: "next",
          at: nextCursor,
        })
    : undefined;
  const navigateEnd = nextCursor
    ? () =>
        setSearchParams({
          source: "previous",
        })
    : undefined;

  const previousCursor = bookListData.previousCursor;
  const navigatePrevious = previousCursor
    ? () =>
        setSearchParams({
          source: "previous",
          at: previousCursor,
        })
    : undefined;
  const navigateStart = previousCursor ? () => setSearchParams() : undefined;

  const rows = useMemo(() => {
    return bookListData.recipes.map((r) => {
      return {
        details: r.shortDescription,
        title: r.name,
        id: r.id,
      } as IPagedTableRow;
    });
  }, [bookListData]);

  return (
    <>
      <RecipeBookMenu
        bookId={bookData.id}
        name={bookData.name}
        shortDescription={bookData.shortDescription}
        mayEdit={bookData.mayEdit}
        mayDelete={bookData.mayDelete}
      />
      <PagedTable
        title="Recipes"
        message="No Recipes"
        rows={rows}
        onSelect={(r) => {
          navigate(makeViewRecipePath(bookData.id, r.id));
        }}
        navigateStart={navigateStart}
        navigateNext={navigateNext}
        navigateBack={navigatePrevious}
        navigateEnd={navigateEnd}
        additionalPaginationControls={
          <OptionSelector
            options={pageSize}
            value={selectedValue ?? pageSize[0]}
            onChange={(entry) => updateSize(entry.size)}
          />
        }
      />
    </>
  );
}
