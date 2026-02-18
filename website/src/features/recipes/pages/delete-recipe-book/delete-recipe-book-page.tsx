import { BadPathBanner } from "../../components/bad-path-banner/bad-path-banner.component";
import { useNavigate, useParams } from "react-router";
import { makeBookListPath, makeViewRecipeBookPath } from "../../route-utils";
import { makeBookNameAndDescriptionState } from "../../components/book-information-banner/book-information-banner";
import { BookInformationBannerWithQuery } from "../../components/book-information-banner/book-information-banner-with-query";
import { RecipeBookMutationLoader } from "../../components/recipe-book-loader/recipe-book-mutation-loader";
import { DeleteRecipeBookForm } from "../../components/delete-recipe-book-form/delete-recipe-book-form";

import formStyles from "../../../core/form-common/form-common.module.css";

export function DeleteRecipeBookPage() {
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

  const onDeleted = () => {
    navigate(makeBookListPath());
  };
  return (
    <main className={`${formStyles.formMain}`}>
      {!bookId && <BadPathBanner />}
      {bookId && (
        <>
          <BookInformationBannerWithQuery bookId={bookId} />
          <RecipeBookMutationLoader
            key={bookId}
            bookId={bookId}
            noCache={true}
            refetchInterval={10000}
            onRender={(book) => {
              return (
                <DeleteRecipeBookForm
                  onCancel={goBackToBook}
                  onDeleted={onDeleted}
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
