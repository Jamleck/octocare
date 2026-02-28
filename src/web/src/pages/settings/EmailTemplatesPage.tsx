import { useState } from 'react';
import { Mail, Eye, Pencil } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from '@/components/ui/dialog';
import {
  useEmailTemplates,
  updateEmailTemplate,
  previewEmailTemplate,
} from '@/hooks/useEmailTemplates';
import type { EmailTemplate, EmailTemplatePreview } from '@/types/api';
import { toast } from 'sonner';

const sampleVariables: Record<string, Record<string, string>> = {
  invoice_submitted: {
    invoice_number: 'INV-2025-042',
    provider_name: 'Allied Health Plus',
    participant_name: 'Sarah Johnson',
    amount: '$1,234.56',
  },
  plan_expiring: {
    plan_number: 'NDIS-2025-001',
    participant_name: 'Sarah Johnson',
    expiry_date: '30 Jun 2026',
    days_remaining: '30',
  },
  budget_alert: {
    plan_number: 'NDIS-2025-001',
    category_name: 'Core - Daily Activities',
    utilisation: '87.5',
    alert_message: 'Budget category is approaching the 90% threshold.',
  },
  statement_ready: {
    participant_name: 'Sarah Johnson',
    period_start: '1 Oct 2025',
    period_end: '31 Oct 2025',
  },
};

export function EmailTemplatesPage() {
  const { templates, isLoading, error, refetch } = useEmailTemplates();
  const [editingTemplate, setEditingTemplate] = useState<EmailTemplate | null>(null);
  const [editSubject, setEditSubject] = useState('');
  const [editBody, setEditBody] = useState('');
  const [isSaving, setIsSaving] = useState(false);

  const [previewData, setPreviewData] = useState<EmailTemplatePreview | null>(null);
  const [previewOpen, setPreviewOpen] = useState(false);
  const [isPreviewing, setIsPreviewing] = useState(false);

  const handleEdit = (template: EmailTemplate) => {
    setEditingTemplate(template);
    setEditSubject(template.subject);
    setEditBody(template.body);
  };

  const handleSave = async () => {
    if (!editingTemplate) return;
    setIsSaving(true);
    try {
      await updateEmailTemplate(editingTemplate.id, {
        subject: editSubject,
        body: editBody,
      });
      toast.success('Email template updated successfully.');
      setEditingTemplate(null);
      refetch();
    } catch (err) {
      toast.error('Failed to update email template.');
    } finally {
      setIsSaving(false);
    }
  };

  const handlePreview = async (template: EmailTemplate) => {
    setIsPreviewing(true);
    try {
      const variables = sampleVariables[template.name] ?? {};
      const preview = await previewEmailTemplate(template.id, variables);
      setPreviewData(preview);
      setPreviewOpen(true);
    } catch (err) {
      toast.error('Failed to generate preview.');
    } finally {
      setIsPreviewing(false);
    }
  };

  const formatTemplateName = (name: string): string => {
    return name
      .split('_')
      .map((w) => w.charAt(0).toUpperCase() + w.slice(1))
      .join(' ');
  };

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold tracking-tight">Email Templates</h1>
        <p className="text-muted-foreground">
          Manage the email templates used for notifications and communications.
        </p>
      </div>

      {isLoading ? (
        <Card>
          <CardContent className="flex items-center justify-center py-12">
            <p className="text-muted-foreground">Loading templates...</p>
          </CardContent>
        </Card>
      ) : error ? (
        <Card>
          <CardContent className="flex items-center justify-center py-12">
            <p className="text-destructive">Failed to load email templates.</p>
          </CardContent>
        </Card>
      ) : templates.length === 0 ? (
        <Card>
          <CardContent className="flex flex-col items-center justify-center py-12">
            <Mail className="mb-4 h-12 w-12 text-muted-foreground/30" />
            <p className="text-lg font-medium text-muted-foreground">No email templates</p>
            <p className="text-sm text-muted-foreground/70">
              Default templates will be created when the application seeds data.
            </p>
          </CardContent>
        </Card>
      ) : (
        <div className="space-y-4">
          {templates.map((template) => (
            <Card key={template.id}>
              <CardHeader className="pb-3">
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-3">
                    <CardTitle className="text-base">
                      {formatTemplateName(template.name)}
                    </CardTitle>
                    <Badge
                      variant={template.isActive ? 'default' : 'secondary'}
                      className="text-[10px]"
                    >
                      {template.isActive ? 'Active' : 'Inactive'}
                    </Badge>
                  </div>
                  <div className="flex items-center gap-2">
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => handlePreview(template)}
                      disabled={isPreviewing}
                    >
                      <Eye className="mr-1.5 h-3.5 w-3.5" />
                      Preview
                    </Button>
                    <Button variant="outline" size="sm" onClick={() => handleEdit(template)}>
                      <Pencil className="mr-1.5 h-3.5 w-3.5" />
                      Edit
                    </Button>
                  </div>
                </div>
              </CardHeader>
              <CardContent>
                <div className="space-y-2 text-sm">
                  <div>
                    <span className="font-medium text-muted-foreground">Template name:</span>{' '}
                    <code className="rounded bg-muted px-1.5 py-0.5 text-xs">{template.name}</code>
                  </div>
                  <div>
                    <span className="font-medium text-muted-foreground">Subject:</span>{' '}
                    {template.subject}
                  </div>
                  <div>
                    <span className="font-medium text-muted-foreground">Last updated:</span>{' '}
                    {new Date(template.updatedAt).toLocaleString()}
                  </div>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      )}

      {/* Edit Dialog */}
      <Dialog open={editingTemplate !== null} onOpenChange={(open) => { if (!open) setEditingTemplate(null); }}>
        <DialogContent className="max-w-2xl">
          <DialogHeader>
            <DialogTitle>
              Edit Template: {editingTemplate ? formatTemplateName(editingTemplate.name) : ''}
            </DialogTitle>
          </DialogHeader>
          <div className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="edit-subject">Subject</Label>
              <Input
                id="edit-subject"
                value={editSubject}
                onChange={(e) => setEditSubject(e.target.value)}
                placeholder="Email subject with {{variables}}"
              />
              <p className="text-xs text-muted-foreground">
                Use {'{{variable_name}}'} for placeholder variables.
              </p>
            </div>
            <div className="space-y-2">
              <Label htmlFor="edit-body">Body (HTML)</Label>
              <textarea
                id="edit-body"
                value={editBody}
                onChange={(e) => setEditBody(e.target.value)}
                className="flex min-h-[200px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2"
                placeholder="HTML body with {{variables}}"
              />
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setEditingTemplate(null)}>
              Cancel
            </Button>
            <Button onClick={handleSave} disabled={isSaving}>
              {isSaving ? 'Saving...' : 'Save Changes'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Preview Dialog */}
      <Dialog open={previewOpen} onOpenChange={setPreviewOpen}>
        <DialogContent className="max-w-2xl">
          <DialogHeader>
            <DialogTitle>Email Preview</DialogTitle>
          </DialogHeader>
          {previewData && (
            <div className="space-y-4">
              <div>
                <Label className="text-muted-foreground">Subject</Label>
                <p className="mt-1 font-medium">{previewData.subject}</p>
              </div>
              <div>
                <Label className="text-muted-foreground">Body</Label>
                <div
                  className="mt-1 rounded-md border bg-white p-4 text-sm"
                  dangerouslySetInnerHTML={{ __html: previewData.body }}
                />
              </div>
            </div>
          )}
          <DialogFooter>
            <Button variant="outline" onClick={() => setPreviewOpen(false)}>
              Close
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
