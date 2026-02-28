import styles from "./table-message.module.css";
import type { ComponentChildren } from "preact";

/**
 * Shows a message inside the table
 */
export function TableMessage({ children }: { children: ComponentChildren }) {
  return (
    <tr className={styles.row}>
      <td colSpan={3} className={styles.cell}>
        {children}
      </td>
    </tr>
  );
}
