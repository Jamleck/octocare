import { useState } from 'react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import { Skeleton } from '@/components/ui/skeleton';
import { ErrorBanner } from '@/components/ErrorBanner';
import {
  BarChart3,
  Download,
  Eye,
  FileSpreadsheet,
  FileText,
  ClipboardList,
  Users,
  Shield,
  DollarSign,
} from 'lucide-react';
import {
  useBudgetUtilisationReport,
  useOutstandingInvoicesReport,
  useClaimStatusReport,
  useParticipantSummaryReport,
  useAuditTrailReport,
  downloadReport,
} from '@/hooks/useReports';
import type { ReportName } from '@/hooks/useReports';

interface ReportConfig {
  name: ReportName;
  title: string;
  description: string;
  icon: React.ElementType;
  columns: { key: string; label: string; align?: 'right' }[];
}

const reportConfigs: ReportConfig[] = [
  {
    name: 'budget-utilisation',
    title: 'Budget Utilisation',
    description:
      'Per participant, per plan budget utilisation showing allocated, spent, available amounts and utilisation percentage.',
    icon: DollarSign,
    columns: [
      { key: 'participantName', label: 'Participant' },
      { key: 'ndisNumber', label: 'NDIS Number' },
      { key: 'planNumber', label: 'Plan' },
      { key: 'category', label: 'Category' },
      { key: 'purpose', label: 'Purpose' },
      { key: 'allocated', label: 'Allocated', align: 'right' },
      { key: 'spent', label: 'Spent', align: 'right' },
      { key: 'available', label: 'Available', align: 'right' },
      { key: 'utilisationPercent', label: 'Utilisation %', align: 'right' },
    ],
  },
  {
    name: 'outstanding-invoices',
    title: 'Outstanding Invoices',
    description:
      'All invoices not yet paid, grouped by age (0-30, 31-60, 61-90, 90+ days) with provider and participant info.',
    icon: FileText,
    columns: [
      { key: 'invoiceNumber', label: 'Invoice #' },
      { key: 'providerName', label: 'Provider' },
      { key: 'participantName', label: 'Participant' },
      { key: 'servicePeriodEnd', label: 'Service End' },
      { key: 'amount', label: 'Amount', align: 'right' },
      { key: 'status', label: 'Status' },
      { key: 'daysOutstanding', label: 'Days', align: 'right' },
      { key: 'ageBucket', label: 'Age Bucket' },
    ],
  },
  {
    name: 'claim-status',
    title: 'Claim Status',
    description:
      'All claims with batch numbers, status, amounts, line item counts, and acceptance/rejection breakdown.',
    icon: ClipboardList,
    columns: [
      { key: 'batchNumber', label: 'Batch #' },
      { key: 'status', label: 'Status' },
      { key: 'totalAmount', label: 'Total Amount', align: 'right' },
      { key: 'lineItemCount', label: 'Line Items', align: 'right' },
      { key: 'acceptedCount', label: 'Accepted', align: 'right' },
      { key: 'rejectedCount', label: 'Rejected', align: 'right' },
      { key: 'submissionDate', label: 'Submitted' },
    ],
  },
  {
    name: 'participant-summary',
    title: 'Participant Summary',
    description:
      'All participants with active plan information, total budget, total spent, and utilisation percentage.',
    icon: Users,
    columns: [
      { key: 'name', label: 'Name' },
      { key: 'ndisNumber', label: 'NDIS Number' },
      { key: 'isActive', label: 'Active' },
      { key: 'activePlanNumber', label: 'Active Plan' },
      { key: 'planEnd', label: 'Plan End' },
      { key: 'totalAllocated', label: 'Allocated', align: 'right' },
      { key: 'totalSpent', label: 'Spent', align: 'right' },
      { key: 'utilisationPercent', label: 'Utilisation %', align: 'right' },
    ],
  },
  {
    name: 'audit-trail',
    title: 'Audit Trail',
    description:
      'Event store audit trail filtered by date range, showing timestamps, stream types, event types, and details.',
    icon: Shield,
    columns: [
      { key: 'timestamp', label: 'Timestamp' },
      { key: 'streamType', label: 'Stream Type' },
      { key: 'eventType', label: 'Event Type' },
      { key: 'streamId', label: 'Stream ID' },
      { key: 'details', label: 'Details' },
    ],
  },
];

function formatCellValue(value: unknown, key: string): string {
  if (value === null || value === undefined) return '--';
  if (typeof value === 'boolean') return value ? 'Yes' : 'No';
  if (typeof value === 'number') {
    if (key.toLowerCase().includes('percent')) return `${value.toFixed(1)}%`;
    if (
      key.toLowerCase().includes('amount') ||
      key.toLowerCase().includes('allocated') ||
      key.toLowerCase().includes('spent') ||
      key.toLowerCase().includes('available')
    ) {
      return `$${value.toLocaleString('en-AU', { minimumFractionDigits: 2 })}`;
    }
    return value.toString();
  }
  if (typeof value === 'string') {
    // Try to format dates
    if (/^\d{4}-\d{2}-\d{2}/.test(value)) {
      try {
        return new Date(value).toLocaleDateString('en-AU');
      } catch {
        return value;
      }
    }
    return value;
  }
  return String(value);
}

