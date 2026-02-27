import { TableMessage } from "./table-message/table-message.component";
import { RetryBannerComponent } from "../../../../core/components/banner/retry-banner.component";

export function LoadFailedUi() {
  return (
    <TableMessage>
      <RetryBannerComponent message="Retrieving list of recipe books failed." />
    </TableMessage>
  );
}
