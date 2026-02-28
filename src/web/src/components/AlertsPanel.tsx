import { useState } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Skeleton } from '@/components/ui/skeleton';
import { ErrorBanner } from '@/components/ErrorBanner';
import { useAlerts, markAlertRead, dismissAlert, generateAlerts } from '@/hooks/useAlerts';
import { Bell, Check, X, RefreshCw, AlertTriangle, AlertCircle, Info } from 'lucide-react';
import type { BudgetAlert, AlertSeverity } from '@/types/api';

const severityConfig: Record<
  AlertSeverity,
  { icon: typeof AlertCircle; label: string; badgeClass: string; borderClass: string }
> = {
  Critical: {
    icon: AlertCircle,
    label: 'Critical',
    badgeClass: 'border-red-200 bg-red-50 text-red-700',
    borderClass: 'border-l-red-500',
  },
  Warning: {
    icon: AlertTriangle,
    label: 'Warning',
    badgeClass: 'border-amber-200 bg-amber-50 text-amber-700',
    borderClass: 'border-l-amber-500',
  },
  Info: {
    icon: Info,
    label: 'Info',
    badgeClass: 'border-blue-200 bg-blue-50 text-blue-700',
    borderClass: 'border-l-blue-500',
  },
};

interface AlertsPanelProps {
  planId: string;
}

export function AlertsPanel({ planId }: AlertsPanelProps) {
  const { alerts, isLoading, error, refetch } = useAlerts(planId);
  const [generating, setGenerating] = useState(false);
  const [actionLoading, setActionLoading] = useState<string | null>(null);

  const handleGenerate = async () => {
    try {
      setGenerating(true);
      await generateAlerts();
      refetch();
    } catch {
      // error will show on refetch
    } finally {
      setGenerating(false);
    }
  };

  const handleMarkRead = async (id: string) => {
    try {
      setActionLoading(id);
      await markAlertRead(id);
      refetch();
    } catch {
      // silent
    } finally {
      setActionLoading(null);
    }
  };

  const handleDismiss = async (id: string) => {
    try {
      setActionLoading(id);
      await dismissAlert(id);
      refetch();
    } catch {
      // silent
    } finally {
      setActionLoading(null);
    }
  };

  if (isLoading) {
    return (
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2 text-base">
            <Bell className="h-4 w-4 text-muted-foreground" />
            Budget Alerts
          </CardTitle>
        </CardHeader>
        <CardContent className="space-y-3">
          <Skeleton className="h-16 w-full" />
          <Skeleton className="h-16 w-full" />
        </CardContent>
      </Card>
    );
  }

  if (error) {
    return (
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2 text-base">
            <Bell className="h-4 w-4 text-muted-foreground" />
            Budget Alerts
          </CardTitle>
        </CardHeader>
        <CardContent>
          <ErrorBanner message={error.message} onRetry={refetch} />
        </CardContent>
      </Card>
    );
  }

  const activeAlerts = alerts.filter((a) => !a.isDismissed);

  // Group by severity for display ordering
  const criticalAlerts = activeAlerts.filter((a) => a.severity === 'Critical');
  const warningAlerts = activeAlerts.filter((a) => a.severity === 'Warning');
  const infoAlerts = activeAlerts.filter((a) => a.severity === 'Info');

  const groupedAlerts = [...criticalAlerts, ...warningAlerts, ...infoAlerts];

  return (
    <Card>
      <CardHeader>
        <div className="flex items-center justify-between">
          <CardTitle className="flex items-center gap-2 text-base">
            <Bell className="h-4 w-4 text-muted-foreground" />
            Budget Alerts
            {activeAlerts.length > 0 && (
              <Badge variant="secondary" className="ml-1">
                {activeAlerts.length}
              </Badge>
            )}
          </CardTitle>
          <Button
            variant="outline"
            size="sm"
            onClick={handleGenerate}
            disabled={generating}
          >
            <RefreshCw className={`mr-1 h-3 w-3 ${generating ? 'animate-spin' : ''}`} />
            {generating ? 'Checking...' : 'Check Now'}
          </Button>
        </div>
      </CardHeader>
      <CardContent>
        {groupedAlerts.length === 0 ? (
          <div className="flex flex-col items-center py-6 text-center">
            <div className="rounded-full bg-emerald-50 p-2.5">
              <Check className="h-4 w-4 text-emerald-600" />
            </div>
            <p className="mt-2 text-sm text-muted-foreground">No active alerts</p>
            <p className="text-xs text-muted-foreground">
              Click "Check Now" to scan for budget and plan issues.
            </p>
          </div>
        ) : (
          <div className="space-y-2">
            {groupedAlerts.map((alert) => (
              <AlertItem
                key={alert.id}
                alert={alert}
                onMarkRead={handleMarkRead}
                onDismiss={handleDismiss}
                isLoading={actionLoading === alert.id}
              />
            ))}
          </div>
        )}
      </CardContent>
    </Card>
  );
}

function AlertItem({
  alert,
  onMarkRead,
  onDismiss,
  isLoading,
}: {
  alert: BudgetAlert;
  onMarkRead: (id: string) => void;
  onDismiss: (id: string) => void;
  isLoading: boolean;
}) {
  const config = severityConfig[alert.severity];
  const SeverityIcon = config.icon;

  return (
    <div
      className={`flex items-start gap-3 rounded-lg border border-l-4 p-3 ${config.borderClass} ${
        alert.isRead ? 'opacity-60' : ''
      }`}
    >
      <SeverityIcon className="mt-0.5 h-4 w-4 shrink-0 text-muted-foreground" />
      <div className="min-w-0 flex-1">
        <div className="flex items-center gap-2">
          <Badge variant="secondary" className={`text-xs ${config.badgeClass}`}>
            {config.label}
          </Badge>
          {!alert.isRead && (
            <span className="h-2 w-2 rounded-full bg-primary" title="Unread" />
          )}
        </div>
        <p className="mt-1 text-sm">{alert.message}</p>
        <p className="mt-0.5 text-xs text-muted-foreground">
          {new Date(alert.createdAt).toLocaleString('en-AU')}
        </p>
      </div>
      <div className="flex shrink-0 gap-1">
        {!alert.isRead && (
          <Button
            variant="ghost"
            size="sm"
            className="h-7 w-7 p-0"
            onClick={() => onMarkRead(alert.id)}
            disabled={isLoading}
            title="Mark as read"
          >
            <Check className="h-3.5 w-3.5" />
          </Button>
        )}
        <Button
          variant="ghost"
          size="sm"
          className="h-7 w-7 p-0"
          onClick={() => onDismiss(alert.id)}
          disabled={isLoading}
          title="Dismiss"
        >
          <X className="h-3.5 w-3.5" />
        </Button>
      </div>
    </div>
  );
}
