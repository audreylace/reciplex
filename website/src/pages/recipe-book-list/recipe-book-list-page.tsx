import { useContext } from "preact/hooks";
import {
  CursorTypes,
  type IGetRecipeBooksArgs,
} from "../../services/recipe-store";
import { Link, useNavigate, useParams } from "react-router";
import style from "./recipe-book-list-page.module.css";
import { useQuery } from "@tanstack/react-query";
import {
  BookListNavigationAction,
  makeBookListPath,
  makeCreateRecipeBookPath,
  makeViewRecipeBookPath,
} from "../../features/recipes/route-utils";
import { RecipeStore } from "../../features/recipes/hooks/useRecipeStoreContext.hook";

/**
 * Entry point for recipe book list page component
 * @param param0 react props
 * @returns jsx tree for rendering by react
 */
export function RecipeBookListPage({}: {}) {
  const recipeStore = useContext(RecipeStore);
  const navigate = useNavigate();

  /**
   * route parameters
   */
  type RouteParams = {
    /** the source of the route navigation */
    source: BookListNavigationAction;
    /** the navigation index */
    index: string;
  };
  const { source, index } = useParams<RouteParams>();

  const query = useQuery({
    queryKey: ["recipe-book-list", { source, index }],
    queryFn: async () => {
      if (!recipeStore) {
        throw new Error("Require recipe store");
      }
      const args: IGetRecipeBooksArgs = {};
      if (
        source &&
        (source === BookListNavigationAction.next ||
          source === BookListNavigationAction.previous)
      ) {
        args.cursor = {
          position: index,
          type:
            source === BookListNavigationAction.next
              ? CursorTypes.next
              : CursorTypes.previous,
        };
      }

      const result = await recipeStore.getRecipeBooks(args);
      if (!result) {
        throw new Error("Get book API failed");
      }

      return result;
    },
  });

  if (query.isLoading) {
    return <p>fetching recipe books from the cloud ... </p>;
  }

  const data = query.data;
  if (!data || query.isError) {
    return (
      <p>
        Retrieving list of recipe books failed.{" "}
        <a
          href="#"
          onClick={(event) => {
            event.preventDefault();
            navigate(0);
          }}
        >
          Try again?
        </a>
      </p>
    );
  }
  return (
    <main>
      {(data.previousCursor || data.nextCursor) && (
        <ul>
          {data.previousCursor && (
            <>
              <li>
                <Link to={makeBookListPath()}>First</Link>
              </li>
              <li>
                <Link
                  to={makeBookListPath({
                    cursor: {
                      order: BookListNavigationAction.previous,
                      index: data.previousCursor.position,
                    },
                  })}
                >
                  Previous
                </Link>
              </li>
            </>
          )}
          {data.nextCursor && (
            <>
              <li>
                <Link
                  to={makeBookListPath({
                    cursor: {
                      order: BookListNavigationAction.next,
                      index: data.nextCursor.position,
                    },
                  })}
                >
                  Next
                </Link>
              </li>
              <li>
                <Link
                  to={makeBookListPath({
                    cursor: {
                      order: BookListNavigationAction.previous,
                    },
                  })}
                >
                  Last
                </Link>
              </li>
            </>
          )}
        </ul>
      )}

      {data.page.length > 0 && (
        <table className={style.table}>
          <thead>
            <tr>
              <th>Name</th>
              <th>Description</th>
              <th aria-description="column with links to the recipe book"></th>
            </tr>
          </thead>
          <tbody>
            {data.page.map((key) => {
              const book = data.recipeBooks[key];
              if (!book) {
                return null;
              }
              const path = makeViewRecipeBookPath(book.id);
              return (
                <tr key={key} onClick={() => navigate(path)}>
                  <td>{book.name}</td>
                  <td>{book.shortDescription}</td>
                  <td>
                    <Link to={path}>View</Link>
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
      )}
      {data.page.length <= 0 && (
        <p>
          <Link to={makeCreateRecipeBookPath()}>
            Create your first recipe book
          </Link>
        </p>
      )}

      {query.isFetching && <p>Checking cloud for new data ... </p>}
    </main>
  );
}
