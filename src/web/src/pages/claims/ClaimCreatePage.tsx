import { useState } from 'react';
import { Link, useNavigate } from 'react-router';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Checkbox } from '@/components/ui/checkbox';
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
import { createClaim } from '@/hooks/useClaims';
import { ErrorBanner } from '@/components/ErrorBanner';
import { ArrowLeft, Send, FileText } from 'lucide-react';

export function ClaimCreatePage() {
  const navigate = useNavigate();
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);
  const [selectedLineItemIds, setSelectedLineItemIds] = useState<Set<string>>(new Set());

  // Fetch approved invoices
  const { invoices, isLoading } = useInvoices(1, 100, 'approved');

  const toggleLineItem = (id: string) => {
    setSelectedLineItemIds((prev) => {
      const next = new Set(prev);
      if (next.has(id)) {
        next.delete(id);
      } else {
        next.add(id);
      }
      return next;
    });
  };

  const toggleAllForInvoice = (invoiceLineItemIds: string[]) => {
    setSelectedLineItemIds((prev) => {
      const next = new Set(prev);
      const allSelected = invoiceLineItemIds.every((id) => next.has(id));
      if (allSelected) {
        invoiceLineItemIds.forEach((id) => next.delete(id));
      } else {
        invoiceLineItemIds.forEach((id) => next.add(id));
      }
      return next;
    });
  };

  const selectedTotal = invoices
    .flatMap((inv) => inv.lineItems)
    .filter((li) => selectedLineItemIds.has(li.id))
    .reduce((sum, li) => sum + li.amount, 0);

  const handleSubmit = async () => {
    setError(null);

    if (selectedLineItemIds.size === 0) {
      setError('Select at least one line item to include in the claim batch.');
      return;
    }

    try {
      setSubmitting(true);
      const claim = await createClaim({
        invoiceLineItemIds: Array.from(selectedLineItemIds),
      });
      navigate(`/claims/${claim.id}`);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to create claim batch.');
    } finally {
      setSubmitting(false);
    }
  };

  if (isLoading) {
    return (
      <div className="space-y-4">
        <Skeleton className="h-5 w-32" />
        <Skeleton className="h-8 w-64" />
        <Skeleton className="h-64 w-full rounded-lg" />
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div>
        <Button variant="ghost" size="sm" className="-ml-2 mb-2 text-muted-foreground" asChild>
          <Link to="/claims">
            <ArrowLeft className="mr-1 h-4 w-4" />
            Back to Claims
          </Link>
        </Button>
        <h1 className="text-2xl font-bold tracking-tight">Create Claim Batch</h1>
        <p className="text-sm text-muted-foreground">
          Select line items from approved invoices to include in a new NDIA claim batch.
        </p>
      </div>

      {error && <ErrorBanner message={error} />}

      {invoices.length === 0 ? (
        <Card>
          <CardContent className="flex flex-col items-center justify-center py-16">
            <div className="rounded-full bg-muted p-4">
              <FileText className="h-8 w-8 text-muted-foreground" />
            </div>
            <h3 className="mt-4 text-lg font-semibold">No approved invoices</h3>
            <p className="mt-1 max-w-sm text-center text-sm text-muted-foreground">
              There are no approved invoices available. Approve invoices first before creating claim
              batches.
            </p>
            <Button className="mt-6" variant="outline" asChild>
              <Link to="/invoices">View Invoices</Link>
            </Button>
          </CardContent>
        </Card>
      ) : (
        <>
          {invoices.map((invoice) => {
            const invoiceLineItemIds = invoice.lineItems.map((li) => li.id);
            const allSelected = invoiceLineItemIds.every((id) => selectedLineItemIds.has(id));
            const someSelected =
              !allSelected && invoiceLineItemIds.some((id) => selectedLineItemIds.has(id));

            return (
              <Card key={invoice.id}>
                <CardHeader>
                  <div className="flex items-center justify-between">
                    <CardTitle className="flex items-center gap-2 text-base">
                      <FileText className="h-4 w-4 text-muted-foreground" />
                      {invoice.invoiceNumber} &mdash; {invoice.providerName}
                    </CardTitle>
                    <div className="flex items-center gap-2">
                      <span className="text-sm text-muted-foreground">
                        {invoice.participantName}
                      </span>
                      <Button
                        variant="outline"
                        size="sm"
                        onClick={() => toggleAllForInvoice(invoiceLineItemIds)}
                      >
                        {allSelected ? 'Deselect All' : someSelected ? 'Select All' : 'Select All'}
                      </Button>
                    </div>
                  </div>
                </CardHeader>
                <CardContent>
                  <div className="rounded-lg border">
                    <Table>
                      <TableHeader>
                        <TableRow>
                          <TableHead className="w-12" />
                          <TableHead>Support Item</TableHead>
                          <TableHead>Description</TableHead>
                          <TableHead>Service Date</TableHead>
                          <TableHead className="text-right">Qty</TableHead>
                          <TableHead className="text-right">Rate</TableHead>
                          <TableHead className="text-right">Amount</TableHead>
                        </TableRow>
                      </TableHeader>
                      <TableBody>
                        {invoice.lineItems.map((li) => (
                          <TableRow
                            key={li.id}
                            className="cursor-pointer"
                            onClick={() => toggleLineItem(li.id)}
                          >
                            <TableCell>
                              <Checkbox
                                checked={selectedLineItemIds.has(li.id)}
                                onCheckedChange={() => toggleLineItem(li.id)}
                              />
                            </TableCell>
                            <TableCell className="font-mono text-sm">
                              {li.supportItemNumber}
                            </TableCell>
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
                          </TableRow>
                        ))}
                      </TableBody>
                    </Table>
                  </div>
                </CardContent>
              </Card>
            );
          })}

          <div className="sticky bottom-0 rounded-lg border bg-card p-4 shadow-lg">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-muted-foreground">
                  {selectedLineItemIds.size} line item{selectedLineItemIds.size !== 1 ? 's' : ''}{' '}
                  selected
                </p>
                <p className="text-xl font-bold">
                  ${selectedTotal.toLocaleString('en-AU', { minimumFractionDigits: 2 })}
                </p>
              </div>
              <div className="flex gap-2">
                <Button variant="outline" asChild>
                  <Link to="/claims">Cancel</Link>
                </Button>
                <Button
                  onClick={handleSubmit}
                  disabled={submitting || selectedLineItemIds.size === 0}
                >
                  <Send className="mr-2 h-4 w-4" />
                  {submitting ? 'Creating...' : 'Create Claim Batch'}
                </Button>
              </div>
            </div>
          </div>
        </>
      )}
    </div>
  );
}
