import { useParams } from "react-router";
import { useGetRecipeBookById } from "../../hooks/useGetRecipeBookById.hook";
import { BadPathBanner } from "../../components/bad-path-banner/bad-path-banner.component";
import { BookInformationHeader } from "../../components/book-information-header/book-information-header.component";

import { ViewRecipeBookPageBody } from "./view-recipe-book-page-body.component";

/**
 * Entry point for viewing a recipe book
 */
export function ViewRecipeBookPage() {
  const { bookId } = useParams<{
    bookId: string;
  }>();
  const bookQuery = useGetRecipeBookById(bookId);
  return (
    <main className="pageMain">
      {!bookId && <BadPathBanner />}
      {bookId && (
        <>
          <BookInformationHeader
            name={bookQuery.data?.name}
            shortDescription={bookQuery.data?.shortDescription}
          />
          <ViewRecipeBookPageBody
            loadingStatus={bookQuery.status}
            bookData={bookQuery.data}
            fetchStatus={bookQuery.fetchStatus}
          />
        </>
      )}
    </main>
  );
}
