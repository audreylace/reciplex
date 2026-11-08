import { useParams } from "react-router";
import { RecipeBookNotFoundBanner } from "../../components/recipe-book-not-found-banner/recipe-book-not-found-banner.component";
import { useGetRecipeBookById } from "../../hooks/useGetRecipeBookById.hook";
import { LoadingFailedAlert } from "../../../core/components/loading-failed-alert/loading-failed-alert.component";
import { LoadingIndicator } from "../../../core/components/loading-indicator/loading-indicator.component";
import { BrowseRecipesInBookAsList } from "./browse-recipes-in-book-as-list.component";
import {
  useSearchString,
  useSetSearchQuery,
} from "./useBrowseRecipesInBookViaSearch.hook";
import { BrowseRecipesInBookViaSearch } from "./browse-recipes-via-search.component";
import { useEffect, useRef, useState } from "react";

/**
 * Entry point for viewing a recipe book
 */
export function ViewRecipeBookPage() {
  const { bookId } = useParams<{
    bookId: string;
  }>();
  const setSearchQuery = useSetSearchQuery();
  const bookQuery = useGetRecipeBookById(bookId);
  const searchString = useSearchString();
  const [focusSearch, setFocusSearch] = useState<boolean>(false);
  const mountDoneRef = useRef(false);

  useEffect(() => {
    if (bookQuery.isSuccess && !mountDoneRef.current) {
      mountDoneRef.current = true;
      setFocusSearch(true);
    }
  }, [bookQuery.isSuccess]);

  if (!bookId || !bookQuery.data) {
    return <RecipeBookNotFoundBanner />;
  }

  // book loading failed
  if (bookQuery.error) {
    return <LoadingFailedAlert />;
  }
  // book is loading
  else if (bookQuery.isPending) {
    return <LoadingIndicator />;
  }

  if (searchString) {
    return (
      <BrowseRecipesInBookViaSearch
        book={bookQuery.data}
        setSearchValue={setSearchQuery}
        searchValue={searchString}
        focusSearch={focusSearch}
      />
    );
  }

  return (
    <BrowseRecipesInBookAsList
      book={bookQuery.data}
      showSearch
      setSearchValue={setSearchQuery}
      focusSearch={focusSearch}
    />
  );
}
