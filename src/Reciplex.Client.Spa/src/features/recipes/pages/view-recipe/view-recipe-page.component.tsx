import { useNavigate, useParams } from "react-router";
import { useGetRecipeByIdQuery } from "../../hooks/useGetRecipeByIdQuery.hook";
import { NotFoundAlert } from "../../../core/components/not-found-alert/not-found-alert.component";
import { LoadingIndicator } from "../../../core/components/loading-indicator/loading-indicator.component";
import { LoadingFailedAlert } from "../../../core/components/loading-failed-alert/loading-failed-alert.component";
import { makeEditRecipePath } from "../../route-utils";
import { PageHeader } from "../../../core/components/page-header/page-header.component";
import { RecipeMenuButton } from "../../components/recipe-menu-button/recipe-menu-button.component";
import { RecipeDetailsViewer } from "../../components/recipe-details-viewer/recipe-details-viewer.component";
import { SharedRecipeBookIndicator } from "../../components/shared-recipe-book-indicator/shared-recipe-book-indicator.component";
import { BrowserTitle } from "../../../core/components/browser-title/browser-title.component";

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
      <BrowserTitle title={data.name ? data.name : "View Review"} />
      <PageHeader
        title={data.name}
        subTitle={data.shortDescription}
        titleComponent={<SharedRecipeBookIndicator bookId={data.bookId} />}
        sideComponent={
          <RecipeMenuButton
            recipeId={recipeId}
            mayEdit={data.mayEdit}
            bookId={data.bookId}
            hideViewRecipeLink
          />
        }
      />
      <RecipeDetailsViewer
        detailsMd={data.details}
        mayEdit={data.mayEdit}
        goToEditAction={() => {
          navigate(makeEditRecipePath(data.bookId, data.id));
        }}
      />
    </>
  );
}
