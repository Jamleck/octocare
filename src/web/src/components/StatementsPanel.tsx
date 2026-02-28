import { useState } from 'react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import { Skeleton } from '@/components/ui/skeleton';
import { Badge } from '@/components/ui/badge';
import {
  useStatements,
  generateStatement,
  sendStatement,
  getStatementPdfUrl,
} from '@/hooks/useStatements';
import {
  FileText,
  Plus,
  Download,
  Mail,
  Loader2,
  CheckCircle2,
} from 'lucide-react';
import type { Plan } from '@/types/api';

interface StatementsPanelProps {
  participantId: string;
  plans: Plan[];
}

export function StatementsPanel({ participantId, plans }: StatementsPanelProps) {
  const { statements, isLoading, refetch } = useStatements(participantId);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [selectedPlanId, setSelectedPlanId] = useState('');
  const [periodStart, setPeriodStart] = useState('');
  const [periodEnd, setPeriodEnd] = useState('');
  const [generating, setGenerating] = useState(false);
  const [sendingId, setSendingId] = useState<string | null>(null);

  const handleGenerate = async () => {
    if (!selectedPlanId || !periodStart || !periodEnd) return;

    try {
      setGenerating(true);
      await generateStatement(participantId, {
        planId: selectedPlanId,
        periodStart,
        periodEnd,
      });
      setDialogOpen(false);
      setSelectedPlanId('');
      setPeriodStart('');
      setPeriodEnd('');
      refetch();
    } catch {
      // Error handling could be enhanced
    } finally {
      setGenerating(false);
    }
  };

  const handleSend = async (statementId: string) => {
    try {
      setSendingId(statementId);
      await sendStatement(statementId);
      refetch();
    } catch {
      // Error handling could be enhanced
    } finally {
      setSendingId(null);
    }
  };

  const handleDownload = (statementId: string) => {
    window.open(getStatementPdfUrl(statementId), '_blank');
  };

  return (
    <Card>
      <CardHeader>
        <div className="flex items-center justify-between">
          <CardTitle className="flex items-center gap-2 text-base">
            <FileText className="h-4 w-4 text-muted-foreground" />
            Statements
          </CardTitle>
          <Button variant="outline" size="sm" onClick={() => setDialogOpen(true)}>
            <Plus className="mr-1 h-3 w-3" />
            Generate Statement
          </Button>
        </div>
      </CardHeader>
      <CardContent>
        {isLoading ? (
          <div className="space-y-2">
            <Skeleton className="h-8 w-full" />
            <Skeleton className="h-8 w-full" />
          </div>
        ) : statements.length === 0 ? (
          <div className="flex flex-col items-center py-6 text-center">
            <div className="rounded-full bg-muted p-2.5">
              <FileText className="h-4 w-4 text-muted-foreground" />
            </div>
            <p className="mt-2 text-sm text-muted-foreground">No statements yet</p>
            <Button variant="link" size="sm" className="mt-1" onClick={() => setDialogOpen(true)}>
              Generate a statement
            </Button>
          </div>
        ) : (
          <div className="rounded-lg border">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Period</TableHead>
                  <TableHead>Generated</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead className="text-right">Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {statements.map((stmt) => (
                  <TableRow key={stmt.id}>
                    <TableCell className="text-sm">
                      {new Date(stmt.periodStart).toLocaleDateString('en-AU')} &ndash;{' '}
                      {new Date(stmt.periodEnd).toLocaleDateString('en-AU')}
                    </TableCell>
                    <TableCell className="text-sm">
                      {new Date(stmt.generatedAt).toLocaleDateString('en-AU')}
                    </TableCell>
                    <TableCell>
                      {stmt.sentAt ? (
                        <Badge
                          variant="secondary"
                          className="border-emerald-200 bg-emerald-50 text-emerald-700"
                        >
                          <CheckCircle2 className="mr-1 h-3 w-3" />
                          Sent
                        </Badge>
                      ) : (
                        <Badge variant="secondary" className="text-muted-foreground">
                          Generated
                        </Badge>
                      )}
                    </TableCell>
                    <TableCell className="text-right">
                      <div className="flex justify-end gap-1">
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => handleDownload(stmt.id)}
                          title="Download PDF"
                        >
                          <Download className="h-3.5 w-3.5" />
                        </Button>
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => handleSend(stmt.id)}
                          disabled={sendingId === stmt.id}
                          title="Send via email"
                        >
                          {sendingId === stmt.id ? (
                            <Loader2 className="h-3.5 w-3.5 animate-spin" />
                          ) : (
                            <Mail className="h-3.5 w-3.5" />
                          )}
                        </Button>
                      </div>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </div>
        )}
      </CardContent>

      <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Generate Statement</DialogTitle>
            <DialogDescription>
              Select a plan and date range to generate a participant statement PDF.
            </DialogDescription>
          </DialogHeader>

          <div className="space-y-4 py-2">
            <div className="space-y-2">
              <Label htmlFor="plan">Plan</Label>
              <Select value={selectedPlanId} onValueChange={setSelectedPlanId}>
                <SelectTrigger id="plan">
                  <SelectValue placeholder="Select a plan" />
                </SelectTrigger>
                <SelectContent>
                  {plans.map((plan) => (
                    <SelectItem key={plan.id} value={plan.id}>
                      {plan.planNumber} ({plan.status})
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="periodStart">Period Start</Label>
                <Input
                  id="periodStart"
                  type="date"
                  value={periodStart}
                  onChange={(e) => setPeriodStart(e.target.value)}
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="periodEnd">Period End</Label>
                <Input
                  id="periodEnd"
                  type="date"
                  value={periodEnd}
                  onChange={(e) => setPeriodEnd(e.target.value)}
                />
              </div>
            </div>
          </div>

          <DialogFooter>
            <Button variant="outline" onClick={() => setDialogOpen(false)}>
              Cancel
            </Button>
            <Button
              onClick={handleGenerate}
              disabled={generating || !selectedPlanId || !periodStart || !periodEnd}
            >
              {generating ? (
                <>
                  <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                  Generating...
                </>
              ) : (
                'Generate'
              )}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </Card>
  );
}
