import { HardApplicationFailureBanner } from "../../features/core/components/hard-application-failure-banner/hard-application-failure-banner.component";

/** webserver redirects to this page on internal failure */
export function AppErrorPage() {
  return <HardApplicationFailureBanner />;
}
