import { type ButtonProps } from "@headlessui/react";
import styles from "./primary-button.module.css";
import {
  ButtonCommon,
  type ButtonType,
} from "../buttons/button-common.component";

export function PrimaryButton({
  className,
  ...props
}: ButtonProps & { className?: string; buttonType?: ButtonType }) {
  return (
    <ButtonCommon classArray={[className ?? "", styles.primary]} {...props} />
  );
}
