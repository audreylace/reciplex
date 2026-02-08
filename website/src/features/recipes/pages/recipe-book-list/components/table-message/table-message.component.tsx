import type { PropsWithChildren } from "preact/compat";
import styles from "./table-message.module.css";

/**
 * Shows a message inside the table
 */
export function TableMessage({ children }: PropsWithChildren<{}>) {
  return (
    <tr className={styles.row}>
      <td colspan={3} className={styles.cell}>
        {children}
      </td>
    </tr>
  );
}
