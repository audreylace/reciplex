import { NavLink, useNavigate, useParams, useSearchParams } from "react-router";
import { makeCreateRecipePath, makeViewRecipePath } from "../../route-utils";
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
import { useRecipeForBookQuery } from "../../hooks/useRecipesForBookQuery.hook";
import { CursorTypes, type IRecipeModel } from "../../services/recipe-types";
import { makeRecipeNameAndDescriptionState } from "../../components/recipe-title-and-description/recipe-title-and-description.component";

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
              <FormButtons notInForm>
                {bookQuery.data.canAddRecipesToBook && (
                  <NavLink to={makeCreateRecipePath(bookId)}>
                    <SuccessButton buttonType="dotted">
                      Add Recipe
                    </SuccessButton>
                  </NavLink>
                )}
                {bookQuery.data.canDeleteBook && (
                  <NavLink to={`/books/${bookId}/delete`} state={bookNavState}>
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
            </>
          )}
          <RecipeListTable bookId={bookId} />
        </>
      )}
    </main>
  );
}

function RecipeListTable({ bookId }: { bookId: string }) {
  const [searchParams, setSearchParams] = useSearchParams();

  const at = searchParams.get("at") ?? undefined;
  const sourceQuery = searchParams.get("source");
  const source =
    sourceQuery === "next"
      ? CursorTypes.next
      : sourceQuery === "previous"
        ? CursorTypes.previous
        : undefined;

  const recipes = useRecipeForBookQuery(
    bookId,
    source ? { position: at, type: source } : undefined,
  );

  const next = recipes.data?.nextCursor;
  const back = recipes.data?.previousCursor;
  return (
    <>
      <table>
        <RecipeListHeader />
        <tbody>
          {recipes.data?.recipes.map((r) => (
            <RecipeListRow key={r.id} recipe={r} />
          ))}
        </tbody>
      </table>
      {back && (
        <>
          <button onClick={() => setSearchParams({})}>First</button>
          <button
            onClick={() =>
              setSearchParams({ at: back.position, source: "previous" })
            }
          >
            back
          </button>
        </>
      )}
      {next && (
        <>
          <button
            onClick={() =>
              setSearchParams({ at: next.position, source: "next" })
            }
          >
            next
          </button>
          <button onClick={() => setSearchParams({ source: "previous" })}>
            Last
          </button>
        </>
      )}
    </>
  );
}
function RecipeListRow({ recipe }: { recipe: IRecipeModel }) {
  const navigate = useNavigate();
  const path = makeViewRecipePath(recipe.bookId, recipe.id);
  const state = makeRecipeNameAndDescriptionState(
    recipe.name,
    recipe.shortDescription,
  );
  return (
    <tr onClick={() => navigate(path, { state })}>
      <td>{recipe.name}</td>
      <td>{recipe.shortDescription}</td>
    </tr>
  );
}

function RecipeListHeader() {
  return (
    <thead>
      <tr>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
  );
}
