import { RecipeBookNotFoundBanner } from "../../components/recipe-book-not-found-banner/recipe-book-not-found-banner.component";
import { FetchingRecipeBookBanner } from "../../components/fetching-recipe-book-banner/fetching-recipe-book-banner.component";
import { FetchingRecipeBookFailedBanner } from "../../components/fetching-recipe-book-failed-banner/fetching-recipe-book-failed-banner.component";
import { OfflineBanner } from "../../components/offline-banner/offline-banner.component";
import formStyles from "../../../core/form-common/form-common.module.css";
import { FetchingStatusDispatch } from "../../../core/components/fetch-status-dispatch/fetch-status-dispatch.component";
import { QueryStatusDispatch } from "../../../core/components/query-status-dispatch/query-status-dispatch.component";
import { makeViewRecipeBookPath, makeViewRecipePath } from "../../route-utils";
import { useNavigate, useParams } from "react-router";
import { useGetRecipeBookById } from "../../hooks/useGetRecipeBookById.hook";
import {
  BookInformationBanner,
  makeBookNameAndDescriptionState,
} from "../../components/book-information-banner/book-information-banner.component";
import { CreateRecipeForm } from "../../components/create-recipe-form/create-recipe-form.component";

/**
 * Entry point for create recipe page component
 */
export function CreateRecipePage() {
  const { bookId } = useParams<{
    bookId: string;
  }>();

  const { fetchStatus, status, data: bookData } = useGetRecipeBookById(bookId);
  const { name: bookName, shortDescription: bookShortDescription } =
    bookData ?? {};

  const navigate = useNavigate();
  const onCancel = () => {
    if (bookId) {
      const bookPath = makeViewRecipeBookPath(bookId);
      const bookNavigationState = makeBookNameAndDescriptionState(
        bookName,
        bookShortDescription,
      );
      navigate(bookPath, { state: bookNavigationState });
    }
  };
  const onAfterCreated = (recipeId: string) => {
    if (bookId) {
      navigate(makeViewRecipePath(bookId, recipeId));
    }
  };

  return (
    <main className={`${formStyles.formMain}`}>
      <BookInformationBanner
        name={bookName}
        shortDescription={bookShortDescription}
      />
      <QueryStatusDispatch
        loadingStatus={status}
        error={<FetchingRecipeBookFailedBanner />}
        pending={
          <FetchingStatusDispatch
            fetchStatus={fetchStatus}
            fetching={<FetchingRecipeBookBanner />}
            idle={<FetchingRecipeBookFailedBanner />}
            paused={<OfflineBanner />}
          />
        }
        success={() => {
          if (!bookData) {
            return <RecipeBookNotFoundBanner />;
          }

          return (
            <CreateRecipeForm
              book={bookData}
              onCancel={onCancel}
              onAfterCreated={onAfterCreated}
            />
          );
        }}
      />
    </main>
  );
}
