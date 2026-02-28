import { useCallback, useEffect, useState } from 'react';
import { get } from '@/lib/api-client';
import type { BudgetOverview } from '@/types/api';

export function useBudgetOverview(planId: string) {
  const [overview, setOverview] = useState<BudgetOverview | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<Error | null>(null);

  useEffect(() => {
    const controller = new AbortController();

    async function fetchOverview() {
      try {
        setIsLoading(true);
        setError(null);
        const data = await get<BudgetOverview>(`/api/plans/${planId}/budget-overview`, {
          signal: controller.signal,
        });
        setOverview(data);
      } catch (err) {
        if (err instanceof DOMException && err.name === 'AbortError') return;
        setError(err instanceof Error ? err : new Error('Failed to fetch budget overview'));
      } finally {
        if (!controller.signal.aborted) {
          setIsLoading(false);
        }
      }
    }

    fetchOverview();
    return () => controller.abort();
  }, [planId]);

  const refetch = useCallback(() => {
    setError(null);
    setIsLoading(true);
    get<BudgetOverview>(`/api/plans/${planId}/budget-overview`)
      .then(setOverview)
      .catch((err) =>
        setError(err instanceof Error ? err : new Error('Failed to fetch budget overview')),
      )
      .finally(() => setIsLoading(false));
  }, [planId]);

  return { overview, isLoading, error, refetch };
}
