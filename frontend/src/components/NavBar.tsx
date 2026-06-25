// Presentation layer — navigation
import { NavLink } from "react-router-dom";
import { useTheme } from "../context/ThemeContext";

export function NavBar() {
  const { theme, toggle } = useTheme();

  return (
    <nav className="nav">
      <NavLink to="/products" className={({ isActive }) => isActive ? "nav-link active" : "nav-link"}>
        Products
      </NavLink>
      <NavLink to="/users" className={({ isActive }) => isActive ? "nav-link active" : "nav-link"}>
        Users
      </NavLink>
      <NavLink to="/orders" className={({ isActive }) => isActive ? "nav-link active" : "nav-link"}>
        Orders
      </NavLink>
      <button className="btn btn-small nav-theme-toggle" onClick={toggle}>
        {theme === "dark" ? "Light" : "Dark"}
      </button>
    </nav>
  );
}
