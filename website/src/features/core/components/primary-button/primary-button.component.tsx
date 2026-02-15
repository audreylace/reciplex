import { type ButtonProps } from "@headlessui/react";
import styles from "./primary-button.module.css";
import { ButtonCommon } from "../../button-common/button-common";

export function PrimaryButton({
  className,
  ...props
}: ButtonProps & { className?: string }) {
  return (
    <ButtonCommon classArray={[className ?? "", styles.primary]} {...props} />
  );
}
