import { Link, useParams } from 'react-router';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Skeleton } from '@/components/ui/skeleton';
import { useProvider } from '@/hooks/useProviders';
import {
  ArrowLeft,
  Pencil,
  Building2,
  Phone,
  Mail,
  MapPin,
  Hash,
} from 'lucide-react';

export function ProviderDetailPage() {
  const { id } = useParams<{ id: string }>();
  const { provider, isLoading } = useProvider(id!);

  if (isLoading) {
    return (
      <div className="space-y-4">
        <Skeleton className="h-5 w-32" />
        <Skeleton className="h-8 w-64" />
        <Skeleton className="h-48 rounded-lg" />
      </div>
    );
  }

  if (!provider) {
    return (
      <div className="flex flex-col items-center py-16">
        <Building2 className="h-10 w-10 text-muted-foreground" />
        <p className="mt-3 text-lg font-medium">Provider not found</p>
        <Button variant="outline" className="mt-4" asChild>
          <Link to="/providers">
            <ArrowLeft className="mr-2 h-4 w-4" />
            Back to Providers
          </Link>
        </Button>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div>
        <Button variant="ghost" size="sm" className="-ml-2 mb-2 text-muted-foreground" asChild>
          <Link to="/providers">
            <ArrowLeft className="mr-1 h-4 w-4" />
            Providers
          </Link>
        </Button>

        <div className="flex items-center justify-between">
          <div className="flex items-center gap-3">
            <div className="flex h-10 w-10 items-center justify-center rounded-full bg-primary/10 text-primary">
              <Building2 className="h-5 w-5" />
            </div>
            <div>
              <h1 className="text-2xl font-bold tracking-tight">{provider.name}</h1>
              {provider.abn && (
                <p className="font-mono text-sm text-muted-foreground">ABN {provider.abn}</p>
              )}
            </div>
            <Badge
              variant="secondary"
              className={
                provider.isActive
                  ? 'border-emerald-200 bg-emerald-50 text-emerald-700'
                  : 'text-muted-foreground'
              }
            >
              {provider.isActive ? 'Active' : 'Inactive'}
            </Badge>
          </div>
          <Button variant="outline" asChild>
            <Link to={`/providers/${provider.id}/edit`}>
              <Pencil className="mr-2 h-4 w-4" />
              Edit
            </Link>
          </Button>
        </div>
      </div>

      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2 text-base">
            <Building2 className="h-4 w-4 text-muted-foreground" />
            Provider Details
          </CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <DetailRow icon={Hash} label="ABN" value={provider.abn} mono />
          <DetailRow icon={Mail} label="Email" value={provider.contactEmail} />
          <DetailRow icon={Phone} label="Phone" value={provider.contactPhone} />
          <DetailRow icon={MapPin} label="Address" value={provider.address} />
        </CardContent>
      </Card>
    </div>
  );
}

function DetailRow({
  icon: Icon,
  label,
  value,
  mono,
}: {
  icon: typeof Building2;
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
