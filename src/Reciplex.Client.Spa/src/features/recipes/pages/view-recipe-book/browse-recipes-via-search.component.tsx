import { LoadingFailedAlert } from "../../../core/components/loading-failed-alert/loading-failed-alert.component";
import type { IRecipeBookModel } from "../../services/recipe-types";
import { BrowseRecipesByBook } from "./browse-recipes-by-book.component";
import type { IRecipeTableSearchProps } from "../../components/recipe-table/recipe-table-title.component";
import { useBrowseRecipesInBookViaSearch } from "./useBrowseRecipesInBookViaSearch.hook";

/** browses recipes in a book via user supplied search*/
export function BrowseRecipesInBookViaSearch({
  book,
  searchValue,
  setSearchValue,
  focusSearch,
}: IBrowseRecipesInBookViaSearch) {
  const hook = useBrowseRecipesInBookViaSearch({
    enable: true,
    bookId: book.id,
    searchValue,
  });

  if (hook.dataError) {
    return <LoadingFailedAlert />;
  }

  return (
    <BrowseRecipesByBook
      book={book}
      recipes={hook.data}
      pending={hook.dataPending}
      nextPending={hook.nextPending}
      previousPending={hook.previousPending}
      jumpToEndPending={hook.previousPending}
      jumpToStartPending={hook.nextPending}
      showSearch
      focusSearch={focusSearch}
      searchValue={searchValue}
      setSearchValue={setSearchValue}
      goNext={hook.goNext}
      jumpToEnd={hook.jumpToEnd}
      goPrevious={hook.goPrevious}
      jumpToStart={hook.jumpToStart}
    />
  );
}

/** properties for `BrowseRecipesInBookViaSearch` */
export interface IBrowseRecipesInBookViaSearch extends Omit<
  IRecipeTableSearchProps,
  "showSearch"
> {
  /** the book being browsed */
  book: IRecipeBookModel;
}
