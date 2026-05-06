import Box from "@mui/material/Box";
import type { ComponentChildren } from "preact";

export function MaybeRenderInvisible({
  children,
  invisible,
}: IMaybeRenderInvisibleProps) {
  if (!invisible) {
    return <Box>{children}</Box>;
  }
  return <Box sx={{ visibility: "hidden" }}>{children}</Box>;
}

export interface IMaybeRenderInvisibleProps {
  invisible?: boolean;
  children: ComponentChildren;
}
