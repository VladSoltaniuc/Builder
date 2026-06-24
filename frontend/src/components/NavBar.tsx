// Presentation layer — navigation
import { NavLink } from "react-router-dom";

export function NavBar() {
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
    </nav>
  );
}
