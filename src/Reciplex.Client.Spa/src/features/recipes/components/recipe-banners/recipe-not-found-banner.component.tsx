import { ErrorBanner } from "../../../core/components/banner/banner.component";

/** banner when a user tries to load a non-existent recipe */
export function RecipeNotFoundBanner() {
  return (
    <ErrorBanner
      to="/"
      title="Not Found"
      message="Requested recipe was not found"
      buttonCaption="Go Home"
    />
  );
}