export function ReportsPage() {
  const [activeReport, setActiveReport] = useState<ReportName | null>(null);
  const [downloading, setDownloading] = useState<string | null>(null);

  const budgetReport = useBudgetUtilisationReport();
  const invoicesReport = useOutstandingInvoicesReport();
  const claimReport = useClaimStatusReport();
  const participantReport = useParticipantSummaryReport();
  const auditReport = useAuditTrailReport();

  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  type ReportHook = {
    data: any[] | null;
    isLoading: boolean;
    error: Error | null;
    fetchReport: (params?: Record<string, string>) => Promise<void>;
  };

  const reportHooks: Record<ReportName, ReportHook> = {
    'budget-utilisation': budgetReport,
    'outstanding-invoices': invoicesReport,
    'claim-status': claimReport,
    'participant-summary': participantReport,
    'audit-trail': auditReport,
  };

  const handleView = async (reportName: ReportName) => {
    setActiveReport(reportName);
    const hook = reportHooks[reportName];
    if (!hook.data) {
      await hook.fetchReport();
    }
  };

  const handleDownload = async (reportName: ReportName, format: 'csv' | 'xlsx') => {
    const key = `${reportName}-${format}`;
    try {
      setDownloading(key);
      await downloadReport(reportName, format);
    } catch (err) {
      console.error('Download failed:', err);
    } finally {
      setDownloading(null);
    }
  };

  const activeConfig = reportConfigs.find((r) => r.name === activeReport);
  const activeHook = activeReport ? reportHooks[activeReport] : null;

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold tracking-tight">Reports</h1>
        <p className="text-sm text-muted-foreground">
          Generate and export reports across participants, budgets, invoices, claims, and audit
          trails.
        </p>
      </div>

      {/* Report cards */}
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
        {reportConfigs.map((config) => {
          const Icon = config.icon;
          const isActive = activeReport === config.name;
          return (
            <Card
              key={config.name}
              className={isActive ? 'ring-2 ring-primary' : ''}
            >
              <CardHeader>
                <div className="flex items-center gap-3">
                  <div className="rounded-lg bg-primary/10 p-2">
                    <Icon className="h-5 w-5 text-primary" />
                  </div>
                  <div>
                    <CardTitle className="text-base">{config.title}</CardTitle>
                  </div>
                </div>
                <CardDescription>{config.description}</CardDescription>
              </CardHeader>
              <CardContent>
                <div className="flex flex-wrap gap-2">
                  <Button
                    variant={isActive ? 'default' : 'outline'}
                    size="sm"
                    onClick={() => handleView(config.name)}
                    disabled={reportHooks[config.name].isLoading}
                  >
                    <Eye className="mr-1.5 h-3.5 w-3.5" />
                    {reportHooks[config.name].isLoading ? 'Loading...' : 'View'}
                  </Button>
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => handleDownload(config.name, 'csv')}
                    disabled={downloading === `${config.name}-csv`}
                  >
                    <FileText className="mr-1.5 h-3.5 w-3.5" />
                    {downloading === `${config.name}-csv` ? 'Downloading...' : 'CSV'}
                  </Button>
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => handleDownload(config.name, 'xlsx')}
                    disabled={downloading === `${config.name}-xlsx`}
                  >
                    <FileSpreadsheet className="mr-1.5 h-3.5 w-3.5" />
                    {downloading === `${config.name}-xlsx` ? 'Downloading...' : 'Excel'}
                  </Button>
                </div>
              </CardContent>
            </Card>
          );
        })}
      </div>

      {/* Report data table */}
      {activeReport && activeConfig && activeHook && (
        <div className="space-y-4">
          <div className="flex items-center justify-between">
            <h2 className="text-lg font-semibold">{activeConfig.title} Report</h2>
            <div className="flex gap-2">
              <Button
                variant="outline"
                size="sm"
                onClick={() => handleDownload(activeReport, 'csv')}
              >
                <Download className="mr-1.5 h-3.5 w-3.5" />
                Export CSV
              </Button>
              <Button
                variant="outline"
                size="sm"
                onClick={() => handleDownload(activeReport, 'xlsx')}
              >
                <Download className="mr-1.5 h-3.5 w-3.5" />
                Export Excel
              </Button>
            </div>
          </div>

          {activeHook.isLoading && (
            <div className="space-y-2">
              <Skeleton className="h-10 w-full" />
              <Skeleton className="h-64 w-full rounded-lg" />
            </div>
          )}

          {activeHook.error && (
            <ErrorBanner
              message={activeHook.error.message}
              onRetry={() => activeHook.fetchReport()}
            />
          )}

          {activeHook.data && !activeHook.isLoading && (
            <>
              {activeHook.data.length === 0 ? (
                <Card>
                  <CardContent className="flex flex-col items-center justify-center py-16">
                    <div className="rounded-full bg-primary/10 p-4">
                      <BarChart3 className="h-8 w-8 text-primary" />
                    </div>
                    <h3 className="mt-4 text-lg font-semibold">No data</h3>
                    <p className="mt-1 text-sm text-muted-foreground">
                      No records found for this report.
                    </p>
                  </CardContent>
                </Card>
              ) : (
                <div className="rounded-lg border">
                  <Table>
                    <TableHeader>
                      <TableRow>
                        {activeConfig.columns.map((col) => (
                          <TableHead
                            key={col.key}
                            className={col.align === 'right' ? 'text-right' : ''}
                          >
                            {col.label}
                          </TableHead>
                        ))}
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {activeHook.data.map((row, idx) => (
                        <TableRow key={idx}>
                          {activeConfig.columns.map((col) => (
                            <TableCell
                              key={col.key}
                              className={`${col.align === 'right' ? 'text-right' : ''} ${
                                col.key.includes('amount') ||
                                col.key.includes('Allocated') ||
                                col.key.includes('Spent')
                                  ? 'font-medium'
                                  : ''
                              }`}
                            >
                              {formatCellValue(
                                row[col.key as keyof typeof row],
                                col.key,
                              )}
                            </TableCell>
                          ))}
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </div>
              )}

              <p className="text-sm text-muted-foreground">
                {activeHook.data.length} row{activeHook.data.length !== 1 ? 's' : ''}
              </p>
            </>
          )}
        </div>
      )}
    </div>
  );
}
