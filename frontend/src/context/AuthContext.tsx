// Application layer — auth session state
import { createContext, useCallback, useContext, useEffect, useMemo, useState } from "react";
import { authApi } from "../api/auth";
import { getToken, setToken, clearToken, AUTH_LOGOUT_EVENT } from "../auth/token";
import type { Profile } from "../types/auth";

interface AuthContextValue {
  user: Profile | null;
  isLoading: boolean;
  isAuthenticated: boolean;
  // Stores a fresh token, then loads the profile behind it.
  setSession: (token: string) => Promise<void>;
  logout: () => void;
  refresh: () => Promise<void>;
}

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: Readonly<{ children: React.ReactNode }>) {
  const [user, setUser] = useState<Profile | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  const loadProfile = useCallback(async () => {
    if (!getToken()) {
      setUser(null);
      setIsLoading(false);
      return;
    }
    try {
      setUser(await authApi.me());
    } catch {
      // Bad/expired token — drop it.
      clearToken();
      setUser(null);
    } finally {
      setIsLoading(false);
    }
  }, []);

  // Initial session restore from a persisted token.
  useEffect(() => {
    void loadProfile();
  }, [loadProfile]);

  // The transport layer fires this on any 401.
  useEffect(() => {
    function onLogout() {
      clearToken();
      setUser(null);
    }
    window.addEventListener(AUTH_LOGOUT_EVENT, onLogout);
    return () => window.removeEventListener(AUTH_LOGOUT_EVENT, onLogout);
  }, []);

  const setSession = useCallback(async (token: string) => {
    setToken(token);
    setIsLoading(true);
    await loadProfile();
  }, [loadProfile]);

  const logout = useCallback(() => {
    clearToken();
    setUser(null);
  }, []);

  const value = useMemo<AuthContextValue>(() => ({
    user,
    isLoading,
    isAuthenticated: user !== null,
    setSession,
    logout,
    refresh: loadProfile,
  }), [user, isLoading, setSession, logout, loadProfile]);

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth must be used within an AuthProvider");
  return ctx;
}
