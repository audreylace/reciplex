import { NavLink, Outlet } from "react-router";
import { AppNavigation } from "../../features/core/components/app-navigation/app-navigation.component";
import { SuccessButton } from "../../features/core/components/buttons/success-button.component";
import { useRehydrateActiveUser } from "../../features/auth/hooks/useRehydrateActiveUser.hook";

export function DefaultLayout() {
  useRehydrateActiveUser();
  return (
    <>
      <AppNavigation childrenType="list-elements">
        <li>
          <NavLink to="/books">
            <SuccessButton buttonType="hidden">Books</SuccessButton>
          </NavLink>
        </li>
      </AppNavigation>
      <Outlet />
    </>
  );
}
