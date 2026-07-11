import { useParams } from "react-router";
import { RecipeBookNotFoundBanner } from "../../components/recipe-book-not-found-banner/recipe-book-not-found-banner.component";
import { EditRecipeBookPageBody } from "./edit-recipe-book-page-body.component";
import { BrowserTitle } from "../../../core/components/browser-title/browser-title.component";

/**
 * page for editing a recipe book
 */
export function EditRecipeBookPage() {
  const { bookId } = useParams<{ bookId: string }>();

  if (!bookId) {
    return <RecipeBookNotFoundBanner />;
  }

  return (
    <>
      <BrowserTitle title="Editing Book" />
      <EditRecipeBookPageBody bookId={bookId} />
    </>
  );
}
