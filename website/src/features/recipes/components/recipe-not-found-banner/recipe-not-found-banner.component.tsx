import { ActionBanner } from "../action-banner/action-banner.component";

export function RecipeNotFoundBanner({}: {}) {
  return (
    <ActionBanner
      to="/"
      message="Recipe not found in the cloud ..."
      linkText="Go home"
    />
  );
}
