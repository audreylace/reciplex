import { Button, type ButtonProps } from "@headlessui/react";
import styles from "./button-common.module.css";

export function ButtonCommon({
  classArray,
  buttonType,
  ...props
}: ButtonProps & { classArray?: string[]; buttonType?: ButtonType }) {
  const extendedStyle =
    (buttonType ?? "solid") === "solid"
      ? styles.commonButton
      : styles.commonButtonHidden;

  return (
    <Button
      className={`${(classArray ?? []).join(" ")} ${extendedStyle}`}
      {...props}
    />
  );
}

export type ButtonType = "solid" | "hidden";
