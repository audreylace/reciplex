import { useNavigate, useParams } from "react-router";
import { useGetRecipeByIdQuery } from "../../hooks/useGetRecipeByIdQuery.hook";
import { NotFoundAlert } from "../../../core/components/not-found-alert/not-found-alert.component";
import { RecipeMenuButton } from "../../components/recipe-menu/recipe-menu.component";
import { LoadingIndicator } from "../../../core/components/loading-indicator/loading-indicator.component";
import { LoadingFailedAlert } from "../../../core/components/loading-failed-alert/loading-failed-alert.component";
import { DetailsRender } from "./details-render.component";
import { makeEditRecipePath } from "../../route-utils";
import { PageHeader } from "../../../core/components/page-header/page-header.component";

/**
 * page for viewing a recipe
 */
export function ViewRecipePage() {
  const { recipeId } = useParams<{
    recipeId: string;
  }>();
  const { data, isPending, isError } = useGetRecipeByIdQuery(recipeId);
  const navigate = useNavigate();

  if (isPending) {
    return <LoadingIndicator />;
  }

  if (isError) {
    return <LoadingFailedAlert />;
  }

  if (!recipeId || !data) {
    return <NotFoundAlert />;
  }

  return (
    <>
      <PageHeader
        title={data.name}
        subTitle={data.shortDescription}
        sideComponent={
          <RecipeMenuButton
            recipeId={recipeId}
            mayEdit={data.mayEdit}
            bookId={data.bookId}
            hideViewRecipeLink
          />
        }
      />
      <DetailsRender
        detailsMd={data.details}
        mayEdit={data.mayEdit}
        goToEditAction={() => {
          navigate(makeEditRecipePath(data.bookId, data.id));
        }}
      />
    </>
  );
}
