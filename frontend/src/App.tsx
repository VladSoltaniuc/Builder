// Application layer — wiring only
import { BrowserRouter } from "react-router-dom";
import { NavBar } from "./components/NavBar";
import { AppRoutes } from "./routes/AppRoutes";

export function App() {
  return (
    <BrowserRouter>
      <NavBar />
      <AppRoutes />
    </BrowserRouter>
  );
}
