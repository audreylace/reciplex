import { useNavigate } from "react-router";
import { makeBookNameAndDescriptionState } from "../../components/book-information-banner/book-information-banner";
import { DeleteRecipeForm } from "../../components/delete-recipe-form/delete-recipe-form.component";
import {
  RecipeNameAndDescription,
  makeRecipeNameAndDescriptionState,
} from "../../components/recipe-title-and-description/recipe-title-and-description.component";
import { makeViewRecipeBookPath, makeViewRecipePath } from "../../route-utils";
import type {
  IRecipeBookModel,
  IRecipeModel,
} from "../../services/recipe-types";

export function DeleteRecipeEditBody({
  book,
  recipe,
}: {
  book: IRecipeBookModel;
  recipe: IRecipeModel;
}) {
  const navigate = useNavigate();
  return (
    <>
      <RecipeNameAndDescription
        name={recipe.name}
        shortDescription={recipe.shortDescription}
      />
      <DeleteRecipeForm
        key={recipe.id}
        data={recipe}
        onDeleted={() =>
          navigate(makeViewRecipeBookPath(book.id), {
            state: makeBookNameAndDescriptionState(
              book.name,
              book.shortDescription,
            ),
          })
        }
        onCancel={() =>
          navigate(makeViewRecipePath(book.id, recipe.id), {
            state: makeRecipeNameAndDescriptionState(
              recipe.name,
              recipe.shortDescription,
            ),
          })
        }
      />
    </>
  );
}
