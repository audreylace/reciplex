import { useRequestAccessToBookMutation } from "../../hooks/useRequestAccessToBookMutation.hook";
import { InvitePageBodyTemplate } from "./invite-form-template.component";

/** form users will use to request access to a recipe book */
export function RequestAccessBody({
  name,
  shortDescription,
  shareKey,
  bookId,
}: IRequestAccessBodyProps) {
  const requestAccessMutation = useRequestAccessToBookMutation();
  return (
    <InvitePageBodyTemplate
      name={name}
      shortDescription={shortDescription}
      title="Request access to recipe book"
      buttonCaption="Request Access"
      buttonDisabled={!requestAccessMutation.isIdle}
      showErrorBanner={requestAccessMutation.isError}
      showLoadingIndicator={requestAccessMutation.isPending}
      onClick={() => {
        if (requestAccessMutation.isIdle) {
          requestAccessMutation.mutateAsync({ shareKey, bookId });
        }
      }}
    />
  );
}

/** props for `<RequestAccessBody />` */
export interface IRequestAccessBodyProps {
  /** name of the book. */
  name: string;
  /** book short description. */
  shortDescription: string;
  /** book share key. Used by server to authenticate request. */
  shareKey: string;
  /** book id. */
  bookId: string;
}
