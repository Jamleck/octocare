import { useCallback, useEffect, useState } from 'react';
import { get, post, put } from '@/lib/api-client';
import type {
  Plan,
  CreatePlanRequest,
  UpdatePlanRequest,
  CreateBudgetCategoryRequest,
  BudgetCategory,
} from '@/types/api';

export function usePlans(participantId: string) {
  const [plans, setPlans] = useState<Plan[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<Error | null>(null);

  const fetchPlans = useCallback(
    (signal?: AbortSignal) => {
      setIsLoading(true);
      setError(null);
      get<Plan[]>(`/api/participants/${participantId}/plans`, { signal })
        .then(setPlans)
        .catch((err) => {
          if (err instanceof DOMException && err.name === 'AbortError') return;
          setError(err instanceof Error ? err : new Error('Failed to fetch plans'));
        })
        .finally(() => setIsLoading(false));
    },
    [participantId],
  );

  useEffect(() => {
    const controller = new AbortController();
    fetchPlans(controller.signal);
    return () => controller.abort();
  }, [fetchPlans]);

  const refetch = useCallback(() => fetchPlans(), [fetchPlans]);

  return { plans, isLoading, error, refetch };
}

export function usePlan(id: string) {
  const [plan, setPlan] = useState<Plan | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<Error | null>(null);

  useEffect(() => {
    const controller = new AbortController();

    async function fetchPlan() {
      try {
        setIsLoading(true);
        setError(null);
        const data = await get<Plan>(`/api/plans/${id}`, { signal: controller.signal });
        setPlan(data);
      } catch (err) {
        if (err instanceof DOMException && err.name === 'AbortError') return;
        setError(err instanceof Error ? err : new Error('Failed to fetch plan'));
      } finally {
        if (!controller.signal.aborted) {
          setIsLoading(false);
        }
      }
    }

    fetchPlan();
    return () => controller.abort();
  }, [id]);

  const refetch = useCallback(() => {
    setError(null);
    setIsLoading(true);
    get<Plan>(`/api/plans/${id}`)
      .then(setPlan)
      .catch((err) => setError(err instanceof Error ? err : new Error('Failed to fetch plan')))
      .finally(() => setIsLoading(false));
  }, [id]);

  return { plan, isLoading, error, refetch };
}

export async function createPlan(participantId: string, request: CreatePlanRequest): Promise<Plan> {
  return post<Plan>(`/api/participants/${participantId}/plans`, request);
}

export async function updatePlan(id: string, request: UpdatePlanRequest): Promise<Plan> {
  return put<Plan>(`/api/plans/${id}`, request);
}

export async function activatePlan(id: string): Promise<Plan> {
  return post<Plan>(`/api/plans/${id}/activate`);
}

export async function addBudgetCategory(
  planId: string,
  request: CreateBudgetCategoryRequest,
): Promise<BudgetCategory> {
  return post<BudgetCategory>(`/api/plans/${planId}/budget-categories`, request);
}

export async function updateBudgetCategory(
  planId: string,
  categoryId: string,
  request: { allocatedAmount: number },
): Promise<BudgetCategory> {
  return put<BudgetCategory>(`/api/plans/${planId}/budget-categories/${categoryId}`, request);
}
