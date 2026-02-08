import { Button, type ButtonProps } from "@headlessui/react";
import styles from "./button-common.module.css";

export function ButtonCommon({
  classArray,
  ...props
}: ButtonProps & { classArray?: string[] }) {
  return (
    <Button
      className={`${(classArray ?? []).join(" ")} ${styles.commonButton}`}
      {...props}
    />
  );
}
