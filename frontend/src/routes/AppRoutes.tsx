// Application layer — route definitions
import { Navigate, Route, Routes } from "react-router-dom";
import { ProductsPage } from "../pages/ProductsPage";
import { UsersPage } from "../pages/UsersPage";
import { OrdersPage } from "../pages/OrdersPage";

export function AppRoutes() {
  return (
    <Routes>
      <Route path="/" element={<Navigate to="/products" replace />} />
      <Route path="/products" element={<ProductsPage />} />
      <Route path="/users" element={<UsersPage />} />
      <Route path="/orders" element={<OrdersPage />} />
    </Routes>
  );
}
