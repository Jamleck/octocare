import { useState } from 'react';
import { Link, useNavigate } from 'react-router';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { createInvoice } from '@/hooks/useInvoices';
import { useParticipants } from '@/hooks/useParticipants';
import { useProviders } from '@/hooks/useProviders';
import { usePlans } from '@/hooks/usePlans';
import { ErrorBanner } from '@/components/ErrorBanner';
import { ArrowLeft, FileText, Plus, Trash2 } from 'lucide-react';

interface LineItemForm {
  supportItemNumber: string;
  description: string;
  serviceDate: string;
  quantity: string;
  rate: string;
}

export function InvoiceCreatePage() {
  const navigate = useNavigate();
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  const { participants, isLoading: participantsLoading } = useParticipants(1, 100);
  const { providers, isLoading: providersLoading } = useProviders(1, 100);

  const [participantId, setParticipantId] = useState('');
  const [providerId, setProviderId] = useState('');
  const [planId, setPlanId] = useState('');
  const [invoiceNumber, setInvoiceNumber] = useState('');
  const [servicePeriodStart, setServicePeriodStart] = useState('');
  const [servicePeriodEnd, setServicePeriodEnd] = useState('');
  const [notes, setNotes] = useState('');

  // Plans depend on selected participant
  const { plans, isLoading: plansLoading } = usePlans(participantId || '__none__');

  const [lineItems, setLineItems] = useState<LineItemForm[]>([
    { supportItemNumber: '', description: '', serviceDate: '', quantity: '1', rate: '' },
  ]);

  const addLineItem = () => {
    setLineItems([
      ...lineItems,
      { supportItemNumber: '', description: '', serviceDate: '', quantity: '1', rate: '' },
    ]);
  };

  const removeLineItem = (index: number) => {
    if (lineItems.length <= 1) return;
    setLineItems(lineItems.filter((_, i) => i !== index));
  };

  const updateLineItem = (index: number, field: keyof LineItemForm, value: string) => {
    const updated = [...lineItems];
    updated[index] = { ...updated[index], [field]: value };
    setLineItems(updated);
  };

  const calculateLineTotal = (item: LineItemForm) => {
    const qty = parseFloat(item.quantity);
    const rate = parseFloat(item.rate);
    if (isNaN(qty) || isNaN(rate)) return 0;
    return qty * rate;
  };

  const grandTotal = lineItems.reduce((sum, item) => sum + calculateLineTotal(item), 0);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    if (!participantId) {
      setError('Participant is required.');
      return;
    }
    if (!providerId) {
      setError('Provider is required.');
      return;
    }
    if (!planId) {
      setError('Plan is required.');
      return;
    }
    if (!invoiceNumber.trim()) {
      setError('Invoice number is required.');
      return;
    }
    if (!servicePeriodStart || !servicePeriodEnd) {
      setError('Service period start and end dates are required.');
      return;
    }
    if (servicePeriodStart >= servicePeriodEnd) {
      setError('Service period end must be after start.');
      return;
    }

    const validItems = lineItems.filter(
      (item) => item.supportItemNumber.trim() && item.description.trim(),
    );
    if (validItems.length === 0) {
      setError('At least one line item is required.');
      return;
    }

    for (const item of validItems) {
      const qty = parseFloat(item.quantity);
      const rate = parseFloat(item.rate);
      if (isNaN(qty) || qty <= 0) {
        setError('All line items must have a quantity greater than zero.');
        return;
      }
      if (isNaN(rate) || rate <= 0) {
        setError('All line items must have a rate greater than zero.');
        return;
      }
      if (!item.serviceDate) {
        setError('All line items must have a service date.');
        return;
      }
    }

    try {
      setSubmitting(true);
      const invoice = await createInvoice({
        participantId,
        providerId,
        planId,
        invoiceNumber: invoiceNumber.trim(),
        servicePeriodStart,
        servicePeriodEnd,
        notes: notes.trim() || undefined,
        lineItems: validItems.map((item) => ({
          supportItemNumber: item.supportItemNumber.trim(),
          description: item.description.trim(),
          serviceDate: item.serviceDate,
          quantity: parseFloat(item.quantity),
          rate: parseFloat(item.rate),
        })),
      });
      navigate(`/invoices/${invoice.id}`);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to create invoice.');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="space-y-6">
      <div>
        <Button variant="ghost" size="sm" className="-ml-2 mb-2 text-muted-foreground" asChild>
          <Link to="/invoices">
            <ArrowLeft className="mr-1 h-4 w-4" />
            Back to Invoices
          </Link>
        </Button>
        <h1 className="text-2xl font-bold tracking-tight">Create Invoice</h1>
        <p className="text-sm text-muted-foreground">
          Enter invoice details from a service provider.
        </p>
      </div>

      {error && <ErrorBanner message={error} />}

      <form onSubmit={handleSubmit} className="space-y-6">
        <Card className="max-w-2xl">
          <CardHeader>
            <CardTitle className="flex items-center gap-2 text-base">
              <FileText className="h-4 w-4 text-muted-foreground" />
              Invoice Details
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="invoiceNumber">Invoice Number</Label>
              <Input
                id="invoiceNumber"
                value={invoiceNumber}
                onChange={(e) => setInvoiceNumber(e.target.value)}
                placeholder="e.g. INV-2025-001"
                required
              />
            </div>

            <div className="space-y-2">
              <Label>Participant</Label>
              <Select
                value={participantId}
                onValueChange={(v) => {
                  setParticipantId(v);
                  setPlanId(''); // Reset plan when participant changes
                }}
                disabled={participantsLoading}
              >
                <SelectTrigger>
                  <SelectValue placeholder="Select a participant" />
                </SelectTrigger>
                <SelectContent>
                  {participants.map((p) => (
                    <SelectItem key={p.id} value={p.id}>
                      {p.firstName} {p.lastName} ({p.ndisNumber})
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div className="space-y-2">
              <Label>Provider</Label>
              <Select value={providerId} onValueChange={setProviderId} disabled={providersLoading}>
                <SelectTrigger>
                  <SelectValue placeholder="Select a provider" />
                </SelectTrigger>
                <SelectContent>
                  {providers.map((p) => (
                    <SelectItem key={p.id} value={p.id}>
                      {p.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div className="space-y-2">
              <Label>Plan</Label>
              <Select
                value={planId}
                onValueChange={setPlanId}
                disabled={!participantId || plansLoading}
              >
                <SelectTrigger>
                  <SelectValue
                    placeholder={
                      !participantId ? 'Select a participant first' : 'Select a plan'
                    }
                  />
                </SelectTrigger>
                <SelectContent>
                  {plans.map((p) => (
                    <SelectItem key={p.id} value={p.id}>
                      {p.planNumber} ({p.status})
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div className="grid gap-4 sm:grid-cols-2">
              <div className="space-y-2">
                <Label htmlFor="servicePeriodStart">Service Period Start</Label>
                <Input
                  id="servicePeriodStart"
                  type="date"
                  value={servicePeriodStart}
                  onChange={(e) => setServicePeriodStart(e.target.value)}
                  required
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="servicePeriodEnd">Service Period End</Label>
                <Input
                  id="servicePeriodEnd"
                  type="date"
                  value={servicePeriodEnd}
                  onChange={(e) => setServicePeriodEnd(e.target.value)}
                  required
                />
              </div>
            </div>

            <div className="space-y-2">
              <Label htmlFor="notes">Notes (optional)</Label>
              <Input
                id="notes"
                value={notes}
                onChange={(e) => setNotes(e.target.value)}
                placeholder="Additional notes about this invoice"
              />
            </div>
          </CardContent>
        </Card>

        <Card className="max-w-3xl">
          <CardHeader>
            <div className="flex items-center justify-between">
              <CardTitle className="flex items-center gap-2 text-base">
                <FileText className="h-4 w-4 text-muted-foreground" />
                Line Items
              </CardTitle>
              <Button type="button" variant="outline" size="sm" onClick={addLineItem}>
                <Plus className="mr-1 h-3 w-3" />
                Add Line Item
              </Button>
            </div>
          </CardHeader>
          <CardContent className="space-y-4">
            {lineItems.map((item, index) => (
              <div key={index} className="space-y-3 rounded-lg border p-4">
                <div className="flex items-center justify-between">
                  <span className="text-sm font-medium text-muted-foreground">
                    Line Item {index + 1}
                  </span>
                  {lineItems.length > 1 && (
                    <Button
                      type="button"
                      variant="ghost"
                      size="sm"
                      className="text-destructive hover:text-destructive"
                      onClick={() => removeLineItem(index)}
                    >
                      <Trash2 className="h-4 w-4" />
                    </Button>
                  )}
                </div>
                <div className="grid gap-3 sm:grid-cols-2">
                  <div className="space-y-2">
                    <Label>Support Item Number</Label>
                    <Input
                      value={item.supportItemNumber}
                      onChange={(e) => updateLineItem(index, 'supportItemNumber', e.target.value)}
                      placeholder="e.g. 01_002_0107_1_1"
                    />
                  </div>
                  <div className="space-y-2">
                    <Label>Service Date</Label>
                    <Input
                      type="date"
                      value={item.serviceDate}
                      onChange={(e) => updateLineItem(index, 'serviceDate', e.target.value)}
                    />
                  </div>
                </div>
                <div className="space-y-2">
                  <Label>Description</Label>
                  <Input
                    value={item.description}
                    onChange={(e) => updateLineItem(index, 'description', e.target.value)}
                    placeholder="e.g. Assistance with Self-Care Activities"
                  />
                </div>
                <div className="grid gap-3 sm:grid-cols-3">
                  <div className="space-y-2">
                    <Label>Quantity</Label>
                    <Input
                      type="number"
                      step="0.01"
                      min="0.01"
                      value={item.quantity}
                      onChange={(e) => updateLineItem(index, 'quantity', e.target.value)}
                      placeholder="1"
                    />
                  </div>
                  <div className="space-y-2">
                    <Label>Rate ($)</Label>
                    <Input
                      type="number"
                      step="0.01"
                      min="0.01"
                      value={item.rate}
                      onChange={(e) => updateLineItem(index, 'rate', e.target.value)}
                      placeholder="84.45"
                    />
                  </div>
                  <div className="space-y-2">
                    <Label>Line Total</Label>
                    <div className="flex h-9 items-center rounded-md border bg-muted/50 px-3 text-sm font-medium">
                      ${calculateLineTotal(item).toLocaleString('en-AU', { minimumFractionDigits: 2 })}
                    </div>
                  </div>
                </div>
              </div>
            ))}

            <div className="flex justify-end border-t pt-4">
              <div className="text-right">
                <p className="text-sm text-muted-foreground">Grand Total</p>
                <p className="text-2xl font-bold">
                  ${grandTotal.toLocaleString('en-AU', { minimumFractionDigits: 2 })}
                </p>
              </div>
            </div>
          </CardContent>
        </Card>

        <div className="flex gap-2">
          <Button type="submit" disabled={submitting}>
            {submitting ? 'Creating...' : 'Submit Invoice'}
          </Button>
          <Button type="button" variant="outline" asChild>
            <Link to="/invoices">Cancel</Link>
          </Button>
        </div>
      </form>
    </div>
  );
}
