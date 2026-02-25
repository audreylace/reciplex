import { useGetRecipeBookById } from "../../hooks/useGetRecipeBookById.hook";
import { BookInformationHeader } from "./book-information-header.component";

/** Wraps `BookInformationHeader` populating it via `useGetRecipeBookById` */
export function BookInformationHeaderWithQuery({ bookId }: { bookId: string }) {
  const query = useGetRecipeBookById(bookId);

  if (query.isError || (query.isSuccess && !query.data)) {
    return null;
  }

  return (
    <BookInformationHeader
      name={query.data?.name}
      shortDescription={query.data?.shortDescription}
    />
  );
}
