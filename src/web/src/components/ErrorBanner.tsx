import { AlertCircle } from 'lucide-react';

interface ErrorBannerProps {
  message: string;
  onRetry?: () => void;
}

export function ErrorBanner({ message, onRetry }: ErrorBannerProps) {
  return (
    <div className="rounded-lg border border-destructive/20 bg-destructive/5 p-4">
      <div className="flex items-center gap-3">
        <AlertCircle className="h-5 w-5 shrink-0 text-destructive" />
        <div className="flex-1">
          <p className="text-sm font-medium text-destructive">Something went wrong</p>
          <p className="mt-1 text-sm text-muted-foreground">{message}</p>
        </div>
        {onRetry && (
          <button
            onClick={onRetry}
            className="text-sm font-medium text-primary hover:underline"
          >
            Try again
          </button>
        )}
      </div>
    </div>
  );
}
