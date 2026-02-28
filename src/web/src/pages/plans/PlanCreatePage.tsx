import { useState } from 'react';
import { Link, useParams, useNavigate } from 'react-router';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { createPlan } from '@/hooks/usePlans';
import { ErrorBanner } from '@/components/ErrorBanner';
import { ArrowLeft, FileText } from 'lucide-react';

export function PlanCreatePage() {
  const { participantId } = useParams<{ participantId: string }>();
  const navigate = useNavigate();
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  const [planNumber, setPlanNumber] = useState('');
  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    if (!planNumber.trim()) {
      setError('Plan number is required.');
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

    try {
      setSubmitting(true);
      const plan = await createPlan(participantId!, {
        planNumber: planNumber.trim(),
        startDate,
        endDate,
      });
      navigate(`/plans/${plan.id}`);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to create plan.');
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
        <h1 className="text-2xl font-bold tracking-tight">Create Plan</h1>
        <p className="text-sm text-muted-foreground">
          Enter the NDIS plan details for this participant.
        </p>
      </div>

      {error && <ErrorBanner message={error} />}

      <Card className="max-w-lg">
        <CardHeader>
          <CardTitle className="flex items-center gap-2 text-base">
            <FileText className="h-4 w-4 text-muted-foreground" />
            Plan Details
          </CardTitle>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit} className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="planNumber">Plan Number</Label>
              <Input
                id="planNumber"
                value={planNumber}
                onChange={(e) => setPlanNumber(e.target.value)}
                placeholder="e.g. NDIS-2026-001"
                required
              />
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

            <div className="flex gap-2 pt-2">
              <Button type="submit" disabled={submitting}>
                {submitting ? 'Creating...' : 'Create Plan'}
              </Button>
              <Button type="button" variant="outline" asChild>
                <Link to={`/participants/${participantId}`}>Cancel</Link>
              </Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}
