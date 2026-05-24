import Box from "@mui/material/Box";

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
  children?: React.ReactNode;
}
