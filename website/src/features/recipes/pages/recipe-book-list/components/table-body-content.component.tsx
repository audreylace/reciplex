import type { IGetRecipeBooksResult } from "../../../../../services/recipe-store";
import { useBookData } from "../hooks/useBookData.hook";
import { BookListUi } from "./book-list-ui.component";
import { CreateFirstBookUI } from "./create-first-book-ui.component";
import { EmptyPageUi } from "./empty-page-ui.component";
import { LoadFailedUi } from "./load-failed-ui.component";
import { OfflineUi } from "./offline-ui.component";
import { SkeletonUi } from "./skeleton-ui.component";

export function TableBodyContent({
  isPaused,
  isPending,
  data,
}: {
  isPaused?: boolean;
  isPending?: boolean;
  data: IGetRecipeBooksResult | undefined;
}) {
  const bookData = useBookData(data);

  if (isPaused) {
    return <OfflineUi />;
  }

  if (isPending) {
    return <SkeletonUi />;
  }

  if (data) {
    if (bookData) {
      return <BookListUi books={bookData} />;
    }

    if (data?.nextCursor || data?.previousCursor) {
      return <EmptyPageUi />;
    }

    return <CreateFirstBookUI />;
  }

  return <LoadFailedUi />;
}
