import { createTheme, ThemeProvider as MuiThemeProvider } from "@mui/material";
import { useTheme } from "./ThemeContext";

export function MuiThemeWrapper({
  children,
}: Readonly<{ children: React.ReactNode }>) {
  const { theme } = useTheme();

  const muiTheme = createTheme({
    palette: {
      mode: theme,
      primary: { main: "#3b82f6" },
      error: { main: "#ef4444" },
    },
  });

  return <MuiThemeProvider theme={muiTheme}>{children}</MuiThemeProvider>;
}
