import { useRef, useEffect } from "react";
import { useActiveUserKey } from "../../../auth/hooks/useActiveUser.hook";
import { BrowserTitle } from "../../../core/components/browser-title/browser-title.component";
import { PageHeader } from "../../../core/components/page-header/page-header.component";
import { BookMenuButton } from "../../components/book-menu-button/book-menu-button.component";
import { SharedRecipeBookIndicator } from "../../components/shared-recipe-book-indicator/shared-recipe-book-indicator.component";
import type {
  IRecipeBookModel,
  IRecipeListEntryJsonResponse,
} from "../../services/recipe-types";
import { useRecipeClientStateContext } from "../../hooks/useRecipeClientStateContext.hook";
import LinearProgress from "@mui/material/LinearProgress";
import Paper from "@mui/material/Paper";
import Table from "@mui/material/Table";
import TableContainer from "@mui/material/TableContainer";
import { RecipeTableBody } from "../../components/recipe-table/recipe-table-body.component";
import { RecipeTableHeader } from "../../components/recipe-table/recipe-table-header.component";
import {
  TablePageControls,
  type ITablePageDirectionControlsProps,
} from "../../components/table-page-controls/table-page-controls.component";
import type { IRecipeTableSearchProps } from "../../components/recipe-table/recipe-table-title.component";

/**
 * Component for rendering a list of recipes scoped to a book. Does not assume
 * the source. The data could be from a search or just a linear scan.
 */
export function BrowseRecipesByBook({
  book,
  recipes,
  pending,
  nextPending,
  previousPending,
  searchValue,
  showSearch,
  setSearchValue,
  goNext,
  goPrevious,
  jumpToEndPending,
  jumpToStartPending,
  jumpToEnd,
  jumpToStart,
  focusSearch,
}: IBrowseRecipesByBookParams) {
  const currentUser = useActiveUserKey();
  const tablePaperRef = useRef<HTMLDivElement>(null);
  const firstMountRef = useRef<boolean>(true);
  const pageSize = useRecipeClientStateContext((s) => s.recipeListPageSize);
  const setPageSize = useRecipeClientStateContext(
    (s) => s.setRecipeListPageSize,
  );
  useEffect(() => {
    if (firstMountRef.current) {
      firstMountRef.current = false;
      return;
    }
    if (tablePaperRef.current) {
      tablePaperRef.current.scrollIntoView({
        behavior: "smooth",
      });
    }
  }, [book]);

  return (
    <>
      <BrowserTitle title={book.name ? book.name : "View Book"} />
      <PageHeader
        title={book.name}
        subTitle={book.shortDescription}
        titleComponent={<SharedRecipeBookIndicator bookId={book.id} />}
        sideComponent={
          <BookMenuButton
            bookId={book.id}
            mayEdit={book.mayEdit}
            mayShare={book.mayShare}
            mayLeave={book.ownerId !== currentUser}
            shareKey={book.shareKey}
            bookName={book.name}
          />
        }
      />
      <Paper ref={tablePaperRef}>
        <TableContainer>
          <Table>
            <RecipeTableHeader
              hideHeader={!recipes || recipes.length == 0}
              showSearch={showSearch}
              setSearchValue={setSearchValue}
              searchValue={searchValue}
              focusSearch={focusSearch}
            />
            <RecipeTableBody
              recipes={recipes ?? undefined}
              mayEdit={book.mayEdit ?? false}
            />
          </Table>
        </TableContainer>
        <TablePageControls
          goNext={goNext}
          goPrevious={goPrevious}
          nextPending={nextPending}
          previousPending={previousPending}
          jumpToEndPending={jumpToEndPending}
          jumpToStartPending={jumpToStartPending}
          jumpToEnd={jumpToEnd}
          jumpToStart={jumpToStart}
          setPageSize={setPageSize}
          pageSize={pageSize}
        />
        {pending && <LinearProgress aria-label="Loading…" />}
      </Paper>
    </>
  );
}

/** params for `BrowseRecipesByBook` component */
export interface IBrowseRecipesByBookParams
  extends ITablePageDirectionControlsProps, IRecipeTableSearchProps {
  /** the book being browsed */
  book: IRecipeBookModel;
  /** the recipes  */
  recipes: IRecipeListEntryJsonResponse[] | null | undefined;
  /** if the recipes load is pending */
  pending: boolean;
}
