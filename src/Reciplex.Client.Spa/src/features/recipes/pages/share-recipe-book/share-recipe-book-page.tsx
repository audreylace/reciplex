import { useParams } from "react-router";
import { RecipeBookNotFoundBanner } from "../../components/recipe-book-not-found-banner/recipe-book-not-found-banner.component";
import { ShareRecipeBookPageBody } from "./share-recipe-book-page-body.component";
import { BrowserTitle } from "../../../core/components/browser-title/browser-title.component";

/** page for managing recipe book share link */
export function ShareRecipeBookPage() {
  const { bookId } = useParams<{
    bookId: string;
  }>();

  if (!bookId) {
    return <RecipeBookNotFoundBanner />;
  }
  return (
    <>
      <BrowserTitle title="Share Book" />
      <ShareRecipeBookPageBody bookId={bookId} />;
    </>
  );
}
