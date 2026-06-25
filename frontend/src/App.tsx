// Application layer — wiring only
import { BrowserRouter } from "react-router-dom";
import { Toaster } from "react-hot-toast";
import { ThemeProvider } from "./context/ThemeContext";
import { MuiThemeWrapper } from "./context/MuiThemeWrapper";
import { NavBar } from "./components/NavBar";
import { AppRoutes } from "./routes/AppRoutes";

export function App() {
  return (
    <ThemeProvider>
      <MuiThemeWrapper>
      <BrowserRouter>
        <Toaster position="top-right" toastOptions={{ duration: 3000 }} />
        <NavBar />
        <AppRoutes />
      </BrowserRouter>
      </MuiThemeWrapper>
    </ThemeProvider>
  );
}
