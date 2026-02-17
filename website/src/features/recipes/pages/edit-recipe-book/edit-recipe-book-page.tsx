import { BadPathBanner } from "../../components/bad-path-banner/bad-path-banner.component";
import formStyles from "../../../core/form-common/form-common.module.css";
import { useNavigate, useParams } from "react-router";
import { RecipeBookEditor } from "../../components/recipe-book-editor/recipe-book-editor";
import { makeViewRecipeBookPath } from "../../route-utils";
import { BookInformationBannerWithQuery } from "../../components/book-information-banner/book-information-banner-with-query";
import { makeBookNameAndDescriptionState } from "../../components/book-information-banner/book-information-banner";

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
          <BookInformationBannerWithQuery bookId={bookId} />
          <RecipeBookEditor
            key={bookId}
            bookId={bookId}
            onSaved={goBackToBook}
            onCancel={goBackToBook}
          />
        </>
      )}
    </main>
  );
}
