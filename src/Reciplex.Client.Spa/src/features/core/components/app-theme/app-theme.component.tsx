import { createTheme, ThemeProvider } from "@mui/material/styles";
import type { ComponentChildren } from "preact";

const outerTheme = createTheme({
  palette: {
    mode: "light",
    primary: {
      main: "#f43f5e",
      dark: "#881337",
      light: "#fecdd3",
    },
    secondary: {
      main: "#f59e0b",
      light: "#fef3c7",
      dark: "#b45309",
    },
    background: {
      default: "#ffe2e2",
      paper: "#fff7ed",
    },
  },
  components: {
    MuiButton: {
      styleOverrides: {
        root: {
          variants: [
            {
              props: { variant: "outlined", color: "primary" },
              style: {
                color: "#881337",
              },
            },
            {
              props: { variant: "text", color: "primary" },
              style: {
                color: "#881337",
              },
            },
            {
              props: { variant: "outlined", color: "secondary" },
              style: {
                color: "#b45309",
              },
            },
            {
              props: { variant: "text", color: "secondary" },
              style: {
                color: "#b45309",
              },
            },
          ],
        },
      },
    },
  },
});

export function AppTheme({ children }: { children: ComponentChildren }) {
  return <ThemeProvider theme={outerTheme}>{children}</ThemeProvider>;
}
