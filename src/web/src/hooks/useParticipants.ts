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

  const fetchParticipants = useCallback(async () => {
    try {
      setIsLoading(true);
      setError(null);
      const params = new URLSearchParams({ page: String(page), pageSize: String(pageSize) });
      if (search) params.set('search', search);
      const result = await get<PagedResult<Participant>>(`/api/participants?${params}`);
      setData(result);
    } catch (err) {
      setError(err instanceof Error ? err : new Error('Failed to fetch participants'));
    } finally {
      setIsLoading(false);
    }
  }, [page, pageSize, search]);

  useEffect(() => {
    fetchParticipants();
  }, [fetchParticipants]);

  return {
    participants: data?.items ?? [],
    totalCount: data?.totalCount ?? 0,
    isLoading,
    error,
    refetch: fetchParticipants,
  };
}

export function useParticipant(id: string) {
  const [participant, setParticipant] = useState<Participant | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<Error | null>(null);

  useEffect(() => {
    async function fetch() {
      try {
        setIsLoading(true);
        setError(null);
        const data = await get<Participant>(`/api/participants/${id}`);
        setParticipant(data);
      } catch (err) {
        setError(err instanceof Error ? err : new Error('Failed to fetch participant'));
      } finally {
        setIsLoading(false);
      }
    }
    fetch();
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
