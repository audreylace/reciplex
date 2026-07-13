import { useParams } from "react-router";
import { ManageUserAccessBody } from "./manage-user-access-body.component";
import { RecipeBookNotFoundBanner } from "../../components/recipe-book-not-found-banner/recipe-book-not-found-banner.component";
import { BrowserTitle } from "../../../core/components/browser-title/browser-title.component";

/** page for managing who has access to a recipe book */
export function ManageUserAccessPage() {
  const { bookId } = useParams<{
    bookId: string;
  }>();

  if (!bookId) {
    return <RecipeBookNotFoundBanner />;
  }
  return (
    <>
      <BrowserTitle title="Manage Book Users" />
      <ManageUserAccessBody bookId={bookId} />
    </>
  );
}
