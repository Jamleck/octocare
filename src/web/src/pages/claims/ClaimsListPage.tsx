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
import { useClaims } from '@/hooks/useClaims';
import { ErrorBanner } from '@/components/ErrorBanner';
import { Send, Plus } from 'lucide-react';
import type { ClaimStatus } from '@/types/api';

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

const statusTabs: { value: string; label: string }[] = [
  { value: '', label: 'All' },
  { value: 'draft', label: 'Draft' },
  { value: 'submitted', label: 'Submitted' },
  { value: 'accepted', label: 'Accepted' },
  { value: 'partially_rejected', label: 'Partial' },
  { value: 'rejected', label: 'Rejected' },
];

export function ClaimsListPage() {
  const [statusFilter, setStatusFilter] = useState('');
  const [page, setPage] = useState(1);
  const pageSize = 20;
  const { claims, totalCount, isLoading, error, refetch } = useClaims(
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
          <h1 className="text-2xl font-bold tracking-tight">Claims</h1>
        </div>
        <ErrorBanner message={error.message} onRetry={refetch} />
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Claims</h1>
          {totalCount > 0 && (
            <p className="text-sm text-muted-foreground">
              {totalCount} claim{totalCount !== 1 ? 's' : ''} total
            </p>
          )}
        </div>
        <Button asChild>
          <Link to="/claims/new">
            <Plus className="mr-2 h-4 w-4" />
            New Claim Batch
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

      {claims.length === 0 && !statusFilter ? (
        <Card>
          <CardContent className="flex flex-col items-center justify-center py-16">
            <div className="rounded-full bg-primary/10 p-4">
              <Send className="h-8 w-8 text-primary" />
            </div>
            <h3 className="mt-4 text-lg font-semibold">No claims yet</h3>
            <p className="mt-1 max-w-sm text-center text-sm text-muted-foreground">
              Create a claim batch from approved invoice line items to submit to the NDIA for
              payment.
            </p>
            <Button className="mt-6" asChild>
              <Link to="/claims/new">
                <Plus className="mr-2 h-4 w-4" />
                Create First Claim Batch
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
                  <TableHead>Batch Number</TableHead>
                  <TableHead>Line Items</TableHead>
                  <TableHead className="text-right">Total Amount</TableHead>
                  <TableHead>Submission Date</TableHead>
                  <TableHead>NDIA Reference</TableHead>
                  <TableHead>Status</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {claims.map((claim) => {
                  const status = statusConfig[claim.status] || statusConfig.draft;
                  return (
                    <TableRow
                      key={claim.id}
                      className="cursor-pointer transition-colors hover:bg-accent/50"
                      onClick={() => navigate(`/claims/${claim.id}`)}
                    >
                      <TableCell className="font-mono font-medium">{claim.batchNumber}</TableCell>
                      <TableCell>{claim.lineItems.length}</TableCell>
                      <TableCell className="text-right font-medium">
                        ${claim.totalAmount.toLocaleString('en-AU', { minimumFractionDigits: 2 })}
                      </TableCell>
                      <TableCell className="text-muted-foreground">
                        {claim.submissionDate
                          ? new Date(claim.submissionDate).toLocaleDateString('en-AU')
                          : '--'}
                      </TableCell>
                      <TableCell className="font-mono text-sm text-muted-foreground">
                        {claim.ndiaReference || '--'}
                      </TableCell>
                      <TableCell>
                        <Badge variant="secondary" className={status.className}>
                          {status.label}
                        </Badge>
                      </TableCell>
                    </TableRow>
                  );
                })}
                {claims.length === 0 && statusFilter && (
                  <TableRow>
                    <TableCell colSpan={6} className="h-32 text-center">
                      <div className="flex flex-col items-center gap-1">
                        <Send className="h-5 w-5 text-muted-foreground" />
                        <p className="text-sm font-medium">No claims found</p>
                        <p className="text-xs text-muted-foreground">
                          No claims match the selected status filter.
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
