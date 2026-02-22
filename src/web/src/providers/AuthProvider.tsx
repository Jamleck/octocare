import { Auth0Provider } from '@auth0/auth0-react';
import { auth0Config } from '@/lib/auth';
import { DevAuthProvider } from './DevAuthProvider';

const isDevBypass = import.meta.env.VITE_AUTH_BYPASS === 'true';

export function AuthProvider({ children }: { children: React.ReactNode }) {
  if (isDevBypass) {
    return <DevAuthProvider>{children}</DevAuthProvider>;
  }

  return (
    <Auth0Provider
      {...auth0Config}
      onRedirectCallback={() => {
        window.history.replaceState({}, '', '/dashboard');
      }}
    >
      {children}
    </Auth0Provider>
  );
}
