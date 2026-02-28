import { useCallback, useEffect, useState } from 'react';
import { get, post, put } from '@/lib/api-client';
import type { BudgetAlert, AlertSummary } from '@/types/api';

export function useAlerts(planId?: string) {
  const [alerts, setAlerts] = useState<BudgetAlert[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<Error | null>(null);

  useEffect(() => {
    const controller = new AbortController();

    async function fetchAlerts() {
      try {
        setIsLoading(true);
        setError(null);
        const params = planId ? `?planId=${planId}` : '';
        const data = await get<BudgetAlert[]>(`/api/alerts${params}`, {
          signal: controller.signal,
        });
        setAlerts(data);
      } catch (err) {
        if (err instanceof DOMException && err.name === 'AbortError') return;
        setError(err instanceof Error ? err : new Error('Failed to fetch alerts'));
      } finally {
        if (!controller.signal.aborted) {
          setIsLoading(false);
        }
      }
    }

    fetchAlerts();
    return () => controller.abort();
  }, [planId]);

  const refetch = useCallback(() => {
    setError(null);
    setIsLoading(true);
    const params = planId ? `?planId=${planId}` : '';
    get<BudgetAlert[]>(`/api/alerts${params}`)
      .then(setAlerts)
      .catch((err) =>
        setError(err instanceof Error ? err : new Error('Failed to fetch alerts')),
      )
      .finally(() => setIsLoading(false));
  }, [planId]);

  return { alerts, isLoading, error, refetch };
}

export function useAlertSummary() {
  const [summary, setSummary] = useState<AlertSummary | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<Error | null>(null);

  useEffect(() => {
    const controller = new AbortController();

    async function fetchSummary() {
      try {
        setIsLoading(true);
        setError(null);
        const data = await get<AlertSummary>('/api/alerts/summary', {
          signal: controller.signal,
        });
        setSummary(data);
      } catch (err) {
        if (err instanceof DOMException && err.name === 'AbortError') return;
        setError(err instanceof Error ? err : new Error('Failed to fetch alert summary'));
      } finally {
        if (!controller.signal.aborted) {
          setIsLoading(false);
        }
      }
    }

    fetchSummary();
    return () => controller.abort();
  }, []);

  const refetch = useCallback(() => {
    setError(null);
    setIsLoading(true);
    get<AlertSummary>('/api/alerts/summary')
      .then(setSummary)
      .catch((err) =>
        setError(err instanceof Error ? err : new Error('Failed to fetch alert summary')),
      )
      .finally(() => setIsLoading(false));
  }, []);

  return { summary, isLoading, error, refetch };
}

export async function markAlertRead(id: string): Promise<BudgetAlert> {
  return put<BudgetAlert>(`/api/alerts/${id}/read`, {});
}

export async function dismissAlert(id: string): Promise<BudgetAlert> {
  return put<BudgetAlert>(`/api/alerts/${id}/dismiss`, {});
}

export async function generateAlerts(): Promise<BudgetAlert[]> {
  return post<BudgetAlert[]>('/api/alerts/generate');
}
