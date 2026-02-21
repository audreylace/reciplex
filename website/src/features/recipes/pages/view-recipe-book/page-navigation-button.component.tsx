import { SuccessButton } from "../../../core/components/success-button/success-button.component";

import styles from "./page-navigation-button.module.css";

/** button in the navigation bar */
export function PageNavigationButton({
  icon,
  hoverIcon,
  onClick,
  visibilityHidden,
}: PageNavigationButtonProps) {
  return (
    <SuccessButton
      onClick={onClick}
      className={styles.paginationButton}
      buttonType="dotted"
      data-hide={visibilityHidden}
      disabled={visibilityHidden}
    >
      <span className={styles.buttonIconNormal}>
        <i className={icon}></i>
      </span>
      <span className={styles.buttonIconHover}>
        <i className={hoverIcon}></i>
      </span>
    </SuccessButton>
  );
}

/** props for `PageNavigationButton` */
export interface PageNavigationButtonProps {
  /** normal icon */
  icon: string;
  /** icon on hover */
  hoverIcon: string;
  /** invoked on click */
  onClick: () => void;
  /** hide the button with visibility hidden to still take of space */
  visibilityHidden: boolean;
}
