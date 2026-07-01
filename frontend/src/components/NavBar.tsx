// Presentation layer - navigation
import { NavLink, useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import ReactCountryFlag from "react-country-flag";
import { useTheme } from "../context/ThemeContext";
import { useAuth } from "../context/AuthContext";

export function NavBar() {
  const { theme, toggle: toggleTheme } = useTheme();
  const { t, i18n } = useTranslation();
  const { isAuthenticated, user, logout } = useAuth();
  const navigate = useNavigate();

  function toggleLanguage() {
    const next = i18n.language === "en" ? "ro" : "en";
    void i18n.changeLanguage(next);
    localStorage.setItem("language", next);
  }

  function handleLogout() {
    logout();
    navigate("/login");
  }

  const linkClass = ({ isActive }: { isActive: boolean }) =>
    isActive ? "nav-link active" : "nav-link";

  return (
    <nav className="nav">
      {isAuthenticated && (
        <>
          <NavLink to="/products" className={linkClass}>
            {t("nav.products")}
          </NavLink>
          <NavLink to="/users" className={linkClass}>
            {t("nav.users")}
          </NavLink>
          <NavLink to="/orders" className={linkClass}>
            {t("nav.orders")}
          </NavLink>
          <NavLink to="/profile" className={linkClass}>
            {t("nav.profile")}
          </NavLink>
        </>
      )}

      <div className="nav-controls">
        <button className="btn btn-small nav-btn-flag" onClick={toggleLanguage}>
          <ReactCountryFlag
            countryCode={i18n.language === "en" ? "GB" : "RO"}
            svg
            style={{ width: "1.2em", height: "1.2em" }}
          />
          {i18n.language === "en" ? "EN" : "RO"}
        </button>
        <button className="btn btn-small nav-btn-icon" onClick={toggleTheme}>
          {theme === "dark" ? (
            <svg
              width="14"
              height="14"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth="2"
              strokeLinecap="round"
            >
              <path d="M21 12.79A9 9 0 1 1 11.21 3 7 7 0 0 0 21 12.79z" />
            </svg>
          ) : (
            <svg
              width="14"
              height="14"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth="2"
              strokeLinecap="round"
            >
              <circle cx="12" cy="12" r="5" />
              <line x1="12" y1="1" x2="12" y2="3" />
              <line x1="12" y1="21" x2="12" y2="23" />
              <line x1="4.22" y1="4.22" x2="5.64" y2="5.64" />
              <line x1="18.36" y1="18.36" x2="19.78" y2="19.78" />
              <line x1="1" y1="12" x2="3" y2="12" />
              <line x1="21" y1="12" x2="23" y2="12" />
              <line x1="4.22" y1="19.78" x2="5.64" y2="18.36" />
              <line x1="18.36" y1="5.64" x2="19.78" y2="4.22" />
            </svg>
          )}
          {theme === "dark" ? "Dark" : "Light"}
        </button>

        {isAuthenticated ? (
          <>
            <span className="nav-user">{user?.name}</span>
            <button className="btn btn-small" onClick={handleLogout}>
              {t("auth.logout")}
            </button>
          </>
        ) : (
          <NavLink to="/login" className="btn btn-small">
            {t("auth.login")}
          </NavLink>
        )}
      </div>
    </nav>
  );
}
