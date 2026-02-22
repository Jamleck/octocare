import { useAuth } from '@/hooks/useAuth';
import { useEffect } from 'react';
import { useNavigate } from 'react-router';
import { LoadingScreen } from './LoadingScreen';

export function AuthCallback() {
  const { isAuthenticated, isLoading } = useAuth();
  const navigate = useNavigate();

  useEffect(() => {
    if (!isLoading && isAuthenticated) {
      navigate('/dashboard', { replace: true });
    }
  }, [isLoading, isAuthenticated, navigate]);

  return <LoadingScreen />;
}
