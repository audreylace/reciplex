import type { PropsWithChildren } from "preact/compat";
import formStyles from "../../form-common/form-common.module.css";

export function FormButtons({
  children,
  notInForm,
}: PropsWithChildren<{ notInForm?: boolean }>) {
  return (
    <div
      className={formStyles.formButtonRow}
      data-not-in-form={notInForm ?? false}
    >
      {children}
    </div>
  );
}
