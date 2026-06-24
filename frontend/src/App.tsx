// Application layer — routing only
import { BrowserRouter, NavLink, Navigate, Route, Routes } from "react-router-dom";
import { ProductsPage } from "./pages/ProductsPage";
import { UsersPage } from "./pages/UsersPage";
import { OrdersPage } from "./pages/OrdersPage";

export function App() {
  return (
    <BrowserRouter>
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

      <Routes>
        <Route path="/" element={<Navigate to="/products" replace />} />
        <Route path="/products" element={<ProductsPage />} />
        <Route path="/users" element={<UsersPage />} />
        <Route path="/orders" element={<OrdersPage />} />
      </Routes>
    </BrowserRouter>
  );
}
