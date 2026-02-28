import { useCallback, useEffect, useState } from 'react';
import { get, post } from '@/lib/api-client';
import type {
  Claim,
  ClaimPagedResult,
  CreateClaimRequest,
  RecordClaimOutcomeRequest,
} from '@/types/api';

const API_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5000';

export function useClaims(page: number = 1, pageSize: number = 20, status?: string) {
  const [data, setData] = useState<ClaimPagedResult | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<Error | null>(null);

  useEffect(() => {
    const controller = new AbortController();

    async function fetchClaims() {
      try {
        setIsLoading(true);
        setError(null);
        const params = new URLSearchParams({ page: String(page), pageSize: String(pageSize) });
        if (status) params.set('status', status);
        const result = await get<ClaimPagedResult>(`/api/claims?${params}`, {
          signal: controller.signal,
        });
        setData(result);
      } catch (err) {
        if (err instanceof DOMException && err.name === 'AbortError') return;
        setError(err instanceof Error ? err : new Error('Failed to fetch claims'));
      } finally {
        if (!controller.signal.aborted) {
          setIsLoading(false);
        }
      }
    }

    fetchClaims();
    return () => controller.abort();
  }, [page, pageSize, status]);

  const refetch = useCallback(() => {
    setError(null);
    setIsLoading(true);
    const params = new URLSearchParams({ page: String(page), pageSize: String(pageSize) });
    if (status) params.set('status', status);
    get<ClaimPagedResult>(`/api/claims?${params}`)
      .then(setData)
      .catch((err) => setError(err instanceof Error ? err : new Error('Failed to fetch claims')))
      .finally(() => setIsLoading(false));
  }, [page, pageSize, status]);

  return {
    claims: data?.items ?? [],
    totalCount: data?.totalCount ?? 0,
    isLoading,
    error,
    refetch,
  };
}

export function useClaim(id: string) {
  const [claim, setClaim] = useState<Claim | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<Error | null>(null);

  useEffect(() => {
    const controller = new AbortController();

    async function fetchClaim() {
      try {
        setIsLoading(true);
        setError(null);
        const data = await get<Claim>(`/api/claims/${id}`, { signal: controller.signal });
        setClaim(data);
      } catch (err) {
        if (err instanceof DOMException && err.name === 'AbortError') return;
        setError(err instanceof Error ? err : new Error('Failed to fetch claim'));
      } finally {
        if (!controller.signal.aborted) {
          setIsLoading(false);
        }
      }
    }

    fetchClaim();
    return () => controller.abort();
  }, [id]);

  const refetch = useCallback(() => {
    setError(null);
    setIsLoading(true);
    get<Claim>(`/api/claims/${id}`)
      .then(setClaim)
      .catch((err) => setError(err instanceof Error ? err : new Error('Failed to fetch claim')))
      .finally(() => setIsLoading(false));
  }, [id]);

  return { claim, isLoading, error, refetch };
}

export async function createClaim(request: CreateClaimRequest): Promise<Claim> {
  return post<Claim>('/api/claims', request);
}

export async function submitClaim(id: string): Promise<Claim> {
  return post<Claim>(`/api/claims/${id}/submit`);
}

export async function recordClaimOutcome(
  id: string,
  request: RecordClaimOutcomeRequest,
): Promise<Claim> {
  return post<Claim>(`/api/claims/${id}/outcome`, request);
}

export function getClaimCsvUrl(id: string): string {
  return `${API_URL}/api/claims/${id}/csv`;
}
