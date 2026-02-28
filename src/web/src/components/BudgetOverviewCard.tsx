import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Skeleton } from '@/components/ui/skeleton';
import { ErrorBanner } from '@/components/ErrorBanner';
import { useBudgetOverview } from '@/hooks/useBudgetOverview';
import { DollarSign, TrendingUp } from 'lucide-react';
import type { BudgetCategoryProjection } from '@/types/api';

const categoryColors: Record<string, { bar: string; bg: string }> = {
  Core: { bar: 'bg-blue-500', bg: 'bg-blue-100' },
  CapacityBuilding: { bar: 'bg-purple-500', bg: 'bg-purple-100' },
  Capital: { bar: 'bg-amber-500', bg: 'bg-amber-100' },
};

const categoryLabels: Record<string, string> = {
  Core: 'Core',
  CapacityBuilding: 'Capacity Building',
  Capital: 'Capital',
};

const purposeLabels: Record<string, string> = {
  DailyActivities: 'Daily Activities',
  TransportActivities: 'Transport',
  ConsumablesAndEquipment: 'Consumables & Equipment',
  AssistiveTechnology: 'Assistive Technology',
  HomeModifications: 'Home Modifications',
  CoordinationOfSupports: 'Coordination of Supports',
  ImprovedLivingArrangements: 'Improved Living',
  IncreasedSocialAndCommunityParticipation: 'Social & Community',
  FindingAndKeepingAJob: 'Finding & Keeping a Job',
  ImprovedRelationships: 'Improved Relationships',
  ImprovedHealthAndWellbeing: 'Health & Wellbeing',
  ImprovedLearning: 'Improved Learning',
  ImprovedLifeChoices: 'Life Choices',
  ImprovedDailyLivingSkills: 'Daily Living Skills',
};

function formatDollars(amount: number): string {
  return `$${amount.toLocaleString('en-AU', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
}

interface BudgetOverviewCardProps {
  planId: string;
}

export function BudgetOverviewCard({ planId }: BudgetOverviewCardProps) {
  const { overview, isLoading, error, refetch } = useBudgetOverview(planId);

  if (isLoading) {
    return (
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2 text-base">
            <TrendingUp className="h-4 w-4 text-muted-foreground" />
            Budget Overview
          </CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <Skeleton className="h-6 w-48" />
          <Skeleton className="h-4 w-full" />
          <Skeleton className="h-4 w-full" />
          <Skeleton className="h-4 w-full" />
        </CardContent>
      </Card>
    );
  }

  if (error) {
    return (
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2 text-base">
            <TrendingUp className="h-4 w-4 text-muted-foreground" />
            Budget Overview
          </CardTitle>
        </CardHeader>
        <CardContent>
          <ErrorBanner message={error.message} onRetry={refetch} />
        </CardContent>
      </Card>
    );
  }

  if (!overview) return null;

  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center gap-2 text-base">
          <TrendingUp className="h-4 w-4 text-muted-foreground" />
          Budget Overview
        </CardTitle>
      </CardHeader>
      <CardContent className="space-y-6">
        {/* Summary stats */}
        <div className="grid grid-cols-2 gap-4 sm:grid-cols-4">
          <SummaryItem label="Allocated" value={formatDollars(overview.totalAllocated)} />
          <SummaryItem label="Committed" value={formatDollars(overview.totalCommitted)} />
          <SummaryItem label="Spent" value={formatDollars(overview.totalSpent)} />
          <SummaryItem
            label="Available"
            value={formatDollars(overview.totalAvailable)}
            highlight={overview.totalAvailable < 0}
          />
        </div>

        {/* Overall utilisation */}
        <div className="space-y-1.5">
          <div className="flex items-center justify-between text-sm">
            <span className="text-muted-foreground">Overall Utilisation</span>
            <span className="font-medium">{overview.utilisationPercentage.toFixed(1)}%</span>
          </div>
          <div className="h-2.5 w-full rounded-full bg-muted">
            <div
              className="h-full rounded-full bg-primary transition-all"
              style={{ width: `${Math.min(overview.utilisationPercentage, 100)}%` }}
            />
          </div>
        </div>

        {/* Per-category breakdown */}
        <div className="space-y-4">
          {overview.categories.map((cat) => (
            <CategoryRow key={cat.categoryId} category={cat} />
          ))}
        </div>
      </CardContent>
    </Card>
  );
}

function SummaryItem({
  label,
  value,
  highlight,
}: {
  label: string;
  value: string;
  highlight?: boolean;
}) {
  return (
    <div className="space-y-0.5">
      <p className="text-xs text-muted-foreground">{label}</p>
      <p className={`text-sm font-semibold ${highlight ? 'text-destructive' : ''}`}>
        {value}
      </p>
    </div>
  );
}

function CategoryRow({ category }: { category: BudgetCategoryProjection }) {
  const colors = categoryColors[category.supportCategory] || { bar: 'bg-gray-500', bg: 'bg-gray-100' };
  const catLabel = categoryLabels[category.supportCategory] || category.supportCategory;
  const purposeLabel = purposeLabels[category.supportPurpose] || category.supportPurpose;

  // Calculate proportional widths for the stacked bar
  const allocated = category.allocated;
  const spentPct = allocated > 0 ? (category.spent / allocated) * 100 : 0;
  const committedPct = allocated > 0 ? (category.committed / allocated) * 100 : 0;
  const pendingPct = allocated > 0 ? (category.pending / allocated) * 100 : 0;

  return (
    <div className="space-y-2 rounded-lg border p-3">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-2">
          <div className={`h-2.5 w-2.5 rounded-full ${colors.bar}`} />
          <span className="text-sm font-medium">{catLabel}</span>
          <span className="text-xs text-muted-foreground">{purposeLabel}</span>
        </div>
        <span className="text-sm font-medium">{category.utilisationPercentage.toFixed(1)}%</span>
      </div>

      {/* Stacked progress bar */}
      <div className="h-2 w-full rounded-full bg-muted">
        <div className="flex h-full overflow-hidden rounded-full">
          {spentPct > 0 && (
            <div
              className={`${colors.bar} h-full`}
              style={{ width: `${Math.min(spentPct, 100)}%` }}
              title={`Spent: ${formatDollars(category.spent)}`}
            />
          )}
          {committedPct > 0 && (
            <div
              className={`${colors.bar} h-full opacity-50`}
              style={{ width: `${Math.min(committedPct, 100 - spentPct)}%` }}
              title={`Committed: ${formatDollars(category.committed)}`}
            />
          )}
          {pendingPct > 0 && (
            <div
              className="h-full bg-yellow-400 opacity-60"
              style={{ width: `${Math.min(pendingPct, 100 - spentPct - committedPct)}%` }}
              title={`Pending: ${formatDollars(category.pending)}`}
            />
          )}
        </div>
      </div>

      {/* Amounts row */}
      <div className="flex flex-wrap gap-x-4 gap-y-1 text-xs text-muted-foreground">
        <span>
          <DollarSign className="mr-0.5 inline h-3 w-3" />
          Allocated: {formatDollars(category.allocated)}
        </span>
        <span>Spent: {formatDollars(category.spent)}</span>
        <span>Committed: {formatDollars(category.committed)}</span>
        {category.pending > 0 && <span>Pending: {formatDollars(category.pending)}</span>}
        <span className={category.available < 0 ? 'font-medium text-destructive' : ''}>
          Available: {formatDollars(category.available)}
        </span>
      </div>
    </div>
  );
}
