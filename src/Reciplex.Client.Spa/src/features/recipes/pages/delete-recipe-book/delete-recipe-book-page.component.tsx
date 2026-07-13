import { useParams } from "react-router";
import { DeleteRecipeBookPageBody } from "./delete-recipe-book-page-body.component";
import { RecipeBookNotFoundBanner } from "../../components/recipe-book-not-found-banner/recipe-book-not-found-banner.component";
import { BrowserTitle } from "../../../core/components/browser-title/browser-title.component";

/** page for deleting a recipe book */
export function DeleteRecipeBookPage() {
  const { bookId } = useParams<{ bookId: string }>();

  if (!bookId) {
    return <RecipeBookNotFoundBanner />;
  }

  return (
    <>
      <BrowserTitle title="Deleting Book" />
      <DeleteRecipeBookPageBody key={bookId} bookKey={bookId} />
    </>
  );
}
