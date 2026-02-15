import { type ButtonProps } from "@headlessui/react";
import styles from "./success-button.module.css";
import {
  ButtonCommon,
  type ButtonType,
} from "../../button-common/button-common";

export function SuccessButton({
  className,
  buttonType,
  ...props
}: ButtonProps & { className?: string; buttonType?: ButtonType }) {
  return (
    <ButtonCommon
      buttonType={buttonType}
      classArray={[className ?? "", styles.success]}
      {...props}
    />
  );
}
