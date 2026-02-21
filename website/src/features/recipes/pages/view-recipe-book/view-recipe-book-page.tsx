import { useParams } from "react-router";
import { useGetRecipeBookById } from "../../hooks/useGetRecipeBookById.hook";
import { BadPathBanner } from "../../components/bad-path-banner/bad-path-banner.component";
import { BookInformationBanner } from "../../components/book-information-banner/book-information-banner";

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
          <BookInformationBanner
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
