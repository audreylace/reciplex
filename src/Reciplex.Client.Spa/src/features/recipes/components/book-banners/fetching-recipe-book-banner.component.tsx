import { InformationBanner } from "../../../core/components/banner/banner.component";
import fetchingRecipeBookBannerStylesModule from "./fetching-recipe-book-banner.module.css";

/** banner shown while a recipe book is loading */
export function FetchingRecipeBookBanner() {
  return (
    <InformationBanner
      title="Loading"
      message="Fetching recipe book"
      className={fetchingRecipeBookBannerStylesModule.fadeIn}
    />
  );
}
