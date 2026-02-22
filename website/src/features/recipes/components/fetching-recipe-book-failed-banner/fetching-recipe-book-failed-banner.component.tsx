import { RetryBannerComponent } from "../retry-banner/retry-banner.component";

export function FetchingRecipeBookFailedBanner() {
  return (
    <RetryBannerComponent message="Failed to fetch recipe book from the cloud." />
  );
}
