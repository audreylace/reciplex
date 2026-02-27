import { FetchingRecipeBookBanner } from "../../components/book-banners/fetching-recipe-book-banner.component";
import { FetchingRecipeBookFailedBanner } from "../../components/book-banners/fetching-recipe-book-failed-banner.component";
import { OfflineBanner } from "../../../core/components/banner/offline-banner.component";
import formStyles from "../../../core/form-common/form-common.module.css";
import { FetchingStatusDispatch } from "../../../core/components/fetch-status-dispatch/fetch-status-dispatch.component";
import { QueryStatusDispatch } from "../../../core/components/query-status-dispatch/query-status-dispatch.component";
import { makeViewRecipeBookPath, makeViewRecipePath } from "../../route-utils";
import { useNavigate, useParams } from "react-router";
import { useGetRecipeBookById } from "../../hooks/useGetRecipeBookById.hook";
import {
  BookInformationHeader,
  makeBookNameAndDescriptionState,
} from "../../components/book-information-header/book-information-header.component";
import { CreateRecipeForm } from "../../components/create-recipe-form/create-recipe-form.component";
import { RecipeBookNotFoundBanner } from "../../components/book-banners/recipe-book-not-found-banner.component";
import type { IRecipeBookModel } from "../../services/recipe-types";

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

  return (
    <main className={`${formStyles.formMain}`}>
      <BookInformationHeader
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
        success={<SuccessRender bookData={bookData} />}
      />
    </main>
  );
}

function SuccessRender({
  bookData,
}: {
  bookData: IRecipeBookModel | null | undefined;
}) {
  const {
    id: bookId,
    name: bookName,
    shortDescription: bookShortDescription,
  } = bookData ?? {};
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
}
