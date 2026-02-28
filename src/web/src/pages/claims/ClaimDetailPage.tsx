import { useState } from 'react';
import { Link, useParams, useNavigate } from 'react-router';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Skeleton } from '@/components/ui/skeleton';
import {
  useClaim,
  submitClaim,
  recordClaimOutcome,
  getClaimCsvUrl,
} from '@/hooks/useClaims';
import { ErrorBanner } from '@/components/ErrorBanner';
import {
  ArrowLeft,
  Send,
  Calendar,
  Hash,
  DollarSign,
  Download,
  CheckCircle,
  XCircle,
  ClipboardList,
} from 'lucide-react';
import type { ClaimStatus, ClaimLineItemStatus, ClaimLineItemOutcome } from '@/types/api';

const statusConfig: Record<ClaimStatus, { label: string; className: string }> = {
  draft: { label: 'Draft', className: 'border-gray-200 bg-gray-50 text-gray-700' },
  submitted: { label: 'Submitted', className: 'border-blue-200 bg-blue-50 text-blue-700' },
  accepted: { label: 'Accepted', className: 'border-emerald-200 bg-emerald-50 text-emerald-700' },
  partially_rejected: {
    label: 'Partially Rejected',
    className: 'border-orange-200 bg-orange-50 text-orange-700',
  },
  rejected: { label: 'Rejected', className: 'border-red-200 bg-red-50 text-red-700' },
};

const lineItemStatusConfig: Record<
  ClaimLineItemStatus,
  { label: string; className: string }
> = {
  pending: { label: 'Pending', className: 'border-gray-200 bg-gray-50 text-gray-700' },
  accepted: { label: 'Accepted', className: 'border-emerald-200 bg-emerald-50 text-emerald-700' },
  rejected: { label: 'Rejected', className: 'border-red-200 bg-red-50 text-red-700' },
};

