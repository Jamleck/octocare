import { useState, useEffect } from 'react';
import { Link } from 'react-router';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { useOrganisation } from '@/hooks/useOrganisation';
import { Skeleton } from '@/components/ui/skeleton';
import type { UpdateOrganisationRequest } from '@/types/api';
import { ApiError } from '@/lib/api-error';
import { ErrorBanner } from '@/components/ErrorBanner';
import { Building2, Loader2, Check, Mail, ChevronRight } from 'lucide-react';

export function OrgSettingsPage() {
  const { organisation, isLoading, error, updateOrganisation, refetch } = useOrganisation();
  const [form, setForm] = useState<UpdateOrganisationRequest>({
    name: '',
    abn: '',
    contactEmail: '',
    contactPhone: '',
    address: '',
  });
  const [errors, setErrors] = useState<Record<string, string[]>>({});
  const [saving, setSaving] = useState(false);
  const [saved, setSaved] = useState(false);

  useEffect(() => {
    if (organisation) {
      setForm({
        name: organisation.name,
        abn: organisation.abn ?? '',
        contactEmail: organisation.contactEmail ?? '',
        contactPhone: organisation.contactPhone ?? '',
        address: organisation.address ?? '',
      });
    }
  }, [organisation]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setErrors({});
    setSaving(true);
    setSaved(false);
    try {
      await updateOrganisation(form);
      setSaved(true);
      setTimeout(() => setSaved(false), 3000);
    } catch (err) {
      if (err instanceof ApiError && err.validationErrors) {
        setErrors(err.validationErrors);
      }
    } finally {
      setSaving(false);
    }
  };

  if (isLoading) {
    return (
      <div className="space-y-4">
        <Skeleton className="h-8 w-48" />
        <Skeleton className="h-64 w-full rounded-lg" />
      </div>
    );
  }

  if (error) {
    return (
      <div className="space-y-6">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Organisation Settings</h1>
        </div>
        <ErrorBanner message={error.message} onRetry={refetch} />
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold tracking-tight">Organisation Settings</h1>
        <p className="text-sm text-muted-foreground">
          Manage your organisation's profile and contact information.
        </p>
      </div>

      <Card className="max-w-2xl">
        <CardHeader>
          <CardTitle className="flex items-center gap-2 text-base">
            <Building2 className="h-4 w-4 text-muted-foreground" />
            Organisation Details
          </CardTitle>
          <CardDescription>This information identifies your organisation in the system and on participant statements.</CardDescription>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit} className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="name">Organisation Name *</Label>
              <Input
                id="name"
                value={form.name}
                onChange={(e) => setForm({ ...form, name: e.target.value })}
                required
              />
              {errors.Name && <p className="text-sm text-destructive">{errors.Name[0]}</p>}
            </div>

            <div className="space-y-2">
              <Label htmlFor="abn">ABN</Label>
              <Input
                id="abn"
                value={form.abn}
                onChange={(e) => setForm({ ...form, abn: e.target.value })}
                placeholder="11 digit ABN"
                maxLength={11}
                className="max-w-xs font-mono"
              />
              {errors.Abn && <p className="text-sm text-destructive">{errors.Abn[0]}</p>}
            </div>

            <div className="grid gap-4 sm:grid-cols-2">
              <div className="space-y-2">
                <Label htmlFor="contactEmail">Contact Email</Label>
                <Input
                  id="contactEmail"
                  type="email"
                  value={form.contactEmail}
                  onChange={(e) => setForm({ ...form, contactEmail: e.target.value })}
                />
                {errors.ContactEmail && (
                  <p className="text-sm text-destructive">{errors.ContactEmail[0]}</p>
                )}
              </div>

              <div className="space-y-2">
                <Label htmlFor="contactPhone">Contact Phone</Label>
                <Input
                  id="contactPhone"
                  value={form.contactPhone}
                  onChange={(e) => setForm({ ...form, contactPhone: e.target.value })}
                />
              </div>
            </div>

            <div className="space-y-2">
              <Label htmlFor="address">Address</Label>
              <Input
                id="address"
                value={form.address}
                onChange={(e) => setForm({ ...form, address: e.target.value })}
              />
            </div>

            <div className="flex items-center gap-3 pt-2">
              <Button type="submit" disabled={saving}>
                {saving && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                {saving ? 'Saving...' : 'Save Changes'}
              </Button>
              {saved && (
                <span className="flex items-center gap-1.5 text-sm text-emerald-600">
                  <Check className="h-4 w-4" />
                  Changes saved
                </span>
              )}
            </div>
          </form>
        </CardContent>
      </Card>

      <Card className="max-w-2xl">
        <CardHeader>
          <CardTitle className="flex items-center gap-2 text-base">
            <Mail className="h-4 w-4 text-muted-foreground" />
            Email Templates
          </CardTitle>
          <CardDescription>
            Manage the email templates used for notifications and automated communications.
          </CardDescription>
        </CardHeader>
        <CardContent>
          <Link to="/settings/email-templates">
            <Button variant="outline" className="gap-2">
              Manage Email Templates
              <ChevronRight className="h-4 w-4" />
            </Button>
          </Link>
        </CardContent>
      </Card>
    </div>
  );
}
