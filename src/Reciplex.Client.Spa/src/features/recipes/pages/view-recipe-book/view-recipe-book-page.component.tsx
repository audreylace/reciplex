import { useParams, useSearchParams } from "react-router";
import { RecipeBookNotFoundBanner } from "../../components/recipe-book-not-found-banner/recipe-book-not-found-banner.component";
import { ViewRecipeBookPageBody } from "./view-recipe-book-page-body.component";

/**
 * Entry point for viewing a recipe book
 */
export function ViewRecipeBookPage() {
  const { bookId } = useParams<{
    bookId: string;
  }>();
  const searchParams = useSearchParams()[0];

  if (!bookId) {
    return <RecipeBookNotFoundBanner />;
  }
  return (
    <ViewRecipeBookPageBody
      bookId={bookId}
      source={searchParams.get("source") === "previous" ? "previous" : "next"}
      at={searchParams.get("at") ?? undefined}
    />
  );
}
