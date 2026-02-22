export const auth0Config = {
  domain: import.meta.env.VITE_AUTH_DOMAIN ?? 'your-tenant.auth0.com',
  clientId: import.meta.env.VITE_AUTH_CLIENT_ID ?? '',
  authorizationParams: {
    redirect_uri: typeof window !== 'undefined' ? `${window.location.origin}/callback` : '',
    audience: import.meta.env.VITE_AUTH_AUDIENCE ?? 'https://api.octocare.com.au',
    scope: 'openid profile email',
  },
};