export function ClaimDetailPage() {
  const { id } = useParams<{ id: string }>();
  const { claim, isLoading, error, refetch } = useClaim(id!);
  const navigate = useNavigate();

  const [actionLoading, setActionLoading] = useState(false);
  const [actionError, setActionError] = useState<string | null>(null);

  // Outcome dialog state
  const [outcomeOpen, setOutcomeOpen] = useState(false);
  const [ndiaReference, setNdiaReference] = useState('');
  const [lineItemOutcomes, setLineItemOutcomes] = useState<
    Record<string, { status: string; reason: string }>
  >({});

  if (isLoading) {
    return (
      <div className="space-y-4">
        <Skeleton className="h-5 w-32" />
        <Skeleton className="h-8 w-64" />
        <div className="grid gap-6 md:grid-cols-2">
          <Skeleton className="h-48 rounded-lg" />
          <Skeleton className="h-48 rounded-lg" />
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="space-y-6">
        <Button
          variant="ghost"
          size="sm"
          className="-ml-2 text-muted-foreground"
          onClick={() => navigate(-1)}
        >
          <ArrowLeft className="mr-1 h-4 w-4" />
          Back
        </Button>
        <ErrorBanner message={error.message} onRetry={refetch} />
      </div>
    );
  }

  if (!claim) {
    return (
      <div className="flex flex-col items-center py-16">
        <Send className="h-10 w-10 text-muted-foreground" />
        <p className="mt-3 text-lg font-medium">Claim not found</p>
        <Button variant="outline" className="mt-4" onClick={() => navigate(-1)}>
          <ArrowLeft className="mr-2 h-4 w-4" />
          Go Back
        </Button>
      </div>
    );
  }

  const status = statusConfig[claim.status] || statusConfig.draft;
  const canSubmit = claim.status === 'draft';
  const canRecordOutcome = claim.status === 'submitted';
  const canDownloadCsv = true; // Always allow CSV download

  const handleSubmit = async () => {
    try {
      setActionLoading(true);
      setActionError(null);
      await submitClaim(claim.id);
      refetch();
    } catch (err) {
      setActionError(err instanceof Error ? err.message : 'Failed to submit claim.');
    } finally {
      setActionLoading(false);
    }
  };

  const handleDownloadCsv = () => {
    window.open(getClaimCsvUrl(claim.id), '_blank');
  };

  const openOutcomeDialog = () => {
    // Initialize outcomes for all pending line items
    const initial: Record<string, { status: string; reason: string }> = {};
    for (const li of claim.lineItems.filter((li) => li.status === 'pending')) {
      initial[li.id] = { status: 'accepted', reason: '' };
    }
    setLineItemOutcomes(initial);
    setNdiaReference(claim.ndiaReference || '');
    setOutcomeOpen(true);
  };

  const handleRecordOutcome = async (e: React.FormEvent) => {
    e.preventDefault();

    const outcomes: ClaimLineItemOutcome[] = Object.entries(lineItemOutcomes).map(
      ([lineItemId, { status, reason }]) => ({
        lineItemId,
        status: status as 'accepted' | 'rejected',
        rejectionReason: status === 'rejected' ? reason : undefined,
      }),
    );

    // Validate all rejected items have reasons
    for (const outcome of outcomes) {
      if (outcome.status === 'rejected' && !outcome.rejectionReason?.trim()) {
        setActionError('All rejected line items must have a rejection reason.');
        return;
      }
    }

    try {
      setActionLoading(true);
      setActionError(null);
      await recordClaimOutcome(claim.id, {
        ndiaReference: ndiaReference.trim() || undefined,
        lineItems: outcomes,
      });
      setOutcomeOpen(false);
      refetch();
    } catch (err) {
      setActionError(err instanceof Error ? err.message : 'Failed to record outcome.');
    } finally {
      setActionLoading(false);
    }
  };

  return (
    <div className="space-y-6">
      <div>
        <Button variant="ghost" size="sm" className="-ml-2 mb-2 text-muted-foreground" asChild>
          <Link to="/claims">
            <ArrowLeft className="mr-1 h-4 w-4" />
            Claims
          </Link>
        </Button>

        <div className="flex items-center justify-between">
          <div className="flex items-center gap-3">
            <div className="flex h-10 w-10 items-center justify-center rounded-full bg-primary/10 text-primary">
              <Send className="h-5 w-5" />
            </div>
            <div>
              <h1 className="text-2xl font-bold tracking-tight">{claim.batchNumber}</h1>
              <p className="text-sm text-muted-foreground">
                {claim.lineItems.length} line item{claim.lineItems.length !== 1 ? 's' : ''}
              </p>
            </div>
            <Badge variant="secondary" className={status.className}>
              {status.label}
            </Badge>
          </div>
          <div className="flex gap-2">
            {canDownloadCsv && (
              <Button onClick={handleDownloadCsv} variant="outline">
                <Download className="mr-2 h-4 w-4" />
                Download CSV
              </Button>
            )}
            {canSubmit && (
              <Button onClick={handleSubmit} disabled={actionLoading}>
                <Send className="mr-2 h-4 w-4" />
                Submit to NDIA
              </Button>
            )}
            {canRecordOutcome && (
              <Button onClick={openOutcomeDialog} disabled={actionLoading}>
                <ClipboardList className="mr-2 h-4 w-4" />
                Record Outcome
              </Button>
            )}
          </div>
        </div>
      </div>

      {actionError && <ErrorBanner message={actionError} />}

      <div className="grid gap-6 md:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2 text-base">
              <Send className="h-4 w-4 text-muted-foreground" />
              Claim Details
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <DetailRow icon={Hash} label="Batch Number" value={claim.batchNumber} mono />
            <DetailRow
              icon={DollarSign}
              label="Total Amount"
              value={`$${claim.totalAmount.toLocaleString('en-AU', { minimumFractionDigits: 2 })}`}
            />
            <DetailRow
              icon={Calendar}
              label="Created"
              value={new Date(claim.createdAt).toLocaleDateString('en-AU', {
                year: 'numeric',
                month: 'long',
                day: 'numeric',
              })}
            />
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2 text-base">
              <ClipboardList className="h-4 w-4 text-muted-foreground" />
              Submission Info
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <DetailRow
              icon={Calendar}
              label="Submission Date"
              value={
                claim.submissionDate
                  ? new Date(claim.submissionDate).toLocaleDateString('en-AU')
                  : undefined
              }
            />
            <DetailRow
              icon={Calendar}
              label="Response Date"
              value={
                claim.responseDate
                  ? new Date(claim.responseDate).toLocaleDateString('en-AU')
                  : undefined
              }
            />
            <DetailRow icon={Hash} label="NDIA Reference" value={claim.ndiaReference} mono />
          </CardContent>
        </Card>
      </div>

      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2 text-base">
            <DollarSign className="h-4 w-4 text-muted-foreground" />
            Line Items ({claim.lineItems.length})
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="rounded-lg border">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Support Item</TableHead>
                  <TableHead>Description</TableHead>
                  <TableHead>Invoice</TableHead>
                  <TableHead>Provider</TableHead>
                  <TableHead>Service Date</TableHead>
                  <TableHead className="text-right">Amount</TableHead>
                  <TableHead>Status</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {claim.lineItems.map((li) => {
                  const liStatus =
                    lineItemStatusConfig[li.status] || lineItemStatusConfig.pending;
                  return (
                    <TableRow key={li.id}>
                      <TableCell className="font-mono text-sm">{li.supportItemNumber}</TableCell>
                      <TableCell>{li.description}</TableCell>
                      <TableCell className="font-mono text-sm">{li.invoiceNumber}</TableCell>
                      <TableCell>{li.providerName}</TableCell>
                      <TableCell className="text-muted-foreground">
                        {new Date(li.serviceDate).toLocaleDateString('en-AU')}
                      </TableCell>
                      <TableCell className="text-right font-medium">
                        ${li.amount.toLocaleString('en-AU', { minimumFractionDigits: 2 })}
                      </TableCell>
                      <TableCell>
                        <div className="flex flex-col gap-1">
                          <Badge variant="secondary" className={liStatus.className}>
                            {liStatus.label}
                          </Badge>
                          {li.rejectionReason && (
                            <span className="text-xs text-muted-foreground">
                              {li.rejectionReason}
                            </span>
                          )}
                        </div>
                      </TableCell>
                    </TableRow>
                  );
                })}
                <TableRow className="bg-muted/30 font-medium">
                  <TableCell colSpan={5} className="text-right">
                    Total
                  </TableCell>
                  <TableCell className="text-right">
                    ${claim.totalAmount.toLocaleString('en-AU', { minimumFractionDigits: 2 })}
                  </TableCell>
                  <TableCell />
                </TableRow>
              </TableBody>
            </Table>
          </div>
        </CardContent>
      </Card>

      {/* Record Outcome Dialog */}
      <Dialog open={outcomeOpen} onOpenChange={setOutcomeOpen}>
        <DialogContent className="max-w-2xl">
          <DialogHeader>
            <DialogTitle>Record NDIA Outcome</DialogTitle>
            <DialogDescription>
              Record the NDIA response for each line item in claim {claim.batchNumber}.
            </DialogDescription>
          </DialogHeader>
          <form onSubmit={handleRecordOutcome} className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="ndiaRef">NDIA Reference (optional)</Label>
              <Input
                id="ndiaRef"
                value={ndiaReference}
                onChange={(e) => setNdiaReference(e.target.value)}
                placeholder="e.g. NDIA-2025-REF-001"
              />
            </div>

            <div className="max-h-80 space-y-3 overflow-y-auto">
              {claim.lineItems
                .filter((li) => li.status === 'pending')
                .map((li) => (
                  <div key={li.id} className="space-y-2 rounded-lg border p-3">
                    <div className="flex items-center justify-between">
                      <div>
                        <p className="text-sm font-medium">{li.description}</p>
                        <p className="text-xs text-muted-foreground">
                          {li.supportItemNumber} &mdash; $
                          {li.amount.toLocaleString('en-AU', { minimumFractionDigits: 2 })}
                        </p>
                      </div>
                      <Select
                        value={lineItemOutcomes[li.id]?.status || 'accepted'}
                        onValueChange={(value) =>
                          setLineItemOutcomes((prev) => ({
                            ...prev,
                            [li.id]: { ...prev[li.id], status: value },
                          }))
                        }
                      >
                        <SelectTrigger className="w-32">
                          <SelectValue />
                        </SelectTrigger>
                        <SelectContent>
                          <SelectItem value="accepted">
                            <span className="flex items-center gap-1">
                              <CheckCircle className="h-3 w-3 text-emerald-600" />
                              Accepted
                            </span>
                          </SelectItem>
                          <SelectItem value="rejected">
                            <span className="flex items-center gap-1">
                              <XCircle className="h-3 w-3 text-red-600" />
                              Rejected
                            </span>
                          </SelectItem>
                        </SelectContent>
                      </Select>
                    </div>
                    {lineItemOutcomes[li.id]?.status === 'rejected' && (
                      <Input
                        placeholder="Rejection reason"
                        value={lineItemOutcomes[li.id]?.reason || ''}
                        onChange={(e) =>
                          setLineItemOutcomes((prev) => ({
                            ...prev,
                            [li.id]: { ...prev[li.id], reason: e.target.value },
                          }))
                        }
                      />
                    )}
                  </div>
                ))}
            </div>

            <DialogFooter>
              <Button type="button" variant="outline" onClick={() => setOutcomeOpen(false)}>
                Cancel
              </Button>
              <Button type="submit" disabled={actionLoading}>
                {actionLoading ? 'Recording...' : 'Record Outcome'}
              </Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>
    </div>
  );
}

function DetailRow({
  icon: Icon,
  label,
  value,
  mono,
}: {
  icon: typeof Send;
  label: string;
  value?: string | null;
  mono?: boolean;
}) {
  return (
    <div className="flex items-center gap-3 text-sm">
      <Icon className="h-4 w-4 shrink-0 text-muted-foreground" />
      <span className="min-w-[120px] text-muted-foreground">{label}</span>
      <span className={`${mono ? 'font-mono' : ''} ${value ? '' : 'text-muted-foreground'}`}>
        {value || '--'}
      </span>
    </div>
  );
}
