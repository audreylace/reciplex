import { BadPathBanner } from "../../components/bad-path-banner/bad-path-banner.component";
import formStyles from "../../../core/form-common/form-common.module.css";
import { useNavigate, useParams } from "react-router";
import { makeViewRecipeBookPath } from "../../route-utils";
import { BookInformationHeaderWithQuery } from "../../components/book-information-header/book-information-header-with-query.component";
import { makeBookNameAndDescriptionState } from "../../components/book-information-header/book-information-header.component";
import { RecipeBookMutationLoader } from "../../components/recipe-book-loader/recipe-book-mutation-loader.component";
import { EditRecipeBookForm } from "../../components/edit-recipe-book-form/edit-recipe-book-form.component";

/**
 * page for editing a recipe book
 */
export function EditRecipeBookPage() {
  const { bookId } = useParams<{ bookId: string }>();
  const navigate = useNavigate();
  const goBackToBook = (args?: {
    name: string;
    bookId: string;
    shortDescription: string;
  }) => {
    const id = bookId ?? args?.bookId;
    if (id) {
      navigate(makeViewRecipeBookPath(id), {
        state: makeBookNameAndDescriptionState(
          args?.name,
          args?.shortDescription,
        ),
      });
    }
  };

  return (
    <main className={formStyles.formMain}>
      {!bookId && <BadPathBanner />}
      {bookId && (
        <>
          <BookInformationHeaderWithQuery bookId={bookId} />
          <RecipeBookMutationLoader
            key={bookId}
            bookId={bookId}
            noCache={true}
            refetchInterval={10000}
            onRender={(book) => {
              return (
                <EditRecipeBookForm
                  onCancel={goBackToBook}
                  onSaved={goBackToBook}
                  data={book}
                />
              );
            }}
          />
        </>
      )}
    </main>
  );
}
