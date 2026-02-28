import { useState } from 'react';
import { Link, useParams, useNavigate } from 'react-router';
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
import { createServiceAgreement } from '@/hooks/useServiceAgreements';
import { usePlans } from '@/hooks/usePlans';
import { useProviders } from '@/hooks/useProviders';
import { ErrorBanner } from '@/components/ErrorBanner';
import { ArrowLeft, ClipboardList, Plus, Trash2 } from 'lucide-react';

interface LineItem {
  supportItemNumber: string;
  agreedRate: string;
  frequency: string;
}

export function AgreementCreatePage() {
  const { participantId } = useParams<{ participantId: string }>();
  const navigate = useNavigate();
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  const { plans, isLoading: plansLoading } = usePlans(participantId!);
  const { providers, isLoading: providersLoading } = useProviders();

  const [providerId, setProviderId] = useState('');
  const [planId, setPlanId] = useState('');
  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');
  const [items, setItems] = useState<LineItem[]>([
    { supportItemNumber: '', agreedRate: '', frequency: '' },
  ]);

  const addItem = () => {
    setItems([...items, { supportItemNumber: '', agreedRate: '', frequency: '' }]);
  };

  const removeItem = (index: number) => {
    if (items.length <= 1) return;
    setItems(items.filter((_, i) => i !== index));
  };

  const updateItem = (index: number, field: keyof LineItem, value: string) => {
    const updated = [...items];
    updated[index] = { ...updated[index], [field]: value };
    setItems(updated);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    if (!providerId) {
      setError('Provider is required.');
      return;
    }
    if (!planId) {
      setError('Plan is required.');
      return;
    }
    if (!startDate || !endDate) {
      setError('Start date and end date are required.');
      return;
    }
    if (startDate >= endDate) {
      setError('End date must be after start date.');
      return;
    }

    const validItems = items.filter((item) => item.supportItemNumber.trim());
    if (validItems.length === 0) {
      setError('At least one service item is required.');
      return;
    }

    for (const item of validItems) {
      const rate = parseFloat(item.agreedRate);
      if (isNaN(rate) || rate <= 0) {
        setError('All items must have a rate greater than zero.');
        return;
      }
    }

    try {
      setSubmitting(true);
      const agreement = await createServiceAgreement(participantId!, {
        providerId,
        planId,
        startDate,
        endDate,
        items: validItems.map((item) => ({
          supportItemNumber: item.supportItemNumber.trim(),
          agreedRate: parseFloat(item.agreedRate),
          frequency: item.frequency.trim() || undefined,
        })),
      });
      navigate(`/agreements/${agreement.id}`);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to create service agreement.');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="space-y-6">
      <div>
        <Button variant="ghost" size="sm" className="-ml-2 mb-2 text-muted-foreground" asChild>
          <Link to={`/participants/${participantId}`}>
            <ArrowLeft className="mr-1 h-4 w-4" />
            Back to Participant
          </Link>
        </Button>
        <h1 className="text-2xl font-bold tracking-tight">Create Service Agreement</h1>
        <p className="text-sm text-muted-foreground">
          Set up a service agreement with a provider for this participant.
        </p>
      </div>

      {error && <ErrorBanner message={error} />}

      <form onSubmit={handleSubmit} className="space-y-6">
        <Card className="max-w-2xl">
          <CardHeader>
            <CardTitle className="flex items-center gap-2 text-base">
              <ClipboardList className="h-4 w-4 text-muted-foreground" />
              Agreement Details
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
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
              <Select value={planId} onValueChange={setPlanId} disabled={plansLoading}>
                <SelectTrigger>
                  <SelectValue placeholder="Select a plan" />
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
                <Label htmlFor="startDate">Start Date</Label>
                <Input
                  id="startDate"
                  type="date"
                  value={startDate}
                  onChange={(e) => setStartDate(e.target.value)}
                  required
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="endDate">End Date</Label>
                <Input
                  id="endDate"
                  type="date"
                  value={endDate}
                  onChange={(e) => setEndDate(e.target.value)}
                  required
                />
              </div>
            </div>
          </CardContent>
        </Card>

        <Card className="max-w-2xl">
          <CardHeader>
            <div className="flex items-center justify-between">
              <CardTitle className="flex items-center gap-2 text-base">
                <ClipboardList className="h-4 w-4 text-muted-foreground" />
                Service Items
              </CardTitle>
              <Button type="button" variant="outline" size="sm" onClick={addItem}>
                <Plus className="mr-1 h-3 w-3" />
                Add Item
              </Button>
            </div>
          </CardHeader>
          <CardContent className="space-y-4">
            {items.map((item, index) => (
              <div key={index} className="flex items-end gap-3 rounded-lg border p-3">
                <div className="flex-1 space-y-2">
                  <Label>Support Item Number</Label>
                  <Input
                    value={item.supportItemNumber}
                    onChange={(e) => updateItem(index, 'supportItemNumber', e.target.value)}
                    placeholder="e.g. 01_002_0107_1_1"
                  />
                </div>
                <div className="w-32 space-y-2">
                  <Label>Rate ($)</Label>
                  <Input
                    type="number"
                    step="0.01"
                    min="0.01"
                    value={item.agreedRate}
                    onChange={(e) => updateItem(index, 'agreedRate', e.target.value)}
                    placeholder="84.45"
                  />
                </div>
                <div className="w-32 space-y-2">
                  <Label>Frequency</Label>
                  <Input
                    value={item.frequency}
                    onChange={(e) => updateItem(index, 'frequency', e.target.value)}
                    placeholder="weekly"
                  />
                </div>
                {items.length > 1 && (
                  <Button
                    type="button"
                    variant="ghost"
                    size="sm"
                    className="text-destructive hover:text-destructive"
                    onClick={() => removeItem(index)}
                  >
                    <Trash2 className="h-4 w-4" />
                  </Button>
                )}
              </div>
            ))}
          </CardContent>
        </Card>

        <div className="flex gap-2">
          <Button type="submit" disabled={submitting}>
            {submitting ? 'Creating...' : 'Create Agreement'}
          </Button>
          <Button type="button" variant="outline" asChild>
            <Link to={`/participants/${participantId}`}>Cancel</Link>
          </Button>
        </div>
      </form>
    </div>
  );
}
