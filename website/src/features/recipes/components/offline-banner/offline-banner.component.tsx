import { InformationBanner } from "../../../core/components/banner/banner.component";

export function OfflineBanner() {
  return (
    <InformationBanner
      title="Offline"
      icon="bi bi-wifi-off"
      message="Your offline. We will reconnect once you are online again."
    />
  );
}
