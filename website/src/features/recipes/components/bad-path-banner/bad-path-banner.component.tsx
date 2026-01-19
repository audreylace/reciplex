import { ActionBanner } from "../action-banner/action-banner.component";

export function BadPathBanner() {
  return (
    <ActionBanner message="Something web wrong." to="/" linkText="go home" />
  );
}
