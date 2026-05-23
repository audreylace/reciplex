import Paper from "@mui/material/Paper";
import type { PropsWithChildren } from "preact/compat";

/** wrapper for a content section */
export function ContentWrapper({
  topGutter,
  children,
}: PropsWithChildren<IContentWrapper>) {
  return (
    <Paper
      sx={{
        p: 2,
        mt: topGutter
          ? typeof topGutter === "number"
            ? topGutter
            : 2
          : undefined,
      }}
    >
      {children}
    </Paper>
  );
}

/** props for `<ContentWrapper />` */
interface IContentWrapper {
  /** flag if the component should have a space after it */
  topGutter?: boolean | number;
}
