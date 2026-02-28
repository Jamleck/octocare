import { useCallback, useEffect, useState } from 'react';
import { get, post } from '@/lib/api-client';
import type { PaymentBatchDetail, PaymentBatchPagedResult } from '@/types/api';

const API_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5000';

export function usePayments(page: number = 1, pageSize: number = 20, status?: string) {
  const [data, setData] = useState<PaymentBatchPagedResult | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<Error | null>(null);

  useEffect(() => {
    const controller = new AbortController();

    async function fetchPayments() {
      try {
        setIsLoading(true);
        setError(null);
        const params = new URLSearchParams({ page: String(page), pageSize: String(pageSize) });
        if (status) params.set('status', status);
        const result = await get<PaymentBatchPagedResult>(`/api/payments?${params}`, {
          signal: controller.signal,
        });
        setData(result);
      } catch (err) {
        if (err instanceof DOMException && err.name === 'AbortError') return;
        setError(err instanceof Error ? err : new Error('Failed to fetch payments'));
      } finally {
        if (!controller.signal.aborted) {
          setIsLoading(false);
        }
      }
    }

    fetchPayments();
    return () => controller.abort();
  }, [page, pageSize, status]);

  const refetch = useCallback(() => {
    setError(null);
    setIsLoading(true);
    const params = new URLSearchParams({ page: String(page), pageSize: String(pageSize) });
    if (status) params.set('status', status);
    get<PaymentBatchPagedResult>(`/api/payments?${params}`)
      .then(setData)
      .catch((err) => setError(err instanceof Error ? err : new Error('Failed to fetch payments')))
      .finally(() => setIsLoading(false));
  }, [page, pageSize, status]);

  return {
    payments: data?.items ?? [],
    totalCount: data?.totalCount ?? 0,
    isLoading,
    error,
    refetch,
  };
}

export function usePayment(id: string) {
  const [payment, setPayment] = useState<PaymentBatchDetail | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<Error | null>(null);

  useEffect(() => {
    const controller = new AbortController();

    async function fetchPayment() {
      try {
        setIsLoading(true);
        setError(null);
        const data = await get<PaymentBatchDetail>(`/api/payments/${id}`, {
          signal: controller.signal,
        });
        setPayment(data);
      } catch (err) {
        if (err instanceof DOMException && err.name === 'AbortError') return;
        setError(err instanceof Error ? err : new Error('Failed to fetch payment'));
      } finally {
        if (!controller.signal.aborted) {
          setIsLoading(false);
        }
      }
    }

    fetchPayment();
    return () => controller.abort();
  }, [id]);

  const refetch = useCallback(() => {
    setError(null);
    setIsLoading(true);
    get<PaymentBatchDetail>(`/api/payments/${id}`)
      .then(setPayment)
      .catch((err) => setError(err instanceof Error ? err : new Error('Failed to fetch payment')))
      .finally(() => setIsLoading(false));
  }, [id]);

  return { payment, isLoading, error, refetch };
}

export async function createPaymentBatch(): Promise<PaymentBatchDetail> {
  return post<PaymentBatchDetail>('/api/payments');
}

export async function markPaymentSent(id: string): Promise<PaymentBatchDetail> {
  return post<PaymentBatchDetail>(`/api/payments/${id}/send`);
}

export async function markPaymentConfirmed(id: string): Promise<PaymentBatchDetail> {
  return post<PaymentBatchDetail>(`/api/payments/${id}/confirm`);
}

export function getPaymentAbaUrl(id: string): string {
  return `${API_URL}/api/payments/${id}/aba`;
}
