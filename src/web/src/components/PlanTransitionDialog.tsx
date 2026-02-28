import { useState } from 'react';
import { Button } from '@/components/ui/button';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Checkbox } from '@/components/ui/checkbox';
import { Label } from '@/components/ui/label';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import { ErrorBanner } from '@/components/ErrorBanner';
import {
  usePlanTransitions,
  initiateTransition,
  updateTransition,
  completeTransition,
} from '@/hooks/usePlanTransitions';
import { ArrowRightLeft, Check, ClipboardList, Loader2 } from 'lucide-react';
import type { PlanTransition, TransitionChecklistItem } from '@/types/api';

interface PlanTransitionDialogProps {
  planId: string;
  planNumber: string;
  planStatus: string;
}

const statusBadge: Record<string, { label: string; className: string }> = {
  Pending: {
    label: 'Pending',
    className: 'border-yellow-200 bg-yellow-50 text-yellow-700',
  },
  InProgress: {
    label: 'In Progress',
    className: 'border-blue-200 bg-blue-50 text-blue-700',
  },
  Completed: {
    label: 'Completed',
    className: 'border-emerald-200 bg-emerald-50 text-emerald-700',
  },
};

export function PlanTransitionDialog({
  planId,
  planNumber,
  planStatus,
}: PlanTransitionDialogProps) {
  const { transitions, isLoading, refetch } = usePlanTransitions(planId);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [initiating, setInitiating] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const canInitiate =
    planStatus === 'active' || planStatus === 'expiring';

  const activeTransition = transitions.find(
    (t) => t.status !== 'Completed',
  );

  const handleInitiate = async () => {
    try {
      setInitiating(true);
      setError(null);
      await initiateTransition({ oldPlanId: planId });
      refetch();
    } catch (err) {
      setError(
        err instanceof Error ? err.message : 'Failed to initiate transition.',
      );
    } finally {
      setInitiating(false);
    }
  };

  if (!canInitiate && transitions.length === 0) return null;

  return (
    <>
      <Button
        variant="outline"
        onClick={() => setDialogOpen(true)}
        disabled={isLoading}
      >
        <ArrowRightLeft className="mr-2 h-4 w-4" />
        Plan Transition
        {activeTransition && (
          <Badge variant="secondary" className="ml-2">
            {activeTransition.status === 'Pending'
              ? 'Pending'
              : 'In Progress'}
          </Badge>
        )}
      </Button>

      <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
        <DialogContent className="max-w-lg">
          <DialogHeader>
            <DialogTitle className="flex items-center gap-2">
              <ArrowRightLeft className="h-5 w-5" />
              Plan Transition
            </DialogTitle>
            <DialogDescription>
              Manage the transition from plan {planNumber} to a new plan.
            </DialogDescription>
          </DialogHeader>

          {error && <ErrorBanner message={error} />}

          {isLoading ? (
            <div className="flex items-center justify-center py-8">
              <Loader2 className="h-6 w-6 animate-spin text-muted-foreground" />
            </div>
          ) : activeTransition ? (
            <TransitionChecklist
              transition={activeTransition}
              onUpdate={refetch}
              onError={setError}
            />
          ) : transitions.length > 0 ? (
            <div className="space-y-4">
              <p className="text-sm text-muted-foreground">
                Previous transitions have been completed for this plan.
              </p>
              {transitions.map((t) => (
                <div
                  key={t.id}
                  className="flex items-center justify-between rounded-lg border p-3"
                >
                  <div className="text-sm">
                    <p className="font-medium">
                      {t.oldPlanNumber}{' '}
                      {t.newPlanNumber && (
                        <>
                          <span className="text-muted-foreground">to</span>{' '}
                          {t.newPlanNumber}
                        </>
                      )}
                    </p>
                    <p className="text-xs text-muted-foreground">
                      {t.completedAt
                        ? `Completed ${new Date(t.completedAt).toLocaleDateString('en-AU')}`
                        : `Created ${new Date(t.createdAt).toLocaleDateString('en-AU')}`}
                    </p>
                  </div>
                  <Badge
                    variant="secondary"
                    className={
                      statusBadge[t.status]?.className ?? ''
                    }
                  >
                    {statusBadge[t.status]?.label ?? t.status}
                  </Badge>
                </div>
              ))}
              {canInitiate && (
                <Button onClick={handleInitiate} disabled={initiating}>
                  {initiating ? 'Initiating...' : 'Start New Transition'}
                </Button>
              )}
            </div>
          ) : (
            <div className="space-y-4">
              <p className="text-sm text-muted-foreground">
                No transitions have been started for this plan. Initiating a
                transition creates a checklist to help manage the process of
                moving to a new plan.
              </p>
              <DialogFooter>
                <Button
                  type="button"
                  variant="outline"
                  onClick={() => setDialogOpen(false)}
                >
                  Cancel
                </Button>
                <Button onClick={handleInitiate} disabled={initiating}>
                  <ClipboardList className="mr-2 h-4 w-4" />
                  {initiating ? 'Initiating...' : 'Initiate Transition'}
                </Button>
              </DialogFooter>
            </div>
          )}
        </DialogContent>
      </Dialog>
    </>
  );
}

