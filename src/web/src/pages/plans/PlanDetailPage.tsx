import { useState } from 'react';
import { Link, useParams, useNavigate } from 'react-router';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Skeleton } from '@/components/ui/skeleton';
import { usePlan, activatePlan, addBudgetCategory } from '@/hooks/usePlans';
import { ErrorBanner } from '@/components/ErrorBanner';
import { BudgetOverviewCard } from '@/components/BudgetOverviewCard';
import {
  ArrowLeft,
  Pencil,
  Play,
  Plus,
  FileText,
  Calendar,
  Hash,
  DollarSign,
  Layers,
} from 'lucide-react';
import type { PlanStatus, SupportCategory } from '@/types/api';

const statusConfig: Record<PlanStatus, { label: string; className: string }> = {
  draft: { label: 'Draft', className: 'border-yellow-200 bg-yellow-50 text-yellow-700' },
  active: { label: 'Active', className: 'border-emerald-200 bg-emerald-50 text-emerald-700' },
  expiring: { label: 'Expiring', className: 'border-orange-200 bg-orange-50 text-orange-700' },
  expired: { label: 'Expired', className: 'text-muted-foreground' },
  transitioned: { label: 'Transitioned', className: 'border-blue-200 bg-blue-50 text-blue-700' },
};

const categoryColors: Record<SupportCategory, string> = {
  Core: 'bg-blue-500',
  CapacityBuilding: 'bg-purple-500',
  Capital: 'bg-amber-500',
};

const categoryLabels: Record<SupportCategory, string> = {
  Core: 'Core',
  CapacityBuilding: 'Capacity Building',
  Capital: 'Capital',
};

const supportPurposes = [
  { value: 'DailyActivities', label: 'Daily Activities' },
  { value: 'TransportActivities', label: 'Transport Activities' },
  { value: 'ConsumablesAndEquipment', label: 'Consumables & Equipment' },
  { value: 'AssistiveTechnology', label: 'Assistive Technology' },
  { value: 'HomeModifications', label: 'Home Modifications' },
  { value: 'CoordinationOfSupports', label: 'Coordination of Supports' },
  { value: 'ImprovedLivingArrangements', label: 'Improved Living Arrangements' },
  { value: 'IncreasedSocialAndCommunityParticipation', label: 'Social & Community Participation' },
  { value: 'FindingAndKeepingAJob', label: 'Finding & Keeping a Job' },
  { value: 'ImprovedRelationships', label: 'Improved Relationships' },
  { value: 'ImprovedHealthAndWellbeing', label: 'Improved Health & Wellbeing' },
  { value: 'ImprovedLearning', label: 'Improved Learning' },
  { value: 'ImprovedLifeChoices', label: 'Improved Life Choices' },
  { value: 'ImprovedDailyLivingSkills', label: 'Improved Daily Living Skills' },
];

