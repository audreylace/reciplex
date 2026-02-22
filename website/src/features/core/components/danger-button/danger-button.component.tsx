import { type ButtonProps } from "@headlessui/react";
import styles from "./danger-button.module.css";
import {
  ButtonCommon,
  type ButtonType,
} from "../button-common/button-common.component";

export function DangerButton({
  className,
  ...props
}: ButtonProps & { className?: string; buttonType?: ButtonType }) {
  return (
    <ButtonCommon classArray={[className ?? "", styles.danger]} {...props} />
  );
}
