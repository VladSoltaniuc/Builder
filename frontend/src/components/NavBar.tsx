// Presentation layer — navigation
import { NavLink } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useTheme } from "../context/ThemeContext";

export function NavBar() {
  const { theme, toggle: toggleTheme } = useTheme();
  const { t, i18n } = useTranslation();

  function toggleLanguage() {
    const next = i18n.language === "en" ? "ro" : "en";
    void i18n.changeLanguage(next);
    localStorage.setItem("language", next);
  }

  return (
    <nav className="nav">
      <NavLink to="/products" className={({ isActive }) => isActive ? "nav-link active" : "nav-link"}>
        {t("nav.products")}
      </NavLink>
      <NavLink to="/users" className={({ isActive }) => isActive ? "nav-link active" : "nav-link"}>
        {t("nav.users")}
      </NavLink>
      <NavLink to="/orders" className={({ isActive }) => isActive ? "nav-link active" : "nav-link"}>
        {t("nav.orders")}
      </NavLink>

      <div className="nav-controls">
        <button className="btn btn-small" onClick={toggleLanguage}>
          {i18n.language === "en" ? "🇷🇴 RO" : "🇬🇧 EN"}
        </button>
        <button className="btn btn-small" onClick={toggleTheme}>
          {theme === "dark" ? "Light" : "Dark"}
        </button>
      </div>
    </nav>
  );
}
