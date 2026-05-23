import { createTheme, ThemeProvider } from "@mui/material/styles";

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
  typography: {
    h1: {
      fontSize: "3rem",
    },
    h2: {
      fontSize: "2.5rem",
    },
    h3: {
      fontSize: "2rem",
    },
    h4: {
      fontSize: "1.5rem",
    },
    h5: {
      fontSize: "1.2rem",
    },
    h6: {
      fontSize: "1.1rem",
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

export function AppTheme({ children }: { children?: React.ReactNode }) {
  return <ThemeProvider theme={outerTheme}>{children}</ThemeProvider>;
}
