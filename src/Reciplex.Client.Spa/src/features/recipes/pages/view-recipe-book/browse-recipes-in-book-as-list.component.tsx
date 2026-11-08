import { LoadingFailedAlert } from "../../../core/components/loading-failed-alert/loading-failed-alert.component";
import type { IRecipeBookModel } from "../../services/recipe-types";
import { BrowseRecipesByBook } from "./browse-recipes-by-book.component";
import { useBrowseRecipesInBookAsList } from "./useBrowseRecipesInBookAsList.hook";
import type { IRecipeTableSearchProps } from "../../components/recipe-table/recipe-table-title.component";

/** browses recipes in a book as a list */
export function BrowseRecipesInBookAsList({
  book,
  setSearchValue,
  showSearch,
  focusSearch,
}: IBrowseRecipesInBookAsListProps) {
  const hook = useBrowseRecipesInBookAsList({
    enable: true,
    bookId: book.id,
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
      showSearch={showSearch}
      searchValue={""}
      setSearchValue={setSearchValue}
      goNext={hook.goNext}
      jumpToEnd={hook.jumpToEnd}
      goPrevious={hook.goPrevious}
      jumpToStart={hook.jumpToStart}
      focusSearch={focusSearch}
    />
  );
}

/** properties for `BrowseRecipesInBookAsList` */
export interface IBrowseRecipesInBookAsListProps extends Omit<
  IRecipeTableSearchProps,
  "searchValue"
> {
  /** the book being browsed */
  book: IRecipeBookModel;
}
