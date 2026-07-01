// Shared layer - API contract types
export interface Product {
  id: number;
  name: string;
  category: string;
  price: number;
  stock: number;
  version: number;
  imageUrl?: string;
}

// Form fields only - id and version are server-managed
export type ProductInput = Omit<Product, 'id' | 'version'>;

export interface ImportProductResult {
  imported: number;
  failed: number;
  errors: string[];
}
