import { LoadingFailedAlert } from "../../../core/components/loading-failed-alert/loading-failed-alert.component";
import { LoadingIndicator } from "../../../core/components/loading-indicator/loading-indicator.component";
import { RecipeBookNotFoundBanner } from "../../components/recipe-book-not-found-banner/recipe-book-not-found-banner.component";
import { RecipeTable } from "../../components/recipe-table/recipe-table.component";
import { useGetRecipeBookById } from "../../hooks/useGetRecipeBookById.hook";
import { useGetRecipesInBookQuery } from "../../hooks/useGetRecipesInBookQuery.hook";
import { useRecipeClientStateContext } from "../../hooks/useRecipeClientStateContext.hook";
import { useEffect, useRef } from "react";
import { PageHeader } from "../../../core/components/page-header/page-header.component";
import { BookMenuButton } from "../../components/book-menu-button/book-menu-button.component";
import { useActiveUserKey } from "../../../auth/hooks/useActiveUser.hook";
import { SharedRecipeBookIndicator } from "../../components/shared-recipe-book-indicator/shared-recipe-book-indicator.component";

export function ViewRecipeBookPageBody({
  bookId,
  source,
  at,
}: {
  bookId: string;
  source: "next" | "previous";
  at?: string;
}) {
  const selectedSize = useRecipeClientStateContext(
    (state) => state.recipeListPageSize,
  );
  const currentUser = useActiveUserKey();
  const {
    data: book,
    isError: getRecipeBookError,
    isPending: bookPending,
  } = useGetRecipeBookById(bookId);

  const {
    isError: getRecipesError,
    isPending: getRecipesPending,
    data: recipeList,
    isSuccess: recipeListSuccess,
  } = useGetRecipesInBookQuery(bookId, source, at, selectedSize);

  let nextQueryPos;
  let previousQueryPos;
  if (recipeListSuccess && recipeList) {
    if (source !== "next" || recipeList.length > 0) {
      nextQueryPos = recipeList[recipeList.length - 1]?.recipeKey ?? at;
    }
    if (source !== "previous" || recipeList.length > 0) {
      previousQueryPos = recipeList[0]?.recipeKey ?? at;
    }
  }

  const {
    data: nextRecipe,
    isSuccess: nextRecipeSuccess,
    isPending: nextRecipePending,
    isEnabled: nextRecipeEnabled,
  } = useGetRecipesInBookQuery(
    bookId,
    "next",
    nextQueryPos,
    selectedSize,
    !!nextQueryPos,
  );
  const {
    data: previousRecipe,
    isSuccess: previousRecipeSuccess,
    isPending: previousRecipePending,
    isEnabled: previousRecipeEnabled,
  } = useGetRecipesInBookQuery(
    bookId,
    "previous",
    previousQueryPos,
    selectedSize,
    !!previousQueryPos,
  );

  const tablePaperRef = useRef<HTMLDivElement>(null);
  const firstMountRef = useRef<boolean>(true);
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

  if (getRecipesError || getRecipeBookError) {
    return <LoadingFailedAlert />;
  }

  if (bookPending) {
    return <LoadingIndicator />;
  }

  if (
    !book ||
    // show loading table once the book loads in
    (!getRecipesPending && !recipeList)
  ) {
    return <RecipeBookNotFoundBanner />;
  }

  return (
    <>
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
          />
        }
      />
      <RecipeTable
        ref={tablePaperRef}
        pending={getRecipesPending}
        recipes={recipeList ?? undefined}
        nextLoading={nextRecipeEnabled && nextRecipePending}
        mayEdit={book.mayEdit ?? false}
        next={
          nextRecipeEnabled &&
          nextRecipeSuccess &&
          (nextRecipe?.length ?? 0) > 0
            ? nextQueryPos
            : undefined
        }
        previousLoading={previousRecipeEnabled && previousRecipePending}
        previous={
          previousRecipeEnabled &&
          previousRecipeSuccess &&
          (previousRecipe?.length ?? 0) > 0
            ? previousQueryPos
            : undefined
        }
      />
    </>
  );
}
