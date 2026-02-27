import { NavLink } from "react-router";
import { SuccessButton } from "../buttons/success-button.component";
import styles from "./app-navigation.module.css";
import type { PropsWithChildren } from "preact/compat";

export function AppNavigation({
  children,
  childrenType,
}: PropsWithChildren<{ childrenType?: AppNavigationChildrenType }>) {
  return (
    <div className={`${styles.appNavigationDiv}`}>
      <ul className={`navbar-nav ${styles.navBar}`}>
        <li>
          <NavLink to="/">
            <SuccessButton buttonType="hidden">Reciplex</SuccessButton>
          </NavLink>
        </li>
        {childrenType == "list-elements" && <> {children} </>}
      </ul>
      {(childrenType ?? "second-row") === "second-row" && <> {children} </>}
    </div>
  );
}

export type AppNavigationChildrenType = "second-row" | "list-elements";
