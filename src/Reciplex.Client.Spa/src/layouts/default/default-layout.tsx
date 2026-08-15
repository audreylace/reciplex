import { Outlet } from "react-router";
import { AppNavigation } from "../../features/core/components/app-navigation/app-navigation.component";
import { NavigationInProgressIndicatorComponent } from "../../features/core/components/navigation-in-progress-indicator/navigation-in-progress-indicator.component";

export function DefaultLayout() {
  return (
    <>
      <AppNavigation fixed />
      <AppNavigation invisible />
      <NavigationInProgressIndicatorComponent />
      <main className="pageMain">
        <Outlet />
      </main>
    </>
  );
}
