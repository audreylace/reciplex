import { type ButtonProps } from "@headlessui/react";
import styles from "./success-button.module.css";
import { ButtonCommon } from "../button-common/button-common";

export function SuccessButton({
  className,
  ...props
}: ButtonProps & { className?: string }) {
  return (
    <ButtonCommon classArray={[className ?? "", styles.success]} {...props} />
  );
}
