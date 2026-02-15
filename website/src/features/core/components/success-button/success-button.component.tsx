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
  const extendedStyle =
    (buttonType ?? "solid") === "solid" ? styles.success : styles.successHidden;

  return (
    <ButtonCommon
      buttonType={buttonType}
      classArray={[className ?? "", extendedStyle]}
      {...props}
    />
  );
}
