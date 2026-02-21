import { NavLink, useParams } from "react-router";
import { makeCreateRecipePath } from "../../route-utils";
import { useGetRecipeBookById } from "../../hooks/useGetRecipeBookById.hook";
import { RecipeBookNotFoundBanner } from "../../components/recipe-book-not-found-banner/recipe-book-not-found-banner.component";
import { FetchingRecipeBookBanner } from "../../components/fetching-recipe-book-banner/fetching-recipe-book-banner.component";
import { FetchingRecipeBookFailedBanner } from "../../components/fetching-recipe-book-failed-banner/fetching-recipe-book-failed-banner.component";
import { BadPathBanner } from "../../components/bad-path-banner/bad-path-banner.component";
import { DangerButton } from "../../../core/components/danger-button/danger-button.component";
import { SuccessButton } from "../../../core/components/success-button/success-button.component";
import { FormButtons } from "../../../core/components/form-buttons/form-buttons.component";
import {
  BookInformationBanner,
  makeBookNameAndDescriptionState,
} from "../../components/book-information-banner/book-information-banner";
import { RecipeListTable } from "./recipe-list-table.component";

import styles from "./view-recipe-book-page.module.css";

/**
 * Entry point for viewing a recipe book
 */
export function ViewRecipeBookPage() {
  const { bookId } = useParams<{
    bookId: string;
  }>();
  const bookQuery = useGetRecipeBookById(bookId);
  const notFound = bookQuery.isSuccess && !bookQuery.data;
  const bookNavState = bookQuery.data
    ? makeBookNameAndDescriptionState(
        bookQuery.data.name,
        bookQuery.data.shortDescription,
      )
    : undefined;
  return (
    <main className="pageMain">
      {!bookId && <BadPathBanner />}
      {bookId && (
        <>
          {bookQuery.isLoading && <FetchingRecipeBookBanner />}
          {notFound && <RecipeBookNotFoundBanner />}
          {bookQuery.isError && <FetchingRecipeBookFailedBanner />}
          {bookQuery.isSuccess && bookQuery.data && (
            <>
              <BookInformationBanner
                name={bookQuery.data.name}
                shortDescription={bookQuery.data.shortDescription}
              />
              <div className={styles.formButtonBar}>
                <FormButtons notInForm>
                  {bookQuery.data.canAddRecipesToBook && (
                    <NavLink to={makeCreateRecipePath(bookId)}>
                      <SuccessButton buttonType="dotted">
                        Add Recipe
                      </SuccessButton>
                    </NavLink>
                  )}
                  {bookQuery.data.canDeleteBook && (
                    <NavLink
                      to={`/books/${bookId}/delete`}
                      state={bookNavState}
                    >
                      <DangerButton buttonType="dotted">
                        Delete Recipe Book
                      </DangerButton>
                    </NavLink>
                  )}
                  {bookQuery.data.canEditBookInformation && (
                    <NavLink to={`/books/${bookId}/edit`} state={bookNavState}>
                      <SuccessButton buttonType="dotted">
                        Edit Recipe Book Information
                      </SuccessButton>
                    </NavLink>
                  )}
                </FormButtons>
              </div>
              <RecipeListTable bookId={bookId} />
            </>
          )}
        </>
      )}
    </main>
  );
}
