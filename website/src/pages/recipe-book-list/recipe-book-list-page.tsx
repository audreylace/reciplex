import { useContext, useLayoutEffect, useState } from "preact/hooks";
import {
  CursorTypes,
  type IGetRecipeBooksArgs,
} from "../../services/recipe-store";
import { Link, useNavigate, useParams } from "react-router";
import styles from "./recipe-book-list-page.module.css";
import { useQuery } from "@tanstack/react-query";
import {
  BookListNavigationAction,
  makeCreateRecipeBookPath,
} from "../../features/recipes/route-utils";
import { RecipeStore } from "../../features/recipes/hooks/useRecipeStoreContext.hook";
import { useSetTitle } from "../../layouts/default/default-layout.state";
import { TableMessage } from "./components/table-message/table-message.component";
import { TableRowsSkeleton } from "./components/table-rows-skeleton/table-rows-skeleton";
import { PaginationBar } from "./components/pagination-bar/pagination-bar.component";
import { TableRow } from "./components/table-row/table-row.component";
import { TableHeader } from "./components/table-header/table-header.component";

/**
 * Route parameters
 */
type RouteParams = {
  /** the source of the route navigation */
  source: BookListNavigationAction;
  /** the navigation index */
  index: string;
};

/**
 * Entry point for recipe book list page component
 * @param param0 react props
 * @returns jsx tree for rendering by react
 */
export function RecipeBookListPage({}: {}) {
  const navigate = useNavigate();
  useSetTitle("Recipe Books");
  const { source, index } = useParams<RouteParams>();
  const query = useRecipeBookListQuery(source, index);
  const [fakeLoading, setFakeLoading] = useState(true);

  useLayoutEffect(() => {
    setFakeLoading(true);
    const handle = setTimeout(() => setFakeLoading(false), 200); // reduce flicker on page change
    return () => {
      clearTimeout(handle);
    };
  }, [setFakeLoading, source, index]);

  const data = fakeLoading ? null : query.data;
  const hasPrevious = data?.previousCursor;
  const hasNext = data?.nextCursor;
  const failed = !fakeLoading && query.isError;
  const showPause = !fakeLoading && query.isPaused && !query.isSuccess;
  const showSkeleton =
    fakeLoading || (!failed && query.isPending && !showPause);
  const emptyPage = data && data.page.length <= 0;
  const noBooks = emptyPage && !hasPrevious && !hasNext;

  return (
    <>
      <main className={styles.pageMain}>
        <h2 className={styles.topLevelHeader}>Recipes Books</h2>
        <div className={styles.recipeBookList}>
          <table
            className={styles.recipeBookListTable}
            aria-description="list of recipes books"
          >
            <TableHeader />
            <tbody className={styles.recipeBookListTableBody}>
              {showSkeleton && <TableRowsSkeleton count={20} />}
              {showPause && <TableMessage>Offline</TableMessage>}
              {emptyPage && (
                <TableMessage>No results for this page</TableMessage>
              )}
              {failed && (
                <TableMessage>
                  Retrieving list of recipe books failed.{" "}
                  <a
                    href="#"
                    onClick={(event) => {
                      event.preventDefault();
                      navigate(0);
                    }}
                  >
                    Try again?
                  </a>
                </TableMessage>
              )}
              {noBooks && (
                <TableMessage>
                  <Link to={makeCreateRecipeBookPath()}>
                    Create your first recipe book
                  </Link>
                </TableMessage>
              )}
              {data?.page.map((key) => {
                const book = data.recipeBooks[key];
                if (!book) {
                  return null;
                }
                return <TableRow book={book} key={key} />;
              })}
            </tbody>
          </table>
        </div>
      </main>
      <PaginationBar
        nextCursor={data?.nextCursor?.position}
        previousCursor={data?.previousCursor?.position}
      />
    </>
  );
}

function useRecipeBookListQuery(source?: string, index?: string) {
  const recipeStore = useContext(RecipeStore);
  return useQuery({
    queryKey: ["recipe-book-list", { source, index }],
    queryFn: async () => {
      if (!recipeStore) {
        throw new Error("Require recipe store");
      }
      const args: IGetRecipeBooksArgs = {};
      if (
        source &&
        (source === BookListNavigationAction.next ||
          source === BookListNavigationAction.previous)
      ) {
        args.cursor = {
          position: index,
          type:
            source === BookListNavigationAction.next
              ? CursorTypes.next
              : CursorTypes.previous,
        };
      }

      const result = await recipeStore.getRecipeBooks(args);
      if (!result) {
        throw new Error("Get book API failed");
      }

      return result;
    },
  });
}
