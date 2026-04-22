import { useParams } from "react-router";
import { RecipeBookNotFoundBanner } from "../../components/recipe-book-not-found-banner/recipe-book-not-found-banner.component";
import { EditRecipeBookPageBody } from "./edit-recipe-book-page-body.component";

/**
 * page for editing a recipe book
 */
export function EditRecipeBookPage() {
  const { bookId } = useParams<{ bookId: string }>();

  if (!bookId) {
    return <RecipeBookNotFoundBanner />;
  }

  return <EditRecipeBookPageBody bookId={bookId} />;
}
