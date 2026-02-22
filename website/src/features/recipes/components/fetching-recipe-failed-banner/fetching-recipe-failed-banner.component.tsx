import { RetryBannerComponent } from "../retry-banner/retry-banner.component";

export function FetchingRecipeFailedBanner() {
  return (
    <RetryBannerComponent message="Failed to fetch recipe from the cloud." />
  );
}
