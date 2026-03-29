import styles from "./recipe-book-list-page.module.css";
import { useNavigate, useSearchParams } from "react-router";
import {
  makeViewRecipeBookPath,
  type BookListNavigationAction,
} from "../../route-utils";
import { useRecipeBookListQuery } from "../../hooks/useRecipeBookListQuery.hook";
import {
  PagedTable,
  type IPagedTableRow,
} from "../../components/paged-table/paged-table.component";
import { CreateRecipeBookButton } from "./create-recipe-book-button.component";
import { RetryBannerComponent } from "../../../core/components/banner/retry-banner.component";
import { InformationBanner } from "../../../core/components/banner/banner.component";
import { OfflineBanner } from "../../../core/components/banner/offline-banner.component";
import { useMemo } from "preact/hooks";
import type { IGetRecipeBooksResult } from "../../services/recipe-types";
import {
  BookPageSizeSelector,
  useBookSize,
} from "./book-page-size-selector.component";

/**
 * Entry point for recipe book list page component
 */
export function RecipeBookListPage() {
  const bookSize = useBookSize();
  const searchParams = useSearchParams()[0];
  const source = searchParams.get("source") as
    | BookListNavigationAction
    | undefined;
  const index = searchParams.get("at") as string | undefined;
  const query = useRecipeBookListQuery(source, index, bookSize);

  let body;
  switch (query.status) {
    default:
      body = <RetryBannerComponent />;
      break;
    case "pending":
      if (query.fetchStatus === "paused") {
        body = <OfflineBanner />;
      } else {
        body = <InformationBanner title="loading" />;
      }
      break;
    case "success":
      body = <BookTable data={query.data} />;
      break;
  }

  return (
    <main className="pageMain">
      <div className={styles.topLevelHeaderWrapper}>
        <h2 className={styles.topLevelHeader}>Recipes Books</h2>
        <div className={styles.topLevelHeaderSpacer}></div>
        <CreateRecipeBookButton />
      </div>
      {body}
    </main>
  );
}

function BookTable({ data }: { data: IGetRecipeBooksResult }) {
  const navigate = useNavigate();
  const setSearchParams = useSearchParams()[1];

  const rows = useMemo(() => {
    if (!data) {
      return [];
    }

    return data.books.map((b) => {
      return {
        details: b.shortDescription,
        title: b.name,
        id: b.id,
      } as IPagedTableRow;
    });
  }, [data]);

  const nextCursor = data.nextCursor;
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

  const previousCursor = data.previousCursor;
  const navigatePrevious = previousCursor
    ? () =>
        setSearchParams({
          source: "previous",
          at: previousCursor,
        })
    : undefined;
  const navigateStart = previousCursor ? () => setSearchParams() : undefined;

  return (
    <PagedTable
      title="Books"
      message="No Books"
      rows={rows}
      onSelect={(r) => navigate(makeViewRecipeBookPath(r.id))}
      navigateStart={navigateStart}
      navigateNext={navigateNext}
      navigateBack={navigatePrevious}
      navigateEnd={navigateEnd}
      additionalPaginationControls={<BookPageSizeSelector />}
    />
  );
}
