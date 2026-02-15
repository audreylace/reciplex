import { Button, type ButtonProps } from "@headlessui/react";
import styles from "./button-common.module.css";

export function ButtonCommon({
  classArray,
  buttonType,
  ...props
}: ButtonProps & { classArray?: string[]; buttonType?: ButtonType }) {
  buttonType ??= "solid";
  const extendedStyle =
    buttonType === "solid"
      ? styles.commonButton
      : buttonType === "dotted"
        ? styles.commonButtonDotted
        : styles.commonButtonHidden;

  return (
    <Button
      className={`${(classArray ?? []).join(" ")} ${styles.commonButton}`}
      data-button-type={buttonType}
      {...props}
    />
  );
}

export type ButtonType = "solid" | "hidden" | "dotted";
