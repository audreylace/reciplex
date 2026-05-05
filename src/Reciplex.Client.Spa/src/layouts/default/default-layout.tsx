import { Outlet } from "react-router";
import { AppNavigation } from "../../features/core/components/app-navigation/app-navigation.component";

export function DefaultLayout() {
  return (
    <>
      <AppNavigation fixed />
      <AppNavigation invisible />
      <main className="pageMain">
        <Outlet />
      </main>
    </>
  );
}
