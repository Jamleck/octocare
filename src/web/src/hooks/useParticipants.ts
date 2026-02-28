import { useCallback, useEffect, useState } from 'react';
import { get, post, put } from '@/lib/api-client';
import type {
  Participant,
  CreateParticipantRequest,
  UpdateParticipantRequest,
  PagedResult,
} from '@/types/api';

export function useParticipants(page: number = 1, pageSize: number = 20, search?: string) {
  const [data, setData] = useState<PagedResult<Participant> | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<Error | null>(null);

  useEffect(() => {
    const controller = new AbortController();

    async function fetchParticipants() {
      try {
        setIsLoading(true);
        setError(null);
        const params = new URLSearchParams({ page: String(page), pageSize: String(pageSize) });
        if (search) params.set('search', search);
        const result = await get<PagedResult<Participant>>(`/api/participants?${params}`, { signal: controller.signal });
        setData(result);
      } catch (err) {
        if (err instanceof DOMException && err.name === 'AbortError') return;
        setError(err instanceof Error ? err : new Error('Failed to fetch participants'));
      } finally {
        if (!controller.signal.aborted) {
          setIsLoading(false);
        }
      }
    }

    fetchParticipants();
    return () => controller.abort();
  }, [page, pageSize, search]);

  const refetch = useCallback(() => {
    setError(null);
    setIsLoading(true);
    const params = new URLSearchParams({ page: String(page), pageSize: String(pageSize) });
    if (search) params.set('search', search);
    get<PagedResult<Participant>>(`/api/participants?${params}`)
      .then(setData)
      .catch((err) => setError(err instanceof Error ? err : new Error('Failed to fetch participants')))
      .finally(() => setIsLoading(false));
  }, [page, pageSize, search]);

  return {
    participants: data?.items ?? [],
    totalCount: data?.totalCount ?? 0,
    isLoading,
    error,
    refetch,
  };
}

export function useParticipant(id: string) {
  const [participant, setParticipant] = useState<Participant | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<Error | null>(null);

  useEffect(() => {
    const controller = new AbortController();

    async function fetchParticipant() {
      try {
        setIsLoading(true);
        setError(null);
        const data = await get<Participant>(`/api/participants/${id}`, { signal: controller.signal });
        setParticipant(data);
      } catch (err) {
        if (err instanceof DOMException && err.name === 'AbortError') return;
        setError(err instanceof Error ? err : new Error('Failed to fetch participant'));
      } finally {
        if (!controller.signal.aborted) {
          setIsLoading(false);
        }
      }
    }

    fetchParticipant();
    return () => controller.abort();
  }, [id]);

  return { participant, isLoading, error };
}

export async function createParticipant(request: CreateParticipantRequest): Promise<Participant> {
  return post<Participant>('/api/participants', request);
}

export async function updateParticipant(id: string, request: UpdateParticipantRequest): Promise<Participant> {
  return put<Participant>(`/api/participants/${id}`, request);
}

export async function deactivateParticipant(id: string): Promise<void> {
  await post(`/api/participants/${id}/deactivate`);
}
