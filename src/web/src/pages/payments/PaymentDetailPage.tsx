import { useState } from 'react';
import { Link, useParams, useNavigate } from 'react-router';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import { Skeleton } from '@/components/ui/skeleton';
import {
  usePayment,
  markPaymentSent,
  markPaymentConfirmed,
  getPaymentAbaUrl,
} from '@/hooks/usePayments';
import { ErrorBanner } from '@/components/ErrorBanner';
import {
  ArrowLeft,
  Banknote,
  Calendar,
  Hash,
  DollarSign,
  Download,
  Send,
  CheckCircle,
} from 'lucide-react';
import type { PaymentBatchStatus } from '@/types/api';

const statusConfig: Record<PaymentBatchStatus, { label: string; className: string }> = {
  draft: { label: 'Draft', className: 'border-gray-200 bg-gray-50 text-gray-700' },
  generated: { label: 'Generated', className: 'border-blue-200 bg-blue-50 text-blue-700' },
  sent: { label: 'Sent', className: 'border-amber-200 bg-amber-50 text-amber-700' },
  confirmed: {
    label: 'Confirmed',
    className: 'border-emerald-200 bg-emerald-50 text-emerald-700',
  },
};

export function PaymentDetailPage() {
  const { id } = useParams<{ id: string }>();
  const { payment, isLoading, error, refetch } = usePayment(id!);
  const navigate = useNavigate();

  const [actionLoading, setActionLoading] = useState(false);
  const [actionError, setActionError] = useState<string | null>(null);

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

  if (!payment) {
    return (
      <div className="flex flex-col items-center py-16">
        <Banknote className="h-10 w-10 text-muted-foreground" />
        <p className="mt-3 text-lg font-medium">Payment batch not found</p>
        <Button variant="outline" className="mt-4" onClick={() => navigate(-1)}>
          <ArrowLeft className="mr-2 h-4 w-4" />
          Go Back
        </Button>
      </div>
    );
  }

  const status = statusConfig[payment.status] || statusConfig.draft;
  const canDownloadAba = true; // Always allow ABA download (will generate if needed)
  const canMarkSent = payment.status === 'generated';
  const canMarkConfirmed = payment.status === 'sent';

  const handleDownloadAba = () => {
    window.open(getPaymentAbaUrl(payment.id), '_blank');
  };

  const handleMarkSent = async () => {
    try {
      setActionLoading(true);
      setActionError(null);
      await markPaymentSent(payment.id);
      refetch();
    } catch (err) {
      setActionError(err instanceof Error ? err.message : 'Failed to mark as sent.');
    } finally {
      setActionLoading(false);
    }
  };

  const handleMarkConfirmed = async () => {
    try {
      setActionLoading(true);
      setActionError(null);
      await markPaymentConfirmed(payment.id);
      refetch();
    } catch (err) {
      setActionError(err instanceof Error ? err.message : 'Failed to confirm payment.');
    } finally {
      setActionLoading(false);
    }
  };

  return (
    <div className="space-y-6">
      <div>
        <Button variant="ghost" size="sm" className="-ml-2 mb-2 text-muted-foreground" asChild>
          <Link to="/payments">
            <ArrowLeft className="mr-1 h-4 w-4" />
            Payments
          </Link>
        </Button>

        <div className="flex items-center justify-between">
          <div className="flex items-center gap-3">
            <div className="flex h-10 w-10 items-center justify-center rounded-full bg-primary/10 text-primary">
              <Banknote className="h-5 w-5" />
            </div>
            <div>
              <h1 className="text-2xl font-bold tracking-tight">{payment.batchNumber}</h1>
              <p className="text-sm text-muted-foreground">
                {payment.items.length} provider{payment.items.length !== 1 ? 's' : ''}
              </p>
            </div>
            <Badge variant="secondary" className={status.className}>
              {status.label}
            </Badge>
          </div>
          <div className="flex gap-2">
            {canDownloadAba && (
              <Button onClick={handleDownloadAba} variant="outline">
                <Download className="mr-2 h-4 w-4" />
                Download ABA
              </Button>
            )}
            {canMarkSent && (
              <Button onClick={handleMarkSent} disabled={actionLoading}>
                <Send className="mr-2 h-4 w-4" />
                Mark Sent
              </Button>
            )}
            {canMarkConfirmed && (
              <Button onClick={handleMarkConfirmed} disabled={actionLoading}>
                <CheckCircle className="mr-2 h-4 w-4" />
                Confirm Payment
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
              <Banknote className="h-4 w-4 text-muted-foreground" />
              Batch Details
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <DetailRow icon={Hash} label="Batch Number" value={payment.batchNumber} mono />
            <DetailRow
              icon={DollarSign}
              label="Total Amount"
              value={`$${payment.totalAmount.toLocaleString('en-AU', { minimumFractionDigits: 2 })}`}
            />
            <DetailRow
              icon={Calendar}
              label="Created"
              value={new Date(payment.createdAt).toLocaleDateString('en-AU', {
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
              <Send className="h-4 w-4 text-muted-foreground" />
              Payment Status
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <DetailRow
              icon={Calendar}
              label="Sent Date"
              value={
                payment.sentAt
                  ? new Date(payment.sentAt).toLocaleDateString('en-AU', {
                      year: 'numeric',
                      month: 'long',
                      day: 'numeric',
                    })
                  : undefined
              }
            />
            <DetailRow
              icon={Calendar}
              label="Confirmed Date"
              value={
                payment.confirmedAt
                  ? new Date(payment.confirmedAt).toLocaleDateString('en-AU', {
                      year: 'numeric',
                      month: 'long',
                      day: 'numeric',
                    })
                  : undefined
              }
            />
          </CardContent>
        </Card>
      </div>

      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2 text-base">
            <DollarSign className="h-4 w-4 text-muted-foreground" />
            Payment Items ({payment.items.length})
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="rounded-lg border">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Provider</TableHead>
                  <TableHead>Invoices</TableHead>
                  <TableHead className="text-right">Amount</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {payment.items.map((item) => (
                  <TableRow key={item.id}>
                    <TableCell className="font-medium">{item.providerName}</TableCell>
                    <TableCell className="text-muted-foreground">
                      {item.invoiceCount} invoice{item.invoiceCount !== 1 ? 's' : ''}
                    </TableCell>
                    <TableCell className="text-right font-medium">
                      ${item.amount.toLocaleString('en-AU', { minimumFractionDigits: 2 })}
                    </TableCell>
                  </TableRow>
                ))}
                <TableRow className="bg-muted/30 font-medium">
                  <TableCell colSpan={2} className="text-right">
                    Total
                  </TableCell>
                  <TableCell className="text-right">
                    ${payment.totalAmount.toLocaleString('en-AU', { minimumFractionDigits: 2 })}
                  </TableCell>
                </TableRow>
              </TableBody>
            </Table>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}

function DetailRow({
  icon: Icon,
  label,
  value,
  mono,
}: {
  icon: typeof Banknote;
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
