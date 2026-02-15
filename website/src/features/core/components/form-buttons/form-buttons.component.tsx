import type { PropsWithChildren } from "preact/compat";
import formStyles from "../../form-common/form-common.module.css";

export function FormButtons({ children }: PropsWithChildren<{}>) {
  return <div className={formStyles.formButtonRow}>{children}</div>;
}
