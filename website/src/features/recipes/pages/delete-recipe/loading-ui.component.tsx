import { FetchingRecipeBanner } from "../../components/fetching-recipe-banner/fetching-recipe-banner.component";
import { RecipeNameAndDescription } from "../../components/recipe-title-and-description/recipe-title-and-description.component";

export function LoadingUi() {
  return (
    <>
      <RecipeNameAndDescription />
      <FetchingRecipeBanner />
    </>
  );
}
