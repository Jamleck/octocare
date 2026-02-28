import { useCallback, useEffect, useState } from 'react';
import { get, post, put } from '@/lib/api-client';
import type {
  Provider,
  CreateProviderRequest,
  UpdateProviderRequest,
  PagedResult,
} from '@/types/api';

export function useProviders(page: number = 1, pageSize: number = 20, search?: string) {
  const [data, setData] = useState<PagedResult<Provider> | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<Error | null>(null);

  useEffect(() => {
    const controller = new AbortController();

    async function fetchProviders() {
      try {
        setIsLoading(true);
        setError(null);
        const params = new URLSearchParams({ page: String(page), pageSize: String(pageSize) });
        if (search) params.set('search', search);
        const result = await get<PagedResult<Provider>>(`/api/providers?${params}`, { signal: controller.signal });
        setData(result);
      } catch (err) {
        if (err instanceof DOMException && err.name === 'AbortError') return;
        setError(err instanceof Error ? err : new Error('Failed to fetch providers'));
      } finally {
        if (!controller.signal.aborted) {
          setIsLoading(false);
        }
      }
    }

    fetchProviders();
    return () => controller.abort();
  }, [page, pageSize, search]);

  const refetch = useCallback(() => {
    setError(null);
    setIsLoading(true);
    const params = new URLSearchParams({ page: String(page), pageSize: String(pageSize) });
    if (search) params.set('search', search);
    get<PagedResult<Provider>>(`/api/providers?${params}`)
      .then(setData)
      .catch((err) => setError(err instanceof Error ? err : new Error('Failed to fetch providers')))
      .finally(() => setIsLoading(false));
  }, [page, pageSize, search]);

  return {
    providers: data?.items ?? [],
    totalCount: data?.totalCount ?? 0,
    isLoading,
    error,
    refetch,
  };
}

export function useProvider(id: string) {
  const [provider, setProvider] = useState<Provider | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<Error | null>(null);

  useEffect(() => {
    const controller = new AbortController();

    async function fetchProvider() {
      try {
        setIsLoading(true);
        setError(null);
        const data = await get<Provider>(`/api/providers/${id}`, { signal: controller.signal });
        setProvider(data);
      } catch (err) {
        if (err instanceof DOMException && err.name === 'AbortError') return;
        setError(err instanceof Error ? err : new Error('Failed to fetch provider'));
      } finally {
        if (!controller.signal.aborted) {
          setIsLoading(false);
        }
      }
    }

    fetchProvider();
    return () => controller.abort();
  }, [id]);

  return { provider, isLoading, error };
}

export async function createProvider(request: CreateProviderRequest): Promise<Provider> {
  return post<Provider>('/api/providers', request);
}

export async function updateProvider(id: string, request: UpdateProviderRequest): Promise<Provider> {
  return put<Provider>(`/api/providers/${id}`, request);
}
