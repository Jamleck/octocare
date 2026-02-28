import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { useProdaSync } from '@/hooks/useProdaSync';
import {
  RefreshCw,
  CheckCircle2,
  AlertTriangle,
  XCircle,
  Loader2,
  CloudOff,
} from 'lucide-react';
import type { SyncDiscrepancy } from '@/types/api';

interface ProdaSyncPanelProps {
  participantId: string;
}

const severityConfig: Record<string, { icon: typeof AlertTriangle; className: string; label: string }> = {
  error: {
    icon: XCircle,
    className: 'border-red-200 bg-red-50 text-red-700',
    label: 'Error',
  },
  warning: {
    icon: AlertTriangle,
    className: 'border-yellow-200 bg-yellow-50 text-yellow-700',
    label: 'Warning',
  },
  info: {
    icon: CheckCircle2,
    className: 'border-blue-200 bg-blue-50 text-blue-700',
    label: 'Info',
  },
};

function SeverityBadge({ severity }: { severity: string }) {
  const config = severityConfig[severity] || severityConfig.warning;
  return (
    <Badge variant="secondary" className={config.className}>
      {config.label}
    </Badge>
  );
}

function DiscrepancyRow({ discrepancy }: { discrepancy: SyncDiscrepancy }) {
  return (
    <div className="flex items-start justify-between gap-4 rounded-md border px-4 py-3 text-sm">
      <div className="min-w-0 flex-1 space-y-1">
        <div className="font-medium">{discrepancy.field}</div>
        <div className="flex gap-4 text-muted-foreground">
          <span>
            <span className="font-medium text-foreground">Local:</span> {discrepancy.localValue}
          </span>
          <span>
            <span className="font-medium text-foreground">PRODA:</span> {discrepancy.prodaValue}
          </span>
        </div>
      </div>
      <SeverityBadge severity={discrepancy.severity} />
    </div>
  );
}

export function ProdaSyncPanel({ participantId }: ProdaSyncPanelProps) {
  const { syncResult, isLoading, error, syncParticipantPlan, reset } = useProdaSync();

  const handleSync = () => {
    syncParticipantPlan(participantId);
  };

  return (
    <Card>
      <CardHeader>
        <div className="flex items-center justify-between">
          <CardTitle className="flex items-center gap-2 text-base">
            <RefreshCw className="h-4 w-4 text-muted-foreground" />
            PRODA Sync
          </CardTitle>
          <div className="flex gap-2">
            {syncResult && (
              <Button variant="ghost" size="sm" onClick={reset}>
                Clear
              </Button>
            )}
            <Button
              variant="outline"
              size="sm"
              onClick={handleSync}
              disabled={isLoading}
            >
              {isLoading ? (
                <>
                  <Loader2 className="mr-1 h-3 w-3 animate-spin" />
                  Syncing...
                </>
              ) : (
                <>
                  <RefreshCw className="mr-1 h-3 w-3" />
                  Sync with PRODA
                </>
              )}
            </Button>
          </div>
        </div>
      </CardHeader>
      <CardContent>
        {!syncResult && !error && !isLoading && (
          <div className="flex flex-col items-center py-6 text-center">
            <div className="rounded-full bg-muted p-2.5">
              <CloudOff className="h-4 w-4 text-muted-foreground" />
            </div>
            <p className="mt-2 text-sm text-muted-foreground">
              Click &quot;Sync with PRODA&quot; to compare local data with the NDIA portal.
            </p>
            <p className="mt-1 text-xs text-muted-foreground">
              Note: Using mock data for MVP. Real PRODA integration requires registration.
            </p>
          </div>
        )}

        {isLoading && (
          <div className="flex flex-col items-center py-6 text-center">
            <Loader2 className="h-6 w-6 animate-spin text-primary" />
            <p className="mt-2 text-sm text-muted-foreground">Connecting to PRODA...</p>
          </div>
        )}

        {error && (
          <div className="flex flex-col items-center py-6 text-center">
            <div className="rounded-full bg-red-50 p-2.5">
              <XCircle className="h-4 w-4 text-red-600" />
            </div>
            <p className="mt-2 text-sm font-medium text-red-700">Sync Failed</p>
            <p className="mt-1 text-xs text-muted-foreground">{error.message}</p>
          </div>
        )}

        {syncResult && !isLoading && (
          <div className="space-y-3">
            {syncResult.inSync ? (
              <div className="flex items-center gap-3 rounded-lg border border-emerald-200 bg-emerald-50 px-4 py-3">
                <CheckCircle2 className="h-5 w-5 text-emerald-600" />
                <div>
                  <p className="text-sm font-medium text-emerald-800">In Sync</p>
                  <p className="text-xs text-emerald-600">
                    Local data matches PRODA records.
                  </p>
                </div>
              </div>
            ) : (
              <>
                <div className="flex items-center gap-3 rounded-lg border border-yellow-200 bg-yellow-50 px-4 py-3">
                  <AlertTriangle className="h-5 w-5 text-yellow-600" />
                  <div>
                    <p className="text-sm font-medium text-yellow-800">
                      {syncResult.discrepancies.length} Discrepanc{syncResult.discrepancies.length === 1 ? 'y' : 'ies'} Found
                    </p>
                    <p className="text-xs text-yellow-600">
                      Local data differs from PRODA records. Review the items below.
                    </p>
                  </div>
                </div>
                <div className="space-y-2">
                  {syncResult.discrepancies.map((d, i) => (
                    <DiscrepancyRow key={i} discrepancy={d} />
                  ))}
                </div>
              </>
            )}
          </div>
        )}
      </CardContent>
    </Card>
  );
}
