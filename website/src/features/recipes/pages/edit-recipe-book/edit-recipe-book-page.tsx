import { BadPathBanner } from "../../components/bad-path-banner/bad-path-banner.component";
import formStyles from "../../../core/form-common/form-common.module.css";
import { useNavigate, useParams } from "react-router";
import { RecipeBookEditor } from "../../components/recipe-book-editor/recipe-book-editor";
import { makeViewRecipeBookPath } from "../../route-utils";

/**
 * page for editing a recipe book
 */
export function EditRecipeBookPage() {
  const { bookId } = useParams<{ bookId: string }>();
  const navigate = useNavigate();
  const goBackToBook = () => {
    if (bookId) {
      navigate(makeViewRecipeBookPath(bookId));
    }
  };

  return (
    <main className={formStyles.formMain}>
      {!bookId && <BadPathBanner />}
      {bookId && (
        <RecipeBookEditor
          key={bookId}
          bookId={bookId}
          onSaved={goBackToBook}
          onCancel={goBackToBook}
        />
      )}
    </main>
  );
}
