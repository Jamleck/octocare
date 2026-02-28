import { useCallback, useEffect, useState } from 'react';
import { get, post } from '@/lib/api-client';
import type {
  Invoice,
  InvoicePagedResult,
  CreateInvoiceRequest,
} from '@/types/api';

export function useInvoices(
  page: number = 1,
  pageSize: number = 20,
  status?: string,
  participantId?: string,
  providerId?: string,
) {
  const [data, setData] = useState<InvoicePagedResult | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<Error | null>(null);

  useEffect(() => {
    const controller = new AbortController();

    async function fetchInvoices() {
      try {
        setIsLoading(true);
        setError(null);
        const params = new URLSearchParams({ page: String(page), pageSize: String(pageSize) });
        if (status) params.set('status', status);
        if (participantId) params.set('participantId', participantId);
        if (providerId) params.set('providerId', providerId);
        const result = await get<InvoicePagedResult>(`/api/invoices?${params}`, {
          signal: controller.signal,
        });
        setData(result);
      } catch (err) {
        if (err instanceof DOMException && err.name === 'AbortError') return;
        setError(err instanceof Error ? err : new Error('Failed to fetch invoices'));
      } finally {
        if (!controller.signal.aborted) {
          setIsLoading(false);
        }
      }
    }

    fetchInvoices();
    return () => controller.abort();
  }, [page, pageSize, status, participantId, providerId]);

  const refetch = useCallback(() => {
    setError(null);
    setIsLoading(true);
    const params = new URLSearchParams({ page: String(page), pageSize: String(pageSize) });
    if (status) params.set('status', status);
    if (participantId) params.set('participantId', participantId);
    if (providerId) params.set('providerId', providerId);
    get<InvoicePagedResult>(`/api/invoices?${params}`)
      .then(setData)
      .catch((err) => setError(err instanceof Error ? err : new Error('Failed to fetch invoices')))
      .finally(() => setIsLoading(false));
  }, [page, pageSize, status, participantId, providerId]);

  return {
    invoices: data?.items ?? [],
    totalCount: data?.totalCount ?? 0,
    isLoading,
    error,
    refetch,
  };
}

export function useInvoice(id: string) {
  const [invoice, setInvoice] = useState<Invoice | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<Error | null>(null);

  useEffect(() => {
    const controller = new AbortController();

    async function fetchInvoice() {
      try {
        setIsLoading(true);
        setError(null);
        const data = await get<Invoice>(`/api/invoices/${id}`, { signal: controller.signal });
        setInvoice(data);
      } catch (err) {
        if (err instanceof DOMException && err.name === 'AbortError') return;
        setError(err instanceof Error ? err : new Error('Failed to fetch invoice'));
      } finally {
        if (!controller.signal.aborted) {
          setIsLoading(false);
        }
      }
    }

    fetchInvoice();
    return () => controller.abort();
  }, [id]);

  const refetch = useCallback(() => {
    setError(null);
    setIsLoading(true);
    get<Invoice>(`/api/invoices/${id}`)
      .then(setInvoice)
      .catch((err) => setError(err instanceof Error ? err : new Error('Failed to fetch invoice')))
      .finally(() => setIsLoading(false));
  }, [id]);

  return { invoice, isLoading, error, refetch };
}

export async function createInvoice(request: CreateInvoiceRequest): Promise<Invoice> {
  return post<Invoice>('/api/invoices', request);
}

export async function approveInvoice(id: string): Promise<Invoice> {
  return post<Invoice>(`/api/invoices/${id}/approve`);
}

export async function rejectInvoice(id: string, reason: string): Promise<Invoice> {
  return post<Invoice>(`/api/invoices/${id}/reject`, { reason });
}

export async function disputeInvoice(id: string, reason: string): Promise<Invoice> {
  return post<Invoice>(`/api/invoices/${id}/dispute`, { reason });
}

export async function markInvoicePaid(id: string): Promise<Invoice> {
  return post<Invoice>(`/api/invoices/${id}/mark-paid`);
}
