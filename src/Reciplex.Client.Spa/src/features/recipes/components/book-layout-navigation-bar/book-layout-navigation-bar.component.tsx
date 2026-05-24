import { AppNavigation } from "../../../core/components/app-navigation/app-navigation.component";
import {
  BookLayoutCrumbs,
  type IBookLayoutCrumbsProps,
} from "../book-layout-crumb/book-layout-crumbs.component";

export function BookLayoutNavigationBar(props: IBookLayoutNavigationBarProps) {
  return (
    <>
      <AppNavigation fixed removeLinks removeGrow>
        <BookLayoutCrumbs {...props} />
      </AppNavigation>
      <AppNavigation invisible removeLinks removeGrow>
        <BookLayoutCrumbs {...props} />
      </AppNavigation>
    </>
  );
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface IBookLayoutNavigationBarProps extends IBookLayoutCrumbsProps {}
