import { LoadingFailedAlert } from "../../../core/components/loading-failed-alert/loading-failed-alert.component";
import { LoadingIndicator } from "../../../core/components/loading-indicator/loading-indicator.component";
import { RecipeBookNotFoundBanner } from "../../components/recipe-book-not-found-banner/recipe-book-not-found-banner.component";
import { RecipeTable } from "../../components/recipe-table/recipe-table.component";
import { useGetRecipeBookById } from "../../hooks/useGetRecipeBookById.hook";
import { useGetRecipesInBookQuery } from "../../hooks/useGetRecipesInBookQuery.hook";
import { useRecipeClientStateContext } from "../../hooks/useRecipeClientStateContext.hook";
import { useEffect, useRef, useState } from "react";
import { PageHeader } from "../../../core/components/page-header/page-header.component";
import { BookMenuButton } from "../../components/book-menu-button/book-menu-button.component";
import { useActiveUserKey } from "../../../auth/hooks/useActiveUser.hook";
import { SharedRecipeBookIndicator } from "../../components/shared-recipe-book-indicator/shared-recipe-book-indicator.component";
import { BrowserTitle } from "../../../core/components/browser-title/browser-title.component";
import { useSearchRecipesInBook } from "../../hooks/useSearchRecipesInBook.hook";
import type { IRecipeListEntryJsonResponse } from "../../services/recipe-types";

export function ViewRecipeBookPageBody({
  bookId,
  source,
  at,
}: {
  bookId: string;
  source: RecipeListCursorType;
  at?: string;
}) {
  const selectedSize = useRecipeClientStateContext(
    (state) => state.recipeListPageSize,
  );
  const currentUser = useActiveUserKey();
  const [searchValue, setSearchValue] = useState<string>("");
  const bookQuery = useGetRecipeBookById(bookId);

  // Select UI mode based on if the
  // search input box is set.
  const uiMode =
    searchValue && searchValue.trim()
      ? ComponentDataMode.recipeSearch
      : ComponentDataMode.recipeList;

  const recipeListQuery = useGetRecipesInBookQuery(
    bookId,
    source,
    at,
    selectedSize,
    uiMode === ComponentDataMode.recipeList,
  );
  const searchQuery = useSearchRecipesInBook({
    disable: uiMode !== ComponentDataMode.recipeSearch,
    searchString: searchValue,
    limit: selectedSize,
    bookId: bookId,
  });

  const listPageEdgesQuery = useRecipeListPageCursors({
    enable:
      uiMode === ComponentDataMode.recipeList && recipeListQuery.isSuccess,
    selectedSize: selectedSize,
    bookId: bookId,
    source,
    at,
    recipeData: recipeListQuery.data ?? undefined,
  });

  const tablePaperRef = useRef<HTMLDivElement>(null);
  const firstMountRef = useRef<boolean>(true);

  const [componentData, setComponentData] = useState<
    IRecipeListEntryJsonResponse[] | undefined
  >(undefined);

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
  }, [bookId, source, at]);

  // book loading failed
  if (bookQuery.error) {
    return <LoadingFailedAlert />;
  }
  // book is loading
  else if (bookQuery.isPending) {
    return <LoadingIndicator />;
  }
  // book was not found
  else if (!bookQuery.data) {
    return <RecipeBookNotFoundBanner />;
  }

  let nextListAt, prevListAt;
  let nextLoading = false;
  let previousLoading = false;
  let showPendingUi = false;

  switch (uiMode) {
    case ComponentDataMode.recipeList:
      showPendingUi = recipeListQuery.isPending;

      if (
        recipeListQuery.isSuccess &&
        (recipeListQuery.data ?? undefined) !== componentData
      ) {
        setComponentData(recipeListQuery.data ?? undefined);
      }

      nextLoading = listPageEdgesQuery.nextPending;
      previousLoading = listPageEdgesQuery.previousPending;
      nextListAt = listPageEdgesQuery.nextQueryPos;
      prevListAt = listPageEdgesQuery.previousQueryPos;

      break;
    case ComponentDataMode.recipeSearch:
      showPendingUi = searchQuery.isPending;
      if (
        searchQuery.isSuccess &&
        (searchQuery.data ?? undefined) !== componentData
      ) {
        setComponentData(searchQuery.data ?? undefined);
      }
      break;
  }

  const book = bookQuery.data;
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
      <RecipeTable
        ref={tablePaperRef}
        pending={showPendingUi}
        hideHeader={!componentData || componentData.length < 1}
        recipes={componentData}
        nextLoading={nextLoading}
        mayEdit={book.mayEdit ?? false}
        next={nextListAt}
        previousLoading={previousLoading}
        previous={prevListAt}
        showSearch
        searchValue={searchValue}
        searchOnChange={setSearchValue}
      />
    </>
  );
}

/**
 * Component data mode of the component
 */
const ComponentDataMode = {
  /** use the list recipes endpoint */
  recipeList: 1,
  /** use the search recipe endpoint */
  recipeSearch: 2,
};

/**
 * the cursor type. Encodes if the user went forward or backwards.
 */
type RecipeListCursorType = "next" | "previous";

function useRecipeListPageCursors({
  enable,
  selectedSize,
  bookId,
  source,
  at,
  recipeData,
}: {
  enable: boolean;
  selectedSize: number;
  bookId: string;
  source: RecipeListCursorType;
  at: string | undefined;
  recipeData: IRecipeListEntryJsonResponse[] | undefined;
}): IUseRecipeListPageCursors {
  let nextQueryPos;
  let previousQueryPos;
  if (recipeData && enable) {
    if (source !== "next" || recipeData.length > 0) {
      nextQueryPos = recipeData[recipeData.length - 1]?.recipeKey ?? at;
    }
    if (source !== "previous" || recipeData.length > 0) {
      previousQueryPos = recipeData[0]?.recipeKey ?? at;
    }
  }

  const {
    data: nextRecipe,
    isSuccess: nextRecipeSuccess,
    isPending: nextRecipePending,
  } = useGetRecipesInBookQuery(
    bookId,
    "next",
    nextQueryPos,
    selectedSize,
    !!nextQueryPos && enable,
  );
  const {
    data: previousRecipeData,
    isSuccess: previousRecipeSuccess,
    isPending: previousRecipePending,
  } = useGetRecipesInBookQuery(
    bookId,
    "previous",
    previousQueryPos,
    selectedSize,
    !!previousQueryPos && enable,
  );

  const hasNext = !!(nextRecipeSuccess && nextRecipe && nextRecipe.length > 0);
  const hasPrevious = !!(
    previousRecipeSuccess &&
    previousRecipeData &&
    previousRecipeData.length > 0
  );
  return {
    previousPending: enable && previousRecipePending,
    previousSuccess: previousRecipeSuccess,
    hasPrevious: hasPrevious,
    previousQueryPos: hasPrevious ? previousQueryPos : undefined,

    nextPending: enable && nextRecipePending,
    nextSuccess: nextRecipeSuccess,
    hasNext,
    nextQueryPos: hasNext ? nextQueryPos : undefined,
  };
}

interface IUseRecipeListPageCursors {
  previousSuccess: boolean;
  previousPending: boolean;
  hasPrevious: boolean;
  nextSuccess: boolean;
  nextPending: boolean;
  hasNext: boolean;
  nextQueryPos?: string;
  previousQueryPos?: string;
}
