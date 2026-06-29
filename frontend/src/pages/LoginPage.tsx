// Application layer
import { useState, type FormEvent } from "react";
import { Navigate, useNavigate, Link } from "react-router-dom";
import { useTranslation } from "react-i18next";
import toast from "react-hot-toast";
import { authApi } from "../api/auth";
import { useAuth } from "../context/AuthContext";
import { ApiError } from "../api/errors";

export function LoginPage() {
  const { t } = useTranslation();
  const { setSession, isAuthenticated } = useAuth();
  const navigate = useNavigate();

  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [twoFactorToken, setTwoFactorToken] = useState<string | null>(null);
  const [code, setCode] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);

  if (isAuthenticated) return <Navigate to="/products" replace />;

  async function handleLogin(event: FormEvent) {
    event.preventDefault();
    setIsSubmitting(true);
    try {
      const res = await authApi.login(email, password);
      if (res.requiresTwoFactor) {
        setTwoFactorToken(res.twoFactorToken);
      } else if (res.auth) {
        await setSession(res.auth.token);
        navigate("/products");
      }
    } catch (err) {
      toast.error(
        err instanceof ApiError ? err.message : t("auth.loginFailed"),
      );
    } finally {
      setIsSubmitting(false);
    }
  }

  async function handleVerify(event: FormEvent) {
    event.preventDefault();
    setIsSubmitting(true);
    try {
      const auth = await authApi.verifyTwoFactor(twoFactorToken!, code);
      await setSession(auth.token);
      navigate("/products");
    } catch (err) {
      toast.error(
        err instanceof ApiError ? err.message : t("auth.loginFailed"),
      );
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <main className="container">
      <form
        className="card auth-card"
        onSubmit={twoFactorToken ? handleVerify : handleLogin}
      >
        <h1>{t("auth.loginTitle")}</h1>

        {twoFactorToken ? (
          <label>
            {t("auth.code")}
            <input
              name="code"
              value={code}
              onChange={(e) => setCode(e.target.value)}
              inputMode="numeric"
              autoComplete="one-time-code"
              required
              placeholder=" "
            />
          </label>
        ) : (
          <>
            <label>
              {t("auth.email")}
              <input
                name="email"
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
                placeholder=" "
              />
            </label>
            <label>
              {t("auth.password")}
              <input
                name="password"
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                required
                placeholder=" "
              />
            </label>
          </>
        )}

        <div className="form-actions">
          <button
            type="submit"
            className="btn btn-primary"
            disabled={isSubmitting}
          >
            {isSubmitting ? t("form.saving") : t("auth.login")}
          </button>
        </div>

        {!twoFactorToken && (
          <p className="auth-switch">
            {t("auth.noAccount")}{" "}
            <Link to="/register">{t("auth.register")}</Link>
          </p>
        )}
      </form>
    </main>
  );
}
