import { useCallback, useEffect, useState } from 'react';
import { get, post } from '@/lib/api-client';
import type { ParticipantStatement, GenerateStatementRequest } from '@/types/api';

const API_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5000';

export function useStatements(participantId: string) {
  const [statements, setStatements] = useState<ParticipantStatement[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<Error | null>(null);

  useEffect(() => {
    const controller = new AbortController();

    async function fetchStatements() {
      try {
        setIsLoading(true);
        setError(null);
        const data = await get<ParticipantStatement[]>(
          `/api/participants/${participantId}/statements`,
          { signal: controller.signal },
        );
        setStatements(data);
      } catch (err) {
        if (err instanceof DOMException && err.name === 'AbortError') return;
        setError(err instanceof Error ? err : new Error('Failed to fetch statements'));
      } finally {
        if (!controller.signal.aborted) {
          setIsLoading(false);
        }
      }
    }

    fetchStatements();
    return () => controller.abort();
  }, [participantId]);

  const refetch = useCallback(() => {
    setError(null);
    setIsLoading(true);
    get<ParticipantStatement[]>(`/api/participants/${participantId}/statements`)
      .then(setStatements)
      .catch((err) =>
        setError(err instanceof Error ? err : new Error('Failed to fetch statements')),
      )
      .finally(() => setIsLoading(false));
  }, [participantId]);

  return { statements, isLoading, error, refetch };
}

export async function generateStatement(
  participantId: string,
  request: GenerateStatementRequest,
): Promise<ParticipantStatement> {
  return post<ParticipantStatement>(`/api/participants/${participantId}/statements`, request);
}

export async function sendStatement(statementId: string): Promise<ParticipantStatement> {
  return post<ParticipantStatement>(`/api/statements/${statementId}/send`);
}

export function getStatementPdfUrl(statementId: string): string {
  return `${API_URL}/api/statements/${statementId}/pdf`;
}
