import { useState } from 'react';
import { Link, useNavigate } from 'react-router';
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
import { useInvoices } from '@/hooks/useInvoices';
import { ErrorBanner } from '@/components/ErrorBanner';
import { FileText, Plus } from 'lucide-react';
import type { InvoiceStatus } from '@/types/api';

const statusConfig: Record<InvoiceStatus, { label: string; className: string }> = {
  submitted: { label: 'Submitted', className: 'border-blue-200 bg-blue-50 text-blue-700' },
  under_review: { label: 'Under Review', className: 'border-yellow-200 bg-yellow-50 text-yellow-700' },
  approved: { label: 'Approved', className: 'border-emerald-200 bg-emerald-50 text-emerald-700' },
  rejected: { label: 'Rejected', className: 'border-red-200 bg-red-50 text-red-700' },
  disputed: { label: 'Disputed', className: 'border-orange-200 bg-orange-50 text-orange-700' },
  paid: { label: 'Paid', className: 'border-purple-200 bg-purple-50 text-purple-700' },
};

const statusTabs: { value: string; label: string }[] = [
  { value: '', label: 'All' },
  { value: 'submitted', label: 'Submitted' },
  { value: 'under_review', label: 'Under Review' },
  { value: 'approved', label: 'Approved' },
  { value: 'rejected', label: 'Rejected' },
  { value: 'disputed', label: 'Disputed' },
  { value: 'paid', label: 'Paid' },
];

export function InvoicesListPage() {
  const [statusFilter, setStatusFilter] = useState('');
  const [page, setPage] = useState(1);
  const pageSize = 20;
  const { invoices, totalCount, isLoading, error, refetch } = useInvoices(
    page,
    pageSize,
    statusFilter || undefined,
  );
  const navigate = useNavigate();
  const totalPages = Math.ceil(totalCount / pageSize);

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
          <h1 className="text-2xl font-bold tracking-tight">Invoices</h1>
        </div>
        <ErrorBanner message={error.message} onRetry={refetch} />
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Invoices</h1>
          {totalCount > 0 && (
            <p className="text-sm text-muted-foreground">
              {totalCount} invoice{totalCount !== 1 ? 's' : ''} total
            </p>
          )}
        </div>
        <Button asChild>
          <Link to="/invoices/new">
            <Plus className="mr-2 h-4 w-4" />
            New Invoice
          </Link>
        </Button>
      </div>

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

      {invoices.length === 0 && !statusFilter ? (
        <Card>
          <CardContent className="flex flex-col items-center justify-center py-16">
            <div className="rounded-full bg-primary/10 p-4">
              <FileText className="h-8 w-8 text-primary" />
            </div>
            <h3 className="mt-4 text-lg font-semibold">No invoices yet</h3>
            <p className="mt-1 max-w-sm text-center text-sm text-muted-foreground">
              Get started by creating your first invoice. Invoices track provider charges for NDIS
              services delivered to participants.
            </p>
            <Button className="mt-6" asChild>
              <Link to="/invoices/new">
                <Plus className="mr-2 h-4 w-4" />
                Create Your First Invoice
              </Link>
            </Button>
          </CardContent>
        </Card>
      ) : (
        <>
          <div className="rounded-lg border">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Invoice #</TableHead>
                  <TableHead>Provider</TableHead>
                  <TableHead>Participant</TableHead>
                  <TableHead>Service Period</TableHead>
                  <TableHead className="text-right">Amount</TableHead>
                  <TableHead>Status</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {invoices.map((inv) => {
                  const status = statusConfig[inv.status] || statusConfig.submitted;
                  return (
                    <TableRow
                      key={inv.id}
                      className="cursor-pointer transition-colors hover:bg-accent/50"
                      onClick={() => navigate(`/invoices/${inv.id}`)}
                    >
                      <TableCell className="font-mono font-medium">{inv.invoiceNumber}</TableCell>
                      <TableCell>{inv.providerName}</TableCell>
                      <TableCell>{inv.participantName}</TableCell>
                      <TableCell className="text-muted-foreground">
                        {new Date(inv.servicePeriodStart).toLocaleDateString('en-AU')} &ndash;{' '}
                        {new Date(inv.servicePeriodEnd).toLocaleDateString('en-AU')}
                      </TableCell>
                      <TableCell className="text-right font-medium">
                        ${inv.totalAmount.toLocaleString('en-AU', { minimumFractionDigits: 2 })}
                      </TableCell>
                      <TableCell>
                        <Badge variant="secondary" className={status.className}>
                          {status.label}
                        </Badge>
                      </TableCell>
                    </TableRow>
                  );
                })}
                {invoices.length === 0 && statusFilter && (
                  <TableRow>
                    <TableCell colSpan={6} className="h-32 text-center">
                      <div className="flex flex-col items-center gap-1">
                        <FileText className="h-5 w-5 text-muted-foreground" />
                        <p className="text-sm font-medium">No invoices found</p>
                        <p className="text-xs text-muted-foreground">
                          No invoices match the selected status filter.
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
                Showing {(page - 1) * pageSize + 1}&ndash;{Math.min(page * pageSize, totalCount)} of{' '}
                {totalCount}
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
