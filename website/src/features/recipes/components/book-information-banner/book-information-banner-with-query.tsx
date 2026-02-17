import { useGetRecipeBookById } from "../../hooks/useGetRecipeBookById.hook";
import { BookInformationBanner } from "./book-information-banner";

/** Wraps `BookInformationBanner` populating the banner using `useGetRecipeBookById` */
export function BookInformationBannerWithQuery({ bookId }: { bookId: string }) {
  const query = useGetRecipeBookById(bookId);

  if (query.isError || (query.isSuccess && !query.data)) {
    return null;
  }

  return (
    <BookInformationBanner
      name={query.data?.name}
      shortDescription={query.data?.shortDescription}
    />
  );
}
