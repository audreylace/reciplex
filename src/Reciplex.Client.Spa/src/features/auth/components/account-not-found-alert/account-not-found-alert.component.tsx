import { NotFoundAlert } from "../../../core/components/not-found-alert/not-found-alert.component";
import { useSelectAccountNavigate } from "../../hooks/useSelectAccountNavigate.hook";

/** alert shown when an account is not found */
export function AccountNotFoundAlert({ show }: IAccountNotFoundAlertProps) {
  const [selectAccountPath, navigateSelectAccount] = useSelectAccountNavigate();
  return (
    <NotFoundAlert
      caption="View Accounts"
      href={selectAccountPath}
      onClick={navigateSelectAccount}
      show={show}
    />
  );
}

/** properties for `AccountNotFoundAlert` */
export interface IAccountNotFoundAlertProps {
  /**
   * Flag controlling if the alert is visible.
   * Only hides the alert if value is false.
   * All other values map to true.
   * @see NotFoundAlert
   */
  show?: boolean;
}
