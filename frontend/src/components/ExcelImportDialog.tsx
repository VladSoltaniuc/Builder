// Presentation layer — Excel import dialog
import { useRef, useState } from 'react';
import { createPortal } from 'react-dom';
import toast from 'react-hot-toast';
import { useTranslation } from 'react-i18next';
import { productsApi } from '../api/products';
import { ApiError } from '../api/errors';
import type { ImportProductResult } from '../types/product';

interface Props {
  onClose: () => void;
  onImported: () => void;
}

export function ExcelImportDialog({ onClose, onImported }: Readonly<Props>) {
  const { t } = useTranslation();
  const fileRef = useRef<HTMLInputElement>(null);
  const [result, setResult] = useState<ImportProductResult | null>(null);
  const [isImporting, setIsImporting] = useState(false);

  async function handleImport() {
    const file = fileRef.current?.files?.[0];
    if (!file) {
      toast.error(t('excel.noFileSelected'));
      return;
    }
    setIsImporting(true);
    try {
      const res = await productsApi.importExcel(file);
      setResult(res);
      if (res.imported > 0) {
        toast.success(t('excel.importSuccess', { count: res.imported }));
        onImported();
      }
      if (res.failed === 0) onClose();
    } catch (err) {
      toast.error(err instanceof ApiError
        ? t(`errors.${err.errorCode}`, { columns: err.detail, defaultValue: err.message })
        : t('excel.importError'));
    } finally {
      setIsImporting(false);
    }
  }

  return createPortal(
    <>
      <button className="modal-backdrop" onClick={onClose} aria-label="Close" />
      <dialog className="modal" open>
        <div className="card">
          <h2>{t('excel.importTitle')}</h2>

          {result ? (
            <>
              <p>{t('excel.importResult', { imported: result.imported, failed: result.failed })}</p>
              {result.errors.length > 0 && (
                <ul className="excel-errors">
                  {result.errors.map((e, i) => <li key={i}>{e}</li>)}
                </ul>
              )}
              <div className="form-actions">
                <button className="btn btn-primary" onClick={onClose}>{t('common.close')}</button>
              </div>
            </>
          ) : (
            <>
              <p className="excel-hint">{t('excel.importHint')}</p>
              <input ref={fileRef} type="file" accept=".xlsx" />
              <div className="form-actions">
                <button className="btn btn-primary" onClick={handleImport} disabled={isImporting}>
                  {isImporting ? t('excel.importing') : t('excel.import')}
                </button>
                <button className="btn" onClick={onClose}>{t('form.cancel')}</button>
              </div>
            </>
          )}
        </div>
      </dialog>
    </>,
    document.body,
  );
}
