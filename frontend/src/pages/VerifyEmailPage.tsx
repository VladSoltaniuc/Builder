// Application layer
import { useEffect, useRef, useState } from "react";
import { Link, useNavigate, useSearchParams } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { authApi } from "../api/auth";
import { ApiError } from "../api/errors";

type Status = "verifying" | "success" | "error";

export function VerifyEmailPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [params] = useSearchParams();
  const token = params.get("token");

  const [status, setStatus] = useState<Status>("verifying");
  const [errorMessage, setErrorMessage] = useState("");
  // StrictMode mounts effects twice in dev; guard so we only verify once
  const startedRef = useRef(false);

  useEffect(() => {
    if (startedRef.current) return;
    startedRef.current = true;

    if (!token) {
      setStatus("error");
      setErrorMessage(t("auth.verifyMissingToken"));
      return;
    }

    authApi
      .verifyEmail(token)
      .then(() => setStatus("success"))
      .catch((err) => {
        setStatus("error");
        setErrorMessage(
          err instanceof ApiError ? err.message : t("auth.verifyFailed"),
        );
      });
  }, [token, t]);

  // On success, hold the message briefly then send them to login
  useEffect(() => {
    if (status !== "success") return;
    const timer = setTimeout(() => navigate("/login"), 3000);
    return () => clearTimeout(timer);
  }, [status, navigate]);

  return (
    <main className="container">
      <div className="card auth-card">
        {status === "verifying" && <h1>{t("auth.verifying")}</h1>}

        {status === "success" && (
          <>
            <h1>🎉 {t("auth.verifiedTitle")}</h1>
            <p>{t("auth.verifiedRedirecting")}</p>
          </>
        )}

        {status === "error" && (
          <>
            <h1>{t("auth.verifyFailedTitle")}</h1>
            <p className="error">{errorMessage}</p>
            <p className="auth-switch">
              <Link to="/register">{t("auth.register")}</Link> ·{" "}
              <Link to="/login">{t("auth.login")}</Link>
            </p>
          </>
        )}
      </div>
    </main>
  );
}
