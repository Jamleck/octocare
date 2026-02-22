import { useCallback, useEffect, useState } from 'react';
import { get, put } from '@/lib/api-client';
import type { Organisation, UpdateOrganisationRequest } from '@/types/api';

export function useOrganisation() {
  const [organisation, setOrganisation] = useState<Organisation | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<Error | null>(null);

  const fetchOrganisation = useCallback(async () => {
    try {
      setIsLoading(true);
      setError(null);
      const data = await get<Organisation>('/api/organisations/current');
      setOrganisation(data);
    } catch (err) {
      setError(err instanceof Error ? err : new Error('Failed to fetch organisation'));
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchOrganisation();
  }, [fetchOrganisation]);

  const updateOrganisation = async (request: UpdateOrganisationRequest) => {
    const data = await put<Organisation>('/api/organisations/current', request);
    setOrganisation(data);
    return data;
  };

  return { organisation, isLoading, error, updateOrganisation, refetch: fetchOrganisation };
}
