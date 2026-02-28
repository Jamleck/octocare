import { useState } from 'react';
import { Link, useParams, useNavigate } from 'react-router';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
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
  useInvoice,
  approveInvoice,
  rejectInvoice,
  disputeInvoice,
  markInvoicePaid,
} from '@/hooks/useInvoices';
import { ErrorBanner } from '@/components/ErrorBanner';
import {
  ArrowLeft,
  FileText,
  Calendar,
  Hash,
  DollarSign,
  Building2,
  User,
  CheckCircle,
  XCircle,
  AlertTriangle,
  CreditCard,
} from 'lucide-react';
import type { InvoiceStatus } from '@/types/api';

const statusConfig: Record<InvoiceStatus, { label: string; className: string }> = {
  submitted: { label: 'Submitted', className: 'border-blue-200 bg-blue-50 text-blue-700' },
  under_review: {
    label: 'Under Review',
    className: 'border-yellow-200 bg-yellow-50 text-yellow-700',
  },
  approved: { label: 'Approved', className: 'border-emerald-200 bg-emerald-50 text-emerald-700' },
  rejected: { label: 'Rejected', className: 'border-red-200 bg-red-50 text-red-700' },
  disputed: { label: 'Disputed', className: 'border-orange-200 bg-orange-50 text-orange-700' },
  paid: { label: 'Paid', className: 'border-purple-200 bg-purple-50 text-purple-700' },
};

const validationBadges: Record<string, { label: string; className: string }> = {
  valid: { label: 'Valid', className: 'border-emerald-200 bg-emerald-50 text-emerald-700' },
  warning: { label: 'Warning', className: 'border-yellow-200 bg-yellow-50 text-yellow-700' },
  error: { label: 'Error', className: 'border-red-200 bg-red-50 text-red-700' },
};

