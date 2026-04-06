import { useParams } from "react-router";
import { DeleteRecipeBookPageBody } from "./delete-recipe-book-page-body.component";
import { RecipeBookNotFoundBanner } from "../../components/book-banners/recipe-book-not-found-banner.component";

/** page for deleting a recipe book */
export function DeleteRecipeBookPage() {
  const { bookId } = useParams<{ bookId: string }>();

  if (!bookId) {
    return <RecipeBookNotFoundBanner />;
  }

  return <DeleteRecipeBookPageBody key={bookId} bookKey={bookId} />;
}
