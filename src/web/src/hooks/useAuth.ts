import { useAuth0 } from '@auth0/auth0-react';
import { useDevAuth } from '@/providers/DevAuthProvider';

const isDevBypass = import.meta.env.VITE_AUTH_BYPASS === 'true';

export function useAuth() {
  if (isDevBypass) {
    return useDevAuth();
  }
  return useAuth0();
}
