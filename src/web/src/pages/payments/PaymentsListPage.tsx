import { useState } from 'react';
import { useNavigate } from 'react-router';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Card, CardContent } from '@/components/ui/card';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import { Skeleton } from '@/components/ui/skeleton';
import { usePayments, createPaymentBatch } from '@/hooks/usePayments';
import { ErrorBanner } from '@/components/ErrorBanner';
import { Banknote, Plus } from 'lucide-react';
import type { PaymentBatchStatus } from '@/types/api';

const statusConfig: Record<PaymentBatchStatus, { label: string; className: string }> = {
  draft: { label: 'Draft', className: 'border-gray-200 bg-gray-50 text-gray-700' },
  generated: { label: 'Generated', className: 'border-blue-200 bg-blue-50 text-blue-700' },
  sent: { label: 'Sent', className: 'border-amber-200 bg-amber-50 text-amber-700' },
  confirmed: { label: 'Confirmed', className: 'border-emerald-200 bg-emerald-50 text-emerald-700' },
};

const statusTabs: { value: string; label: string }[] = [
  { value: '', label: 'All' },
  { value: 'draft', label: 'Draft' },
  { value: 'generated', label: 'Generated' },
  { value: 'sent', label: 'Sent' },
  { value: 'confirmed', label: 'Confirmed' },
];

export function PaymentsListPage() {
  const [statusFilter, setStatusFilter] = useState('');
  const [page, setPage] = useState(1);
  const [creating, setCreating] = useState(false);
  const [createError, setCreateError] = useState<string | null>(null);
  const pageSize = 20;
  const { payments, totalCount, isLoading, error, refetch } = usePayments(
    page,
    pageSize,
    statusFilter || undefined,
  );
  const navigate = useNavigate();
  const totalPages = Math.ceil(totalCount / pageSize);

  const handleCreateBatch = async () => {
    try {
      setCreating(true);
      setCreateError(null);
      const batch = await createPaymentBatch();
      navigate(`/payments/${batch.id}`);
    } catch (err) {
      setCreateError(
        err instanceof Error ? err.message : 'Failed to create payment batch.',
      );
    } finally {
      setCreating(false);
    }
  };

  if (isLoading) {
    return (
      <div className="space-y-4">
        <div className="flex items-center justify-between">
          <Skeleton className="h-8 w-48" />
          <Skeleton className="h-9 w-36" />
        </div>
        <Skeleton className="h-10 w-full" />
        <Skeleton className="h-64 w-full rounded-lg" />
      </div>
    );
  }

  if (error) {
    return (
      <div className="space-y-6">
        <div className="flex items-center justify-between">
          <h1 className="text-2xl font-bold tracking-tight">Payments</h1>
        </div>
        <ErrorBanner message={error.message} onRetry={refetch} />
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Payments</h1>
          {totalCount > 0 && (
            <p className="text-sm text-muted-foreground">
              {totalCount} batch{totalCount !== 1 ? 'es' : ''} total
            </p>
          )}
        </div>
        <Button onClick={handleCreateBatch} disabled={creating}>
          <Plus className="mr-2 h-4 w-4" />
          {creating ? 'Creating...' : 'Create Batch'}
        </Button>
      </div>

      {createError && <ErrorBanner message={createError} />}

      {/* Status filter tabs */}
      <div className="flex gap-1 overflow-x-auto rounded-lg border bg-muted/40 p-1">
        {statusTabs.map((tab) => (
          <button
            key={tab.value}
            onClick={() => {
              setStatusFilter(tab.value);
              setPage(1);
            }}
            className={`whitespace-nowrap rounded-md px-3 py-1.5 text-sm font-medium transition-colors ${
              statusFilter === tab.value
                ? 'bg-background text-foreground shadow-sm'
                : 'text-muted-foreground hover:text-foreground'
            }`}
          >
            {tab.label}
          </button>
        ))}
      </div>

      {payments.length === 0 && !statusFilter ? (
        <Card>
          <CardContent className="flex flex-col items-center justify-center py-16">
            <div className="rounded-full bg-primary/10 p-4">
              <Banknote className="h-8 w-8 text-primary" />
            </div>
            <h3 className="mt-4 text-lg font-semibold">No payment batches yet</h3>
            <p className="mt-1 max-w-sm text-center text-sm text-muted-foreground">
              Create a payment batch from approved invoices to generate ABA files for bank
              payments to providers.
            </p>
            <Button className="mt-6" onClick={handleCreateBatch} disabled={creating}>
              <Plus className="mr-2 h-4 w-4" />
              Create First Payment Batch
            </Button>
          </CardContent>
        </Card>
      ) : (
        <>
          <div className="rounded-lg border">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Batch Number</TableHead>
                  <TableHead>Items</TableHead>
                  <TableHead className="text-right">Total Amount</TableHead>
                  <TableHead>Created</TableHead>
                  <TableHead>Sent</TableHead>
                  <TableHead>Status</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {payments.map((batch) => {
                  const status = statusConfig[batch.status] || statusConfig.draft;
                  return (
                    <TableRow
                      key={batch.id}
                      className="cursor-pointer transition-colors hover:bg-accent/50"
                      onClick={() => navigate(`/payments/${batch.id}`)}
                    >
                      <TableCell className="font-mono font-medium">
                        {batch.batchNumber}
                      </TableCell>
                      <TableCell>{batch.itemCount}</TableCell>
                      <TableCell className="text-right font-medium">
                        ${batch.totalAmount.toLocaleString('en-AU', { minimumFractionDigits: 2 })}
                      </TableCell>
                      <TableCell className="text-muted-foreground">
                        {new Date(batch.createdAt).toLocaleDateString('en-AU')}
                      </TableCell>
                      <TableCell className="text-muted-foreground">
                        {batch.sentAt
                          ? new Date(batch.sentAt).toLocaleDateString('en-AU')
                          : '--'}
                      </TableCell>
                      <TableCell>
                        <Badge variant="secondary" className={status.className}>
                          {status.label}
                        </Badge>
                      </TableCell>
                    </TableRow>
                  );
                })}
                {payments.length === 0 && statusFilter && (
                  <TableRow>
                    <TableCell colSpan={6} className="h-32 text-center">
                      <div className="flex flex-col items-center gap-1">
                        <Banknote className="h-5 w-5 text-muted-foreground" />
                        <p className="text-sm font-medium">No batches found</p>
                        <p className="text-xs text-muted-foreground">
                          No payment batches match the selected status filter.
                        </p>
                      </div>
                    </TableCell>
                  </TableRow>
                )}
              </TableBody>
            </Table>
          </div>

          {totalPages > 1 && (
            <div className="flex items-center justify-between">
              <p className="text-sm text-muted-foreground">
                Showing {(page - 1) * pageSize + 1}&ndash;
                {Math.min(page * pageSize, totalCount)} of {totalCount}
              </p>
              <div className="flex gap-2">
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => setPage((p) => Math.max(1, p - 1))}
                  disabled={page === 1}
                >
                  Previous
                </Button>
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
                  disabled={page === totalPages}
                >
                  Next
                </Button>
              </div>
            </div>
          )}
        </>
      )}
    </div>
  );
}
