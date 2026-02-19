import { Field, Input } from "@headlessui/react";
import { useSearchParams, useNavigate, NavLink } from "react-router";
import { SuccessButton } from "../../../core/components/success-button/success-button.component";
import { makeRecipeNameAndDescriptionState } from "../../components/recipe-title-and-description/recipe-title-and-description.component";
import { useRecipeForBookQuery } from "../../hooks/useRecipesForBookQuery.hook";
import { makeViewRecipePath } from "../../route-utils";
import {
  CursorTypes,
  type IPageCursor,
  type IRecipeModel,
} from "../../services/recipe-types";

import commonFormStyles from "../../../core/form-common/form-common.module.css";
import styles from "./recipe-list-table.module.css";

export function RecipeListTable({ bookId }: { bookId: string }) {
  const searchParams = useSearchParams()[0];

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
    <div className={styles.recipeWrapper}>
      <table className={styles.recipeTable}>
        <RecipeListHeader />
        <tbody>
          {recipes.data?.recipes.map((r) => (
            <RecipeListRow key={r.id} recipe={r} />
          ))}
        </tbody>
        <RecipeListTableFooter nextCursor={next} previousCursor={back} />
      </table>
    </div>
  );
}

/**
 * Props for `RecipeListTable`
 * @see RecipeListTable
 */
export interface RecipeListTableProps {
  /** the id of the book to load */
  bookId: string;
}

function RecipeListRow({ recipe }: { recipe: IRecipeModel }) {
  const navigate = useNavigate();
  const path = makeViewRecipePath(recipe.bookId, recipe.id);
  const state = makeRecipeNameAndDescriptionState(
    recipe.name,
    recipe.shortDescription,
  );
  return (
    <tr
      onClick={() => navigate(path, { state })}
      className={styles.recipeListRow}
    >
      <td>{recipe.name}</td>
      <td>{recipe.shortDescription}</td>
    </tr>
  );
}

function RecipeListHeader() {
  return (
    <thead className={styles.recipeHeader}>
      <tr>
        <th colSpan={2} className={styles.searchInput}>
          <form>
            <Field>
              <Input
                type="text"
                className={commonFormStyles.fieldControl}
                placeholder="search"
              ></Input>
              <SuccessButton>
                <i className="bi bi-search"></i>
              </SuccessButton>
            </Field>
          </form>
        </th>
      </tr>
      <tr className={styles.tableColumnHeaders}>
        <th>Name</th>
        <th>Description</th>
      </tr>
    </thead>
  );
}

function RecipeListTableFooter({
  nextCursor,
  previousCursor,
}: {
  nextCursor?: IPageCursor;
  previousCursor?: IPageCursor;
}) {
  const setSearchParams = useSearchParams()[1];

  return (
    <tfoot>
      <tr>
        <td colSpan={2}>
          <div className={styles.paginationWrapper}>
            {previousCursor && (
              <>
                <SuccessButton
                  onClick={() => setSearchParams({})}
                  buttonType="hidden"
                  className={styles.paginationButton}
                >
                  <span className={styles.buttonIconHover}>
                    <i className="bi bi-skip-start-fill"></i>
                  </span>
                  <span className={styles.buttonIconNormal}>
                    <i className="bi bi-skip-start"></i>
                  </span>
                </SuccessButton>
                <SuccessButton
                  onClick={() =>
                    setSearchParams({
                      at: previousCursor.position,
                      source: "previous",
                    })
                  }
                  buttonType="hidden"
                  className={styles.paginationButton}
                >
                  <span className={styles.buttonIconNormal}>
                    <i className="bi bi-rewind"></i>
                  </span>
                  <span className={styles.buttonIconHover}>
                    <i className="bi bi-rewind-fill"></i>
                  </span>
                </SuccessButton>
              </>
            )}
            <div className={styles.paginationGrow}></div>
            {nextCursor && (
              <>
                <SuccessButton
                  onClick={() =>
                    setSearchParams({ at: nextCursor.position, source: "next" })
                  }
                  className={styles.paginationButton}
                  buttonType="hidden"
                >
                  <span className={styles.buttonIconNormal}>
                    <i className="bi bi-fast-forward"></i>
                  </span>
                  <span className={styles.buttonIconHover}>
                    <i className="bi bi-fast-forward-fill"></i>
                  </span>
                </SuccessButton>
                <SuccessButton
                  onClick={() => setSearchParams({ source: "previous" })}
                  className={styles.paginationButton}
                  buttonType="hidden"
                >
                  <span className={styles.buttonIconNormal}>
                    <i className="bi bi-skip-end"></i>
                  </span>
                  <span className={styles.buttonIconHover}>
                    <i className="bi bi-skip-end-fill"></i>
                  </span>
                </SuccessButton>
              </>
            )}
          </div>
        </td>
      </tr>
    </tfoot>
  );
}
