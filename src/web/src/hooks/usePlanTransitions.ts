import { useCallback, useEffect, useState } from 'react';
import { get, post, put } from '@/lib/api-client';
import type {
  PlanTransition,
  InitiateTransitionRequest,
  UpdateTransitionRequest,
} from '@/types/api';

export function usePlanTransitions(planId?: string) {
  const [transitions, setTransitions] = useState<PlanTransition[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<Error | null>(null);

  useEffect(() => {
    const controller = new AbortController();

    async function fetchTransitions() {
      try {
        setIsLoading(true);
        setError(null);
        const params = planId ? `?planId=${planId}` : '';
        const data = await get<PlanTransition[]>(`/api/plan-transitions${params}`, {
          signal: controller.signal,
        });
        setTransitions(data);
      } catch (err) {
        if (err instanceof DOMException && err.name === 'AbortError') return;
        setError(err instanceof Error ? err : new Error('Failed to fetch transitions'));
      } finally {
        if (!controller.signal.aborted) {
          setIsLoading(false);
        }
      }
    }

    fetchTransitions();
    return () => controller.abort();
  }, [planId]);

  const refetch = useCallback(() => {
    setError(null);
    setIsLoading(true);
    const params = planId ? `?planId=${planId}` : '';
    get<PlanTransition[]>(`/api/plan-transitions${params}`)
      .then(setTransitions)
      .catch((err) =>
        setError(err instanceof Error ? err : new Error('Failed to fetch transitions')),
      )
      .finally(() => setIsLoading(false));
  }, [planId]);

  return { transitions, isLoading, error, refetch };
}

export async function initiateTransition(
  request: InitiateTransitionRequest,
): Promise<PlanTransition> {
  return post<PlanTransition>('/api/plan-transitions', request);
}

export async function updateTransition(
  id: string,
  request: UpdateTransitionRequest,
): Promise<PlanTransition> {
  return put<PlanTransition>(`/api/plan-transitions/${id}`, request);
}

export async function completeTransition(id: string): Promise<PlanTransition> {
  return post<PlanTransition>(`/api/plan-transitions/${id}/complete`);
}
