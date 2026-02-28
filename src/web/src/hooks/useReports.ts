import { useCallback, useState } from 'react';
import { get, downloadBlob } from '@/lib/api-client';
import type {
  BudgetUtilisationReportRow,
  OutstandingInvoiceRow,
  ClaimStatusRow,
  ParticipantSummaryRow,
  AuditTrailRow,
} from '@/types/api';

export type ReportName =
  | 'budget-utilisation'
  | 'outstanding-invoices'
  | 'claim-status'
  | 'participant-summary'
  | 'audit-trail';

export type ReportFormat = 'json' | 'csv' | 'xlsx';

export function useReport<T>(reportName: ReportName) {
  const [data, setData] = useState<T[] | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<Error | null>(null);

  const fetchReport = useCallback(
    async (params?: Record<string, string>) => {
      try {
        setIsLoading(true);
        setError(null);
        const searchParams = new URLSearchParams({ format: 'json', ...params });
        const result = await get<T[]>(`/api/reports/${reportName}?${searchParams}`);
        setData(result);
      } catch (err) {
        setError(err instanceof Error ? err : new Error(`Failed to fetch ${reportName} report`));
      } finally {
        setIsLoading(false);
      }
    },
    [reportName],
  );

  return { data, isLoading, error, fetchReport };
}

export async function downloadReport(
  reportName: ReportName,
  format: 'csv' | 'xlsx',
  params?: Record<string, string>,
): Promise<void> {
  const searchParams = new URLSearchParams({ format, ...params });
  const extension = format === 'csv' ? 'csv' : 'xlsx';
  await downloadBlob(
    `/api/reports/${reportName}?${searchParams}`,
    `${reportName}.${extension}`,
  );
}

// Convenience hooks for specific report types
export function useBudgetUtilisationReport() {
  return useReport<BudgetUtilisationReportRow>('budget-utilisation');
}

export function useOutstandingInvoicesReport() {
  return useReport<OutstandingInvoiceRow>('outstanding-invoices');
}

export function useClaimStatusReport() {
  return useReport<ClaimStatusRow>('claim-status');
}

export function useParticipantSummaryReport() {
  return useReport<ParticipantSummaryRow>('participant-summary');
}

export function useAuditTrailReport() {
  return useReport<AuditTrailRow>('audit-trail');
}
