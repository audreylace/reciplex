import { useParams } from "react-router";
import { ManageUserAccessBody } from "./manage-user-access-body.component";
import { RecipeBookNotFoundBanner } from "../../components/recipe-book-not-found-banner/recipe-book-not-found-banner.component";

/** page for managing who has access to a recipe book */
export function ManageUserAccessPage() {
  const { bookId } = useParams<{
    bookId: string;
  }>();

  if (!bookId) {
    return <RecipeBookNotFoundBanner />;
  }
  return <ManageUserAccessBody bookId={bookId} />;
}
