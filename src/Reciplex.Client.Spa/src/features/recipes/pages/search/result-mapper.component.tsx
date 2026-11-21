import { BookResult } from "./book-result.component";
import { RecipeResult } from "./recipe-result.component";
import type { TaggedSearchPageDataUnionType } from "./useSearchPageData.hook";

/**
 * Maps a single result to its correct UI component
 */
export function ResultMapper({ data: d }: IResultMapperProps) {
  if (d.kind === "recipe") {
    return (
      <RecipeResult
        key={"recipe_" + d.recipe.recipeKey}
        name={d.recipe.name}
        shortDescription={d.recipe.shortDescription}
        recipeId={d.recipe.recipeKey}
        bookId={d.recipe.bookKey}
        bookTitle={d.book.name}
      />
    );
  }

  if (d.kind === "book") {
    return (
      <BookResult
        name={d.book.name}
        shortDescription={d.book.shortDescription}
        bookId={d.book.id}
        key={"book_" + d.book.id}
      />
    );
  }

  return null;
}

/**
 * properties for `<ResultMapper />`
 */
export interface IResultMapperProps {
  /**
   * the data to render
   */
  data: TaggedSearchPageDataUnionType;
}
