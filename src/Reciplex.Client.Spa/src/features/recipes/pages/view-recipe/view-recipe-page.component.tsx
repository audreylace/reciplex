import { useNavigate, useParams } from "react-router";
import { useGetRecipeByIdQuery } from "../../hooks/useGetRecipeByIdQuery.hook";
import { NotFoundAlert } from "../../../core/components/not-found-alert/not-found-alert.component";
import { RecipeMenuButton } from "../../components/recipe-menu/recipe-menu.component";
import { LoadingIndicator } from "../../../core/components/loading-indicator/loading-indicator.component";
import { LoadingFailedAlert } from "../../../core/components/loading-failed-alert/loading-failed-alert.component";
import { DetailsRender } from "./details-render.component";
import Typography from "@mui/material/Typography";
import Box from "@mui/material/Box";
import Stack from "@mui/material/Stack";
import { makeEditRecipePath } from "../../route-utils";

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
      <Box
        sx={{
          my: 2,
        }}
      >
        <Stack direction={"row"} sx={{ width: "100%" }}>
          <Box
            sx={{
              flex: "1 1 auto",
            }}
          >
            <Typography variant="h4">{data.name}</Typography>
          </Box>
          <RecipeMenuButton
            recipeId={recipeId}
            mayEdit={data.mayEdit}
            bookId={data.bookId}
          />
        </Stack>
        <Typography variant="subtitle1">{data.shortDescription}</Typography>
      </Box>
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
