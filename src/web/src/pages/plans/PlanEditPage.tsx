import { useState, useEffect } from 'react';
import { Link, useParams, useNavigate } from 'react-router';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Skeleton } from '@/components/ui/skeleton';
import { usePlan, updatePlan } from '@/hooks/usePlans';
import { ErrorBanner } from '@/components/ErrorBanner';
import { ArrowLeft, FileText } from 'lucide-react';

export function PlanEditPage() {
  const { id } = useParams<{ id: string }>();
  const { plan, isLoading } = usePlan(id!);
  const navigate = useNavigate();
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  const [planNumber, setPlanNumber] = useState('');
  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');

  useEffect(() => {
    if (plan) {
      setPlanNumber(plan.planNumber);
      setStartDate(plan.startDate);
      setEndDate(plan.endDate);
    }
  }, [plan]);

  if (isLoading) {
    return (
      <div className="space-y-4">
        <Skeleton className="h-5 w-32" />
        <Skeleton className="h-8 w-64" />
        <Skeleton className="h-48 rounded-lg" />
      </div>
    );
  }

  if (!plan) {
    return (
      <div className="flex flex-col items-center py-16">
        <FileText className="h-10 w-10 text-muted-foreground" />
        <p className="mt-3 text-lg font-medium">Plan not found</p>
        <Button variant="outline" className="mt-4" onClick={() => navigate(-1)}>
          <ArrowLeft className="mr-2 h-4 w-4" />
          Go Back
        </Button>
      </div>
    );
  }

  if (plan.status !== 'draft') {
    return (
      <div className="flex flex-col items-center py-16">
        <FileText className="h-10 w-10 text-muted-foreground" />
        <p className="mt-3 text-lg font-medium">Only draft plans can be edited</p>
        <Button variant="outline" className="mt-4" asChild>
          <Link to={`/plans/${plan.id}`}>
            <ArrowLeft className="mr-2 h-4 w-4" />
            Back to Plan
          </Link>
        </Button>
      </div>
    );
  }

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
      await updatePlan(plan.id, {
        planNumber: planNumber.trim(),
        startDate,
        endDate,
      });
      navigate(`/plans/${plan.id}`);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to update plan.');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="space-y-6">
      <div>
        <Button variant="ghost" size="sm" className="-ml-2 mb-2 text-muted-foreground" asChild>
          <Link to={`/plans/${plan.id}`}>
            <ArrowLeft className="mr-1 h-4 w-4" />
            Back to Plan
          </Link>
        </Button>
        <h1 className="text-2xl font-bold tracking-tight">Edit Plan</h1>
        <p className="text-sm text-muted-foreground">
          Update the NDIS plan details.
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
                {submitting ? 'Saving...' : 'Save Changes'}
              </Button>
              <Button type="button" variant="outline" asChild>
                <Link to={`/plans/${plan.id}`}>Cancel</Link>
              </Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}
