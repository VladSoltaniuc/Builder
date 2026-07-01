// Application layer
import { Navigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useAuth } from "../context/AuthContext";
import { hasFeature } from "../constants/features";

export function ProtectedRoute({
  children,
  feature,
}: Readonly<{ children: React.ReactNode; feature?: number }>) {
  const { user, isAuthenticated, isLoading } = useAuth();
  const { t } = useTranslation();

  // Wait for the initial token check before deciding, so a logged-in user
  // isn't bounced to /login on a hard refresh
  if (isLoading) return <p className="loading">{t("common.loading")}</p>;
  if (!isAuthenticated) return <Navigate to="/login" replace />;
  // Admins pass any gate; operators need the feature bit. Bounce the rest home
  if (feature !== undefined && !hasFeature(user, feature)) return <Navigate to="/" replace />;
  return <>{children}</>;
}
