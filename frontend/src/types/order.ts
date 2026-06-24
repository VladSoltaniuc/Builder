// Shared layer — API contract types
export interface Order {
  id: number;
  userId: number;
  userName: string;
  productId: number;
  productName: string;
  quantity: number;
  totalPrice: number;
  status: string;
  createdAt: string;
  version: number;
}

export interface OrderInput {
  userId: number;
  productId: number;
  quantity: number;
}

export interface OrderUpdateInput {
  quantity: number;
  status: string;
  version: number;
}
