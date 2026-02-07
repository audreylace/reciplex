import { useMemo } from "preact/hooks";
import type {
  IGetRecipeBooksResult,
  IRecipeBookModel,
} from "../../../services/recipe-store";

export function useBookData(data: IGetRecipeBooksResult | undefined) {
  return useMemo(() => {
    if (!data?.page || !data?.recipeBooks) {
      return null;
    }
    const orderedBooks: IRecipeBookModel[] = [];
    for (const key of data.page) {
      const book = data.recipeBooks[key];
      if (!book) {
        return null;
      }
      orderedBooks.push(book);
    }
    return orderedBooks.length > 0 ? orderedBooks : null;
  }, [data?.page, data?.recipeBooks]);
}
