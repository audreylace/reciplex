import Backdrop from "@mui/material/Backdrop";
import { HardApplicationFailureBanner } from "../../features/core/components/hard-application-failure-banner/hard-application-failure-banner.component";

/** router error boundary component */
export function ErrorBoundary() {
  return (
    <Backdrop open>
      <HardApplicationFailureBanner />
    </Backdrop>
  );
}
