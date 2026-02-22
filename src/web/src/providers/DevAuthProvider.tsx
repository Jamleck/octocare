import { createContext, useContext, useMemo } from 'react';

interface DevAuthContextValue {
  isAuthenticated: boolean;
  isLoading: boolean;
  user: {
    sub: string;
    email: string;
    name: string;
    picture?: string;
  };
  loginWithRedirect: () => Promise<void>;
  logout: (options?: { logoutParams?: { returnTo?: string } }) => void;
  getAccessTokenSilently: () => Promise<string>;
}

const DevAuthContext = createContext<DevAuthContextValue | null>(null);

export function DevAuthProvider({ children }: { children: React.ReactNode }) {
  const value = useMemo<DevAuthContextValue>(
    () => ({
      isAuthenticated: true,
      isLoading: false,
      user: {
        sub: 'auth0|dev-admin',
        email: 'admin@acmepm.com.au',
        name: 'Admin User',
      },
      loginWithRedirect: async () => {},
      logout: () => {},
      getAccessTokenSilently: async () => 'dev-token',
    }),
    [],
  );

  return <DevAuthContext.Provider value={value}>{children}</DevAuthContext.Provider>;
}

export function useDevAuth(): DevAuthContextValue {
  const ctx = useContext(DevAuthContext);
  if (!ctx) {
    throw new Error('useDevAuth must be used within a DevAuthProvider');
  }
  return ctx;
}
