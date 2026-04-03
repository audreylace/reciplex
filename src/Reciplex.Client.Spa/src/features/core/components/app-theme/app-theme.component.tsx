import { createTheme, ThemeProvider } from "@mui/material/styles";
import type { ComponentChildren } from "preact";

const outerTheme = createTheme({
  palette: {},
});

export function AppTheme({ children }: { children: ComponentChildren }) {
  return <ThemeProvider theme={outerTheme}>{children}</ThemeProvider>;
}
