// Application layer
import { Navigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useAuth } from "../context/AuthContext";

export function ProtectedRoute({
  children,
}: Readonly<{ children: React.ReactNode }>) {
  const { isAuthenticated, isLoading } = useAuth();
  const { t } = useTranslation();

  // Wait for the initial token check before deciding, so a logged-in user
  // isn't bounced to /login on a hard refresh.
  if (isLoading) return <p className="loading">{t("common.loading")}</p>;
  if (!isAuthenticated) return <Navigate to="/login" replace />;
  return <>{children}</>;
}
