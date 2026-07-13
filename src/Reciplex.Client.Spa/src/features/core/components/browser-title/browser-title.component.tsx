import { useSetBrowserTitle } from "./useSetBrowserTitle.hook";

/** sets the browser title */
export function BrowserTitle({ title }: IBrowserTitleProps) {
  useSetBrowserTitle(title);
  return null;
}

/** props for `BrowserTitle` */
export interface IBrowserTitleProps {
  /** the title of the browser */
  title: string;
}
