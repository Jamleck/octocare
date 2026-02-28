import { useCallback, useEffect, useState } from 'react';
import { get, post } from '@/lib/api-client';
import type {
  ServiceAgreement,
  ServiceBooking,
  CreateServiceAgreementRequest,
  CreateServiceBookingRequest,
} from '@/types/api';

export function useServiceAgreements(participantId: string) {
  const [agreements, setAgreements] = useState<ServiceAgreement[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<Error | null>(null);

  const fetchAgreements = useCallback(
    (signal?: AbortSignal) => {
      setIsLoading(true);
      setError(null);
      get<ServiceAgreement[]>(`/api/participants/${participantId}/agreements`, { signal })
        .then(setAgreements)
        .catch((err) => {
          if (err instanceof DOMException && err.name === 'AbortError') return;
          setError(err instanceof Error ? err : new Error('Failed to fetch service agreements'));
        })
        .finally(() => setIsLoading(false));
    },
    [participantId],
  );

  useEffect(() => {
    const controller = new AbortController();
    fetchAgreements(controller.signal);
    return () => controller.abort();
  }, [fetchAgreements]);

  const refetch = useCallback(() => fetchAgreements(), [fetchAgreements]);

  return { agreements, isLoading, error, refetch };
}

export function useServiceAgreement(id: string) {
  const [agreement, setAgreement] = useState<ServiceAgreement | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<Error | null>(null);

  useEffect(() => {
    const controller = new AbortController();

    async function fetchAgreement() {
      try {
        setIsLoading(true);
        setError(null);
        const data = await get<ServiceAgreement>(`/api/agreements/${id}`, {
          signal: controller.signal,
        });
        setAgreement(data);
      } catch (err) {
        if (err instanceof DOMException && err.name === 'AbortError') return;
        setError(err instanceof Error ? err : new Error('Failed to fetch service agreement'));
      } finally {
        if (!controller.signal.aborted) {
          setIsLoading(false);
        }
      }
    }

    fetchAgreement();
    return () => controller.abort();
  }, [id]);

  const refetch = useCallback(() => {
    setError(null);
    setIsLoading(true);
    get<ServiceAgreement>(`/api/agreements/${id}`)
      .then(setAgreement)
      .catch((err) =>
        setError(err instanceof Error ? err : new Error('Failed to fetch service agreement')),
      )
      .finally(() => setIsLoading(false));
  }, [id]);

  return { agreement, isLoading, error, refetch };
}

export async function createServiceAgreement(
  participantId: string,
  request: CreateServiceAgreementRequest,
): Promise<ServiceAgreement> {
  return post<ServiceAgreement>(`/api/participants/${participantId}/agreements`, request);
}

export async function activateServiceAgreement(id: string): Promise<ServiceAgreement> {
  return post<ServiceAgreement>(`/api/agreements/${id}/activate`);
}

export async function addBooking(
  agreementId: string,
  request: CreateServiceBookingRequest,
): Promise<ServiceBooking> {
  return post<ServiceBooking>(`/api/agreements/${agreementId}/bookings`, request);
}

export async function cancelBooking(
  agreementId: string,
  bookingId: string,
): Promise<ServiceBooking> {
  return post<ServiceBooking>(`/api/agreements/${agreementId}/bookings/${bookingId}/cancel`);
}
