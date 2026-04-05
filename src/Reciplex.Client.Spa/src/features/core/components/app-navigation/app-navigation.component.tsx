import { NavLink } from "react-router";
import { SuccessButton } from "../buttons/success-button.component";
import styles from "./app-navigation.module.css";
import type { PropsWithChildren } from "preact/compat";
import { useSignInNavigate } from "../../../auth/hooks/useSignInNavigate.hook";

export function AppNavigation({
  children,
  childrenType,
}: PropsWithChildren<{ childrenType?: AppNavigationChildrenType }>) {
  const signInPath = useSignInNavigate()[0];
  return (
    <div className={`${styles.appNavigationDiv}`}>
      <div className={styles.navBarInnerDiv}>
        <ul className={`navbar-nav ${styles.navBar}`}>
          <li>
            <NavLink to="/">
              <SuccessButton buttonType="hidden">Reciplex</SuccessButton>
            </NavLink>
          </li>
          {childrenType == "list-elements" && <> {children} </>}
          <li className={styles.grow}></li>
        </ul>
        <div className={styles.accountIcon}>
          <NavLink to={signInPath}>
            <SuccessButton buttonType="hidden">
              <i className="bi bi-person-circle"></i>
            </SuccessButton>
          </NavLink>
        </div>
      </div>
      {(childrenType ?? "second-row") === "second-row" && <> {children} </>}
    </div>
  );
}

export type AppNavigationChildrenType = "second-row" | "list-elements";
