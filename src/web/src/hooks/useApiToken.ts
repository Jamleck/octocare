import { useAuth } from '@/hooks/useAuth';
import { useEffect } from 'react';
import { setTokenGetter } from '@/lib/api-client';

export function useApiToken() {
  const { getAccessTokenSilently, isAuthenticated } = useAuth();

  useEffect(() => {
    if (isAuthenticated) {
      setTokenGetter(getAccessTokenSilently);
    }
  }, [isAuthenticated, getAccessTokenSilently]);
}
