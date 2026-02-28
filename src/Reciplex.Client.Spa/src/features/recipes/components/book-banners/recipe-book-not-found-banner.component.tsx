import { ErrorBanner } from "../../../core/components/banner/banner.component";

/** error banner shown when a recipe book was not found */
export function RecipeBookNotFoundBanner() {
  return (
    <ErrorBanner
      to="/"
      title="Not Found"
      message="Requested recipe book was not found"
      buttonCaption="Go Home"
    />
  );
}
