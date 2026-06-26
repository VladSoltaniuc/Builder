// Presentation layer — Excel export column picker
import { useState } from 'react';
import { createPortal } from 'react-dom';
import toast from 'react-hot-toast';
import { useTranslation } from 'react-i18next';
import { productsApi } from '../api/products';
import { ApiError } from '../api/errors';

type ColumnKey = 'id' | 'name' | 'category' | 'price' | 'stock';

const COLUMNS: { key: ColumnKey; labelKey: string }[] = [
  { key: 'id',       labelKey: 'table.id' },
  { key: 'name',     labelKey: 'table.name' },
  { key: 'category', labelKey: 'table.category' },
  { key: 'price',    labelKey: 'table.price' },
  { key: 'stock',    labelKey: 'table.stock' },
];

interface Props {
  onClose: () => void;
}

export function ExcelExportDialog({ onClose }: Readonly<Props>) {
  const { t } = useTranslation();
  const [selected, setSelected] = useState<Set<ColumnKey>>(
    new Set(COLUMNS.map((c) => c.key)),
  );
  const [isExporting, setIsExporting] = useState(false);

  function toggle(key: ColumnKey) {
    setSelected((prev) => {
      const next = new Set(prev);
      next.has(key) ? next.delete(key) : next.add(key);
      return next;
    });
  }

  async function handleExport() {
    setIsExporting(true);
    try {
      const cols = COLUMNS.filter((c) => selected.has(c.key)).map((c) => c.key);
      const blob = await productsApi.exportExcel(cols);
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = 'products.xlsx';
      a.click();
      URL.revokeObjectURL(url);
      onClose();
    } catch (err) {
      toast.error(err instanceof ApiError ? err.message : t('excel.exportError'));
    } finally {
      setIsExporting(false);
    }
  }

  return createPortal(
    <>
      <button className="modal-backdrop" onClick={onClose} aria-label="Close" />
      <dialog className="modal" open>
        <div className="card">
          <h2>{t('excel.exportTitle')}</h2>
          <p className="excel-hint">{t('excel.selectColumns')}</p>
          <div className="excel-columns">
            {COLUMNS.map((col) => (
              <div key={col.key} className="excel-col-label">
                <input
                  type="checkbox"
                  id={`col-${col.key}`}
                  checked={selected.has(col.key)}
                  onChange={() => toggle(col.key)}
                />
                <label htmlFor={`col-${col.key}`}>{t(col.labelKey)}</label>
              </div>
            ))}
          </div>
          <div className="form-actions">
            <button className="btn btn-primary" onClick={handleExport} disabled={isExporting || selected.size === 0}>
              {isExporting ? t('excel.exporting') : t('excel.export')}
            </button>
            <button className="btn" onClick={onClose}>{t('form.cancel')}</button>
          </div>
        </div>
      </dialog>
    </>,
    document.body,
  );
}
