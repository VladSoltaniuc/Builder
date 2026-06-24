// Presentation layer — list view
import type { Product } from "../types/product";
import type { SortState } from "../types/query";
import { toggleSort } from "../types/query";
import { sortIcons } from "../constants/ui";

interface ProductTableProps {
  products: Product[];
  sort: SortState | null;
  onSort: (sort: SortState | null) => void;
  onEdit: (product: Product) => void;
  onDelete: (id: number) => void;
}

const currency = new Intl.NumberFormat("ro-RO", { style: "currency", currency: "RON" });

function SortIcon({ field, sort }: Readonly<{ field: string; sort: SortState | null }>) {
  if (sort?.field !== field) return <span className="sort-icon">{sortIcons.both}</span>;
  return <span className="sort-icon active">{sort.dir === 'ASC' ? sortIcons.asc : sortIcons.desc}</span>;
}

export function ProductTable({ products, sort, onSort, onEdit, onDelete }: Readonly<ProductTableProps>) {
  function handleSort(field: string) {
    onSort(toggleSort(sort, field));
  }

  if (products.length === 0) {
    return <p className="empty">No products. Add one using the form above.</p>;
  }

  return (
    <table className="table">
      <thead>
        <tr>
          <th>#</th>
          <th className="sortable" onClick={() => handleSort('name')}>
            Name <SortIcon field="name" sort={sort} />
          </th>
          <th className="sortable" onClick={() => handleSort('category')}>
            Category <SortIcon field="category" sort={sort} />
          </th>
          <th className="num sortable" onClick={() => handleSort('price')}>
            Price <SortIcon field="price" sort={sort} />
          </th>
          <th className="num sortable" onClick={() => handleSort('stock')}>
            Stock <SortIcon field="stock" sort={sort} />
          </th>
          <th>Actions</th>
        </tr>
      </thead>
      <tbody>
        {products.map((product) => (
          <tr key={product.id}>
            <td>{product.id}</td>
            <td>{product.name}</td>
            <td>{product.category}</td>
            <td className="num">{currency.format(product.price)}</td>
            <td className="num">{product.stock}</td>
            <td>
              <div className="row-actions">
                <button className="btn btn-small" onClick={() => onEdit(product)}>Edit</button>
                <button className="btn btn-small btn-danger" onClick={() => onDelete(product.id)}>Delete</button>
              </div>
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}
