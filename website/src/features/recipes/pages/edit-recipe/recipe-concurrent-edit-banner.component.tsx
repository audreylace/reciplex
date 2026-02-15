import { RetryBannerComponent } from "../../components/retry-banner/retry-banner.component";

export function RecipeConcurrentEditBanner() {
  return (
    <RetryBannerComponent
      buttonCaption="Discard and Reload?"
      message="Another user has made changes to this recipe. Existing changes must be discarded and the recipe reloaded."
    />
  );
}
