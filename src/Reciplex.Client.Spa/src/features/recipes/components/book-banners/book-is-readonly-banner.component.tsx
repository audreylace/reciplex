import { makeViewRecipeBookPath } from "../../route-utils";
import { ErrorBanner } from "../../../core/components/banner/banner.component";

/**
 * Read-only banner for a recipe book
 */
export function BookIsReadonlyBanner({ bookId }: BookIsReadonlyBannerProps) {
  return (
    <ErrorBanner
      title="Book is read-only"
      message="You may not edit this recipe book"
      icon="bi bi-ban"
      to={makeViewRecipeBookPath(bookId)}
      buttonCaption="View Book"
    />
  );
}

/** props for `BookIsReadonlyBanner` */
export interface BookIsReadonlyBannerProps {
  /** the id of the read-only book */
  bookId: string;
}
