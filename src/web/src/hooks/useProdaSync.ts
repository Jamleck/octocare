import { useCallback, useRef, useState } from 'react';
import { post } from '@/lib/api-client';
import type { SyncResult } from '@/types/api';

export function useProdaSync() {
  const [syncResult, setSyncResult] = useState<SyncResult | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<Error | null>(null);
  const abortControllerRef = useRef<AbortController | null>(null);

  const syncParticipantPlan = useCallback(async (participantId: string) => {
    // Cancel any in-flight request
    abortControllerRef.current?.abort();
    const controller = new AbortController();
    abortControllerRef.current = controller;

    setIsLoading(true);
    setError(null);
    setSyncResult(null);

    try {
      const result = await post<SyncResult>(
        `/api/proda/sync/participant/${participantId}`,
        undefined,
        { signal: controller.signal },
      );
      if (!controller.signal.aborted) {
        setSyncResult(result);
      }
    } catch (err) {
      if (err instanceof DOMException && err.name === 'AbortError') return;
      if (!controller.signal.aborted) {
        setError(err instanceof Error ? err : new Error('Sync failed'));
      }
    } finally {
      if (!controller.signal.aborted) {
        setIsLoading(false);
      }
    }
  }, []);

  const verifyBudget = useCallback(async (planId: string) => {
    abortControllerRef.current?.abort();
    const controller = new AbortController();
    abortControllerRef.current = controller;

    setIsLoading(true);
    setError(null);
    setSyncResult(null);

    try {
      const result = await post<SyncResult>(
        `/api/proda/sync/plan/${planId}/budget`,
        undefined,
        { signal: controller.signal },
      );
      if (!controller.signal.aborted) {
        setSyncResult(result);
      }
    } catch (err) {
      if (err instanceof DOMException && err.name === 'AbortError') return;
      if (!controller.signal.aborted) {
        setError(err instanceof Error ? err : new Error('Budget verification failed'));
      }
    } finally {
      if (!controller.signal.aborted) {
        setIsLoading(false);
      }
    }
  }, []);

  const reset = useCallback(() => {
    abortControllerRef.current?.abort();
    setSyncResult(null);
    setError(null);
    setIsLoading(false);
  }, []);

  return { syncResult, isLoading, error, syncParticipantPlan, verifyBudget, reset };
}
