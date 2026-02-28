import type { IRecipeBookModel } from "../../../services/recipe-types";
import { TableRow } from "./table-row/table-row.component";

export function BookListUi({ books }: { books: IRecipeBookModel[] }) {
  return (
    <>
      {books.map((book) => (
        <TableRow book={book} key={book.id} />
      ))}
    </>
  );
}