function TransitionChecklist({
  transition,
  onUpdate,
  onError,
}: {
  transition: PlanTransition;
  onUpdate: () => void;
  onError: (msg: string | null) => void;
}) {
  const [items, setItems] = useState<TransitionChecklistItem[]>(
    transition.checklistItems,
  );
  const [notes, setNotes] = useState(transition.notes ?? '');
  const [saving, setSaving] = useState(false);
  const [completing, setCompleting] = useState(false);

  const isCompleted = transition.status === 'Completed';
  const allChecked = items.every((item) => item.completed);

  const handleToggle = (index: number) => {
    if (isCompleted) return;
    const updated = [...items];
    updated[index] = { ...updated[index], completed: !updated[index].completed };
    setItems(updated);
  };

  const handleSave = async () => {
    try {
      setSaving(true);
      onError(null);
      await updateTransition(transition.id, { checklistItems: items, notes });
      onUpdate();
    } catch (err) {
      onError(
        err instanceof Error ? err.message : 'Failed to save checklist.',
      );
    } finally {
      setSaving(false);
    }
  };

  const handleComplete = async () => {
    try {
      setCompleting(true);
      onError(null);
      // Save checklist first, then complete
      await updateTransition(transition.id, { checklistItems: items, notes });
      await completeTransition(transition.id);
      onUpdate();
    } catch (err) {
      onError(
        err instanceof Error ? err.message : 'Failed to complete transition.',
      );
    } finally {
      setCompleting(false);
    }
  };

  const completedCount = items.filter((item) => item.completed).length;

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <Badge
          variant="secondary"
          className={
            statusBadge[transition.status]?.className ?? ''
          }
        >
          {statusBadge[transition.status]?.label ?? transition.status}
        </Badge>
        <span className="text-xs text-muted-foreground">
          {completedCount}/{items.length} completed
        </span>
      </div>

      <div className="space-y-2">
        {items.map((item, index) => (
          <label
            key={index}
            className={`flex cursor-pointer items-center gap-3 rounded-lg border p-3 transition-colors hover:bg-muted/50 ${
              item.completed ? 'bg-muted/30' : ''
            } ${isCompleted ? 'cursor-default' : ''}`}
          >
            <Checkbox
              checked={item.completed}
              onCheckedChange={() => handleToggle(index)}
              disabled={isCompleted}
            />
            <span
              className={`text-sm ${
                item.completed ? 'text-muted-foreground line-through' : ''
              }`}
            >
              {item.label}
            </span>
          </label>
        ))}
      </div>

      <div className="space-y-2">
        <Label htmlFor="transition-notes">Notes</Label>
        <Input
          id="transition-notes"
          value={notes}
          onChange={(e) => setNotes(e.target.value)}
          placeholder="Optional notes about this transition..."
          disabled={isCompleted}
        />
      </div>

      {!isCompleted && (
        <DialogFooter className="gap-2 sm:gap-0">
          <Button
            variant="outline"
            onClick={handleSave}
            disabled={saving || completing}
          >
            {saving ? 'Saving...' : 'Save Progress'}
          </Button>
          <Button
            onClick={handleComplete}
            disabled={!allChecked || saving || completing}
            title={
              allChecked
                ? 'Complete transition'
                : 'Complete all checklist items first'
            }
          >
            <Check className="mr-2 h-4 w-4" />
            {completing ? 'Completing...' : 'Complete Transition'}
          </Button>
        </DialogFooter>
      )}
    </div>
  );
}