export function PlanDetailPage() {
  const { id } = useParams<{ id: string }>();
  const { plan, isLoading, error, refetch } = usePlan(id!);
  const navigate = useNavigate();
  const [activating, setActivating] = useState(false);
  const [addCategoryOpen, setAddCategoryOpen] = useState(false);
  const [categoryError, setCategoryError] = useState<string | null>(null);
  const [categorySubmitting, setCategorySubmitting] = useState(false);

  // Budget category form state
  const [newSupportCategory, setNewSupportCategory] = useState<SupportCategory | ''>('');
  const [newSupportPurpose, setNewSupportPurpose] = useState('');
  const [newAllocatedAmount, setNewAllocatedAmount] = useState('');

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
        <Button variant="ghost" size="sm" className="-ml-2 text-muted-foreground" onClick={() => navigate(-1)}>
          <ArrowLeft className="mr-1 h-4 w-4" />
          Back
        </Button>
        <ErrorBanner message={error.message} onRetry={refetch} />
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

  const status = statusConfig[plan.status] || statusConfig.draft;
  const isDraft = plan.status === 'draft';
  const totalBudget = plan.budgetCategories.reduce((sum, bc) => sum + bc.allocatedAmount, 0);

  const handleActivate = async () => {
    try {
      setActivating(true);
      await activatePlan(plan.id);
      refetch();
    } catch (err) {
      // error will be shown on next fetch
    } finally {
      setActivating(false);
    }
  };

  const handleAddBudgetCategory = async (e: React.FormEvent) => {
    e.preventDefault();
    setCategoryError(null);

    if (!newSupportCategory || !newSupportPurpose || !newAllocatedAmount) {
      setCategoryError('All fields are required.');
      return;
    }
    const amount = parseFloat(newAllocatedAmount);
    if (isNaN(amount) || amount <= 0) {
      setCategoryError('Amount must be greater than zero.');
      return;
    }

    try {
      setCategorySubmitting(true);
      await addBudgetCategory(plan.id, {
        supportCategory: newSupportCategory,
        supportPurpose: newSupportPurpose,
        allocatedAmount: amount,
      });
      setAddCategoryOpen(false);
      setNewSupportCategory('');
      setNewSupportPurpose('');
      setNewAllocatedAmount('');
      refetch();
    } catch (err) {
      setCategoryError(err instanceof Error ? err.message : 'Failed to add budget category.');
    } finally {
      setCategorySubmitting(false);
    }
  };

  return (
    <div className="space-y-6">
      <div>
        <Button variant="ghost" size="sm" className="-ml-2 mb-2 text-muted-foreground" asChild>
          <Link to={`/participants/${plan.participantId}`}>
            <ArrowLeft className="mr-1 h-4 w-4" />
            {plan.participantName}
          </Link>
        </Button>

        <div className="flex items-center justify-between">
          <div className="flex items-center gap-3">
            <div className="flex h-10 w-10 items-center justify-center rounded-full bg-primary/10 text-primary">
              <FileText className="h-5 w-5" />
            </div>
            <div>
              <h1 className="text-2xl font-bold tracking-tight">{plan.planNumber}</h1>
              <p className="text-sm text-muted-foreground">{plan.participantName}</p>
            </div>
            <Badge variant="secondary" className={status.className}>
              {status.label}
            </Badge>
          </div>
          <div className="flex gap-2">
            {isDraft && (
              <>
                <Button variant="outline" asChild>
                  <Link to={`/plans/${plan.id}/edit`}>
                    <Pencil className="mr-2 h-4 w-4" />
                    Edit
                  </Link>
                </Button>
                <Button onClick={handleActivate} disabled={activating}>
                  <Play className="mr-2 h-4 w-4" />
                  {activating ? 'Activating...' : 'Activate Plan'}
                </Button>
              </>
            )}
          </div>
        </div>
      </div>

      <div className="grid gap-6 md:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2 text-base">
              <Calendar className="h-4 w-4 text-muted-foreground" />
              Plan Details
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <DetailRow icon={Hash} label="Plan Number" value={plan.planNumber} mono />
            <DetailRow
              icon={Calendar}
              label="Start Date"
              value={new Date(plan.startDate).toLocaleDateString('en-AU')}
            />
            <DetailRow
              icon={Calendar}
              label="End Date"
              value={new Date(plan.endDate).toLocaleDateString('en-AU')}
            />
            <DetailRow
              icon={DollarSign}
              label="Total Budget"
              value={`$${totalBudget.toLocaleString('en-AU', { minimumFractionDigits: 2 })}`}
            />
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <div className="flex items-center justify-between">
              <CardTitle className="flex items-center gap-2 text-base">
                <Layers className="h-4 w-4 text-muted-foreground" />
                Budget Categories
              </CardTitle>
              {isDraft && (
                <Button variant="outline" size="sm" onClick={() => setAddCategoryOpen(true)}>
                  <Plus className="mr-1 h-3 w-3" />
                  Add Category
                </Button>
              )}
            </div>
          </CardHeader>
          <CardContent>
            {plan.budgetCategories.length === 0 ? (
              <div className="flex flex-col items-center py-6 text-center">
                <div className="rounded-full bg-muted p-2.5">
                  <Layers className="h-4 w-4 text-muted-foreground" />
                </div>
                <p className="mt-2 text-sm text-muted-foreground">No budget categories yet</p>
                {isDraft && (
                  <Button
                    variant="link"
                    size="sm"
                    className="mt-1"
                    onClick={() => setAddCategoryOpen(true)}
                  >
                    Add a budget category
                  </Button>
                )}
              </div>
            ) : (
              <div className="space-y-3">
                {plan.budgetCategories.map((bc) => {
                  const proportion = totalBudget > 0 ? (bc.allocatedAmount / totalBudget) * 100 : 0;
                  const catKey = bc.supportCategory as SupportCategory;
                  return (
                    <div key={bc.id} className="space-y-1.5">
                      <div className="flex items-center justify-between text-sm">
                        <div className="flex items-center gap-2">
                          <div
                            className={`h-2.5 w-2.5 rounded-full ${categoryColors[catKey] || 'bg-gray-400'}`}
                          />
                          <span className="font-medium">
                            {categoryLabels[catKey] || bc.supportCategory}
                          </span>
                          <span className="text-muted-foreground">
                            {supportPurposes.find((sp) => sp.value === bc.supportPurpose)?.label ||
                              bc.supportPurpose}
                          </span>
                        </div>
                        <span className="font-medium">
                          ${bc.allocatedAmount.toLocaleString('en-AU', { minimumFractionDigits: 2 })}
                        </span>
                      </div>
                      <div className="h-2 w-full rounded-full bg-muted">
                        <div
                          className={`h-full rounded-full ${categoryColors[catKey] || 'bg-gray-400'} transition-all`}
                          style={{ width: `${proportion}%` }}
                        />
                      </div>
                    </div>
                  );
                })}
              </div>
            )}
          </CardContent>
        </Card>
      </div>

      {/* Budget Overview — shown for active/expiring plans with budget categories */}
      {!isDraft && plan.budgetCategories.length > 0 && (
        <BudgetOverviewCard planId={plan.id} />
      )}

      {/* Add Budget Category Dialog */}
      <Dialog open={addCategoryOpen} onOpenChange={setAddCategoryOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Add Budget Category</DialogTitle>
            <DialogDescription>
              Add a new budget allocation to this plan.
            </DialogDescription>
          </DialogHeader>
          {categoryError && <ErrorBanner message={categoryError} />}
          <form onSubmit={handleAddBudgetCategory} className="space-y-4">
            <div className="space-y-2">
              <Label>Support Category</Label>
              <Select
                value={newSupportCategory}
                onValueChange={(v) => setNewSupportCategory(v as SupportCategory)}
              >
                <SelectTrigger>
                  <SelectValue placeholder="Select category" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="Core">Core</SelectItem>
                  <SelectItem value="CapacityBuilding">Capacity Building</SelectItem>
                  <SelectItem value="Capital">Capital</SelectItem>
                </SelectContent>
              </Select>
            </div>

            <div className="space-y-2">
              <Label>Support Purpose</Label>
              <Select value={newSupportPurpose} onValueChange={setNewSupportPurpose}>
                <SelectTrigger>
                  <SelectValue placeholder="Select purpose" />
                </SelectTrigger>
                <SelectContent>
                  {supportPurposes.map((sp) => (
                    <SelectItem key={sp.value} value={sp.value}>
                      {sp.label}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div className="space-y-2">
              <Label htmlFor="allocatedAmount">Allocated Amount ($)</Label>
              <Input
                id="allocatedAmount"
                type="number"
                step="0.01"
                min="0.01"
                value={newAllocatedAmount}
                onChange={(e) => setNewAllocatedAmount(e.target.value)}
                placeholder="e.g. 15000.00"
              />
            </div>

            <DialogFooter>
              <Button
                type="button"
                variant="outline"
                onClick={() => setAddCategoryOpen(false)}
              >
                Cancel
              </Button>
              <Button type="submit" disabled={categorySubmitting}>
                {categorySubmitting ? 'Adding...' : 'Add Category'}
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
      <span className="min-w-[100px] text-muted-foreground">{label}</span>
      <span className={`${mono ? 'font-mono' : ''} ${value ? '' : 'text-muted-foreground'}`}>
        {value || '--'}
      </span>
    </div>
  );
}
