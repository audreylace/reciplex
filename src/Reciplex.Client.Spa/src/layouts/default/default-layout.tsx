import { NavLink, Outlet } from "react-router";
import { AppNavigation } from "../../features/core/components/app-navigation/app-navigation.component";
import { SuccessButton } from "../../features/core/components/buttons/success-button.component";

export function DefaultLayout() {
  return (
    <>
      <AppNavigation childrenType="list-elements">
        <li>
          <NavLink to="/recipe-books">
            <SuccessButton buttonType="hidden">Books</SuccessButton>
          </NavLink>
        </li>
      </AppNavigation>
      <Outlet />
    </>
  );
}