export function InvoiceDetailPage() {
  const { id } = useParams<{ id: string }>();
  const { invoice, isLoading, error, refetch } = useInvoice(id!);
  const navigate = useNavigate();

  const [actionLoading, setActionLoading] = useState(false);
  const [actionError, setActionError] = useState<string | null>(null);

  // Dialog state
  const [rejectOpen, setRejectOpen] = useState(false);
  const [disputeOpen, setDisputeOpen] = useState(false);
  const [rejectReason, setRejectReason] = useState('');
  const [disputeReason, setDisputeReason] = useState('');

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

  if (!invoice) {
    return (
      <div className="flex flex-col items-center py-16">
        <FileText className="h-10 w-10 text-muted-foreground" />
        <p className="mt-3 text-lg font-medium">Invoice not found</p>
        <Button variant="outline" className="mt-4" onClick={() => navigate(-1)}>
          <ArrowLeft className="mr-2 h-4 w-4" />
          Go Back
        </Button>
      </div>
    );
  }

  const status = statusConfig[invoice.status] || statusConfig.submitted;
  const canApprove =
    invoice.status === 'submitted' || invoice.status === 'under_review';
  const canReject =
    invoice.status === 'submitted' || invoice.status === 'under_review';
  const canDispute =
    invoice.status === 'submitted' ||
    invoice.status === 'under_review' ||
    invoice.status === 'approved';
  const canMarkPaid = invoice.status === 'approved';

  const handleApprove = async () => {
    try {
      setActionLoading(true);
      setActionError(null);
      await approveInvoice(invoice.id);
      refetch();
    } catch (err) {
      setActionError(err instanceof Error ? err.message : 'Failed to approve invoice.');
    } finally {
      setActionLoading(false);
    }
  };

  const handleReject = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!rejectReason.trim()) return;
    try {
      setActionLoading(true);
      setActionError(null);
      await rejectInvoice(invoice.id, rejectReason.trim());
      setRejectOpen(false);
      setRejectReason('');
      refetch();
    } catch (err) {
      setActionError(err instanceof Error ? err.message : 'Failed to reject invoice.');
    } finally {
      setActionLoading(false);
    }
  };

  const handleDispute = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!disputeReason.trim()) return;
    try {
      setActionLoading(true);
      setActionError(null);
      await disputeInvoice(invoice.id, disputeReason.trim());
      setDisputeOpen(false);
      setDisputeReason('');
      refetch();
    } catch (err) {
      setActionError(err instanceof Error ? err.message : 'Failed to dispute invoice.');
    } finally {
      setActionLoading(false);
    }
  };

  const handleMarkPaid = async () => {
    try {
      setActionLoading(true);
      setActionError(null);
      await markInvoicePaid(invoice.id);
      refetch();
    } catch (err) {
      setActionError(err instanceof Error ? err.message : 'Failed to mark invoice as paid.');
    } finally {
      setActionLoading(false);
    }
  };

  return (
    <div className="space-y-6">
      <div>
        <Button variant="ghost" size="sm" className="-ml-2 mb-2 text-muted-foreground" asChild>
          <Link to="/invoices">
            <ArrowLeft className="mr-1 h-4 w-4" />
            Invoices
          </Link>
        </Button>

        <div className="flex items-center justify-between">
          <div className="flex items-center gap-3">
            <div className="flex h-10 w-10 items-center justify-center rounded-full bg-primary/10 text-primary">
              <FileText className="h-5 w-5" />
            </div>
            <div>
              <h1 className="text-2xl font-bold tracking-tight">{invoice.invoiceNumber}</h1>
              <p className="text-sm text-muted-foreground">
                {invoice.providerName}
              </p>
            </div>
            <Badge variant="secondary" className={status.className}>
              {status.label}
            </Badge>
          </div>
          <div className="flex gap-2">
            {canApprove && (
              <Button onClick={handleApprove} disabled={actionLoading} variant="default">
                <CheckCircle className="mr-2 h-4 w-4" />
                Approve
              </Button>
            )}
            {canReject && (
              <Button
                onClick={() => setRejectOpen(true)}
                disabled={actionLoading}
                variant="outline"
                className="text-destructive hover:text-destructive"
              >
                <XCircle className="mr-2 h-4 w-4" />
                Reject
              </Button>
            )}
            {canDispute && (
              <Button
                onClick={() => setDisputeOpen(true)}
                disabled={actionLoading}
                variant="outline"
              >
                <AlertTriangle className="mr-2 h-4 w-4" />
                Dispute
              </Button>
            )}
            {canMarkPaid && (
              <Button onClick={handleMarkPaid} disabled={actionLoading} variant="default">
                <CreditCard className="mr-2 h-4 w-4" />
                Mark Paid
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
              <FileText className="h-4 w-4 text-muted-foreground" />
              Invoice Details
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <DetailRow icon={Hash} label="Invoice Number" value={invoice.invoiceNumber} mono />
            <DetailRow icon={Building2} label="Provider" value={invoice.providerName} />
            <DetailRow icon={User} label="Participant" value={invoice.participantName} />
            <DetailRow icon={FileText} label="Plan" value={invoice.planNumber} mono />
            <DetailRow
              icon={Calendar}
              label="Service Period"
              value={`${new Date(invoice.servicePeriodStart).toLocaleDateString('en-AU')} - ${new Date(invoice.servicePeriodEnd).toLocaleDateString('en-AU')}`}
            />
            <DetailRow
              icon={DollarSign}
              label="Total Amount"
              value={`$${invoice.totalAmount.toLocaleString('en-AU', { minimumFractionDigits: 2 })}`}
            />
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2 text-base">
              <FileText className="h-4 w-4 text-muted-foreground" />
              Additional Info
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <DetailRow icon={FileText} label="Source" value={invoice.source} />
            <DetailRow
              icon={Calendar}
              label="Created"
              value={new Date(invoice.createdAt).toLocaleDateString('en-AU', {
                year: 'numeric',
                month: 'long',
                day: 'numeric',
              })}
            />
            {invoice.notes && (
              <div className="space-y-1">
                <span className="text-sm text-muted-foreground">Notes</span>
                <p className="rounded-md bg-muted/50 p-3 text-sm">{invoice.notes}</p>
              </div>
            )}
          </CardContent>
        </Card>
      </div>

      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2 text-base">
            <DollarSign className="h-4 w-4 text-muted-foreground" />
            Line Items ({invoice.lineItems.length})
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="rounded-lg border">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Support Item</TableHead>
                  <TableHead>Description</TableHead>
                  <TableHead>Service Date</TableHead>
                  <TableHead className="text-right">Qty</TableHead>
                  <TableHead className="text-right">Rate</TableHead>
                  <TableHead className="text-right">Amount</TableHead>
                  <TableHead>Validation</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {invoice.lineItems.map((li) => {
                  const vBadge = validationBadges[li.validationStatus] || validationBadges.valid;
                  return (
                    <TableRow key={li.id}>
                      <TableCell className="font-mono text-sm">{li.supportItemNumber}</TableCell>
                      <TableCell>{li.description}</TableCell>
                      <TableCell className="text-muted-foreground">
                        {new Date(li.serviceDate).toLocaleDateString('en-AU')}
                      </TableCell>
                      <TableCell className="text-right">{li.quantity}</TableCell>
                      <TableCell className="text-right">
                        ${li.rate.toLocaleString('en-AU', { minimumFractionDigits: 2 })}
                      </TableCell>
                      <TableCell className="text-right font-medium">
                        ${li.amount.toLocaleString('en-AU', { minimumFractionDigits: 2 })}
                      </TableCell>
                      <TableCell>
                        <div className="flex flex-col gap-1">
                          <Badge variant="secondary" className={vBadge.className}>
                            {vBadge.label}
                          </Badge>
                          {li.validationMessage && (
                            <span className="text-xs text-muted-foreground">
                              {li.validationMessage}
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
                    ${invoice.totalAmount.toLocaleString('en-AU', { minimumFractionDigits: 2 })}
                  </TableCell>
                  <TableCell />
                </TableRow>
              </TableBody>
            </Table>
          </div>
        </CardContent>
      </Card>

      {/* Reject Dialog */}
      <Dialog open={rejectOpen} onOpenChange={setRejectOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Reject Invoice</DialogTitle>
            <DialogDescription>
              Provide a reason for rejecting invoice {invoice.invoiceNumber}.
            </DialogDescription>
          </DialogHeader>
          <form onSubmit={handleReject} className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="rejectReason">Reason</Label>
              <Input
                id="rejectReason"
                value={rejectReason}
                onChange={(e) => setRejectReason(e.target.value)}
                placeholder="e.g. Incorrect line items, duplicate invoice"
                required
              />
            </div>
            <DialogFooter>
              <Button type="button" variant="outline" onClick={() => setRejectOpen(false)}>
                Cancel
              </Button>
              <Button
                type="submit"
                variant="destructive"
                disabled={actionLoading || !rejectReason.trim()}
              >
                {actionLoading ? 'Rejecting...' : 'Reject Invoice'}
              </Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>

      {/* Dispute Dialog */}
      <Dialog open={disputeOpen} onOpenChange={setDisputeOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Dispute Invoice</DialogTitle>
            <DialogDescription>
              Provide a reason for disputing invoice {invoice.invoiceNumber}.
            </DialogDescription>
          </DialogHeader>
          <form onSubmit={handleDispute} className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="disputeReason">Reason</Label>
              <Input
                id="disputeReason"
                value={disputeReason}
                onChange={(e) => setDisputeReason(e.target.value)}
                placeholder="e.g. Rate exceeds agreement, service not delivered"
                required
              />
            </div>
            <DialogFooter>
              <Button type="button" variant="outline" onClick={() => setDisputeOpen(false)}>
                Cancel
              </Button>
              <Button type="submit" disabled={actionLoading || !disputeReason.trim()}>
                {actionLoading ? 'Disputing...' : 'Dispute Invoice'}
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
  icon: typeof FileText;
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
