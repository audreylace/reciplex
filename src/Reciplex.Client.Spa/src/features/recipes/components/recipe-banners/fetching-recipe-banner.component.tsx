import { InformationBanner } from "../../../core/components/banner/banner.component";
import fetchingRecipeBannerStylesModule from "./fetching-recipe-banner.module.css";

export function FetchingRecipeBanner() {
  return (
    <InformationBanner
      title="Loading"
      message="Fetching recipe data"
      className={fetchingRecipeBannerStylesModule.fadeIn}
    />
  );
}
