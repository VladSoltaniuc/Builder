// ListView - Presentation layer
import type { Product } from "../types/product";

interface ProductTableProps {
  products: Product[];
  onEdit: (product: Product) => void;
  onDelete: (id: number) => void;
}

// Formatăm prețul ca monedă românească. Intl este nativ în browser - fără librării.
const currency = new Intl.NumberFormat("ro-RO", {
  style: "currency",
  currency: "RON",
});

/**
 * Componentă "de prezentare" (presentational): primește date prin props și
 * notifică părintele prin callback-uri. Nu știe nimic despre API sau state global.
 */
export function ProductTable({
  products,
  onEdit,
  onDelete,
}: ProductTableProps) {
  if (products.length === 0) {
    return (
      <p className="empty">
        Niciun produs. Adaugă unul folosind formularul de mai sus.
      </p>
    );
  }

  return (
    <table className="table">
      <thead>
        <tr>
          <th>#</th>
          <th>Nume</th>
          <th>Categorie</th>
          <th className="num">Preț</th>
          <th className="num">Stoc</th>
          <th>Acțiuni</th>
        </tr>
      </thead>
      <tbody>
        {/* key={product.id} ajută React să identifice fiecare rând eficient la re-render. */}
        {products.map((product) => (
          <tr key={product.id}>
            <td>{product.id}</td>
            <td>{product.name}</td>
            <td>{product.category}</td>
            <td className="num">{currency.format(product.price)}</td>
            <td className="num">{product.stock}</td>
            <td>
              <div className="row-actions">
                <button
                  className="btn btn-small"
                  onClick={() => onEdit(product)}
                >
                  Editează
                </button>
                <button
                  className="btn btn-small btn-danger"
                  onClick={() => onDelete(product.id)}
                >
                  Șterge
                </button>
              </div>
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}
