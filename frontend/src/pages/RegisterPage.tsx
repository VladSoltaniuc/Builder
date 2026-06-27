// Application layer — sign up
import { useState, type FormEvent } from "react";
import { Navigate, useNavigate, Link } from "react-router-dom";
import { useTranslation } from "react-i18next";
import toast from "react-hot-toast";
import { authApi } from "../api/auth";
import { useAuth } from "../context/AuthContext";
import { ApiError } from "../api/errors";

export function RegisterPage() {
  const { t } = useTranslation();
  const { setSession, isAuthenticated } = useAuth();
  const navigate = useNavigate();

  const [name, setName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);

  if (isAuthenticated) return <Navigate to="/products" replace />;

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setIsSubmitting(true);
    try {
      const res = await authApi.register(name, email, password);
      await setSession(res.token);
      navigate("/products");
    } catch (err) {
      toast.error(err instanceof ApiError ? err.message : t("auth.registerFailed"));
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <main className="container">
      <form className="card auth-card" onSubmit={handleSubmit}>
        <h1>{t("auth.registerTitle")}</h1>

        <label>
          {t("auth.name")}
          <input name="name" value={name} onChange={(e) => setName(e.target.value)} required minLength={2} placeholder=" " />
        </label>
        <label>
          {t("auth.email")}
          <input name="email" type="email" value={email} onChange={(e) => setEmail(e.target.value)} required placeholder=" " />
        </label>
        <label>
          {t("auth.password")}
          <input
            name="password"
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
            minLength={8}
            placeholder=" "
          />
        </label>

        <div className="form-actions">
          <button type="submit" className="btn btn-primary" disabled={isSubmitting}>
            {isSubmitting ? t("form.saving") : t("auth.register")}
          </button>
        </div>

        <p className="auth-switch">
          {t("auth.haveAccount")} <Link to="/login">{t("auth.login")}</Link>
        </p>
      </form>
    </main>
  );
}
