/// <reference types="vite/client" />

// Tipăm variabilele de environment ca să avem autocomplete + type-safety pe import.meta.env.
interface ImportMetaEnv {
  readonly VITE_API_BASE_URL: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}
