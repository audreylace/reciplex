import { type ButtonProps } from "@headlessui/react";
import styles from "./danger-button.module.css";
import { ButtonCommon } from "../../button-common/button-common";

export function DangerButton({
  className,
  ...props
}: ButtonProps & { className?: string }) {
  return (
    <ButtonCommon classArray={[className ?? "", styles.danger]} {...props} />
  );
}
