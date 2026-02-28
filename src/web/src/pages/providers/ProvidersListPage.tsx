import { useState } from 'react';
import { Link, useNavigate } from 'react-router';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import { Card, CardContent } from '@/components/ui/card';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import { Skeleton } from '@/components/ui/skeleton';
import { useProviders } from '@/hooks/useProviders';
import { useDebounce } from '@/hooks/useDebounce';
import { ErrorBanner } from '@/components/ErrorBanner';
import { Search, Plus, Building2 } from 'lucide-react';

export function ProvidersListPage() {
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(1);
  const pageSize = 20;
  const debouncedSearch = useDebounce(search, 300);
  const { providers, totalCount, isLoading, error, refetch } = useProviders(page, pageSize, debouncedSearch || undefined);
  const navigate = useNavigate();

  const totalPages = Math.ceil(totalCount / pageSize);

  if (isLoading) {
    return (
      <div className="space-y-4">
        <div className="flex items-center justify-between">
          <Skeleton className="h-8 w-48" />
          <Skeleton className="h-9 w-36" />
        </div>
        <Skeleton className="h-10 w-full max-w-sm" />
        <Skeleton className="h-64 w-full rounded-lg" />
      </div>
    );
  }

  if (error) {
    return (
      <div className="space-y-6">
        <div className="flex items-center justify-between">
          <h1 className="text-2xl font-bold tracking-tight">Providers</h1>
        </div>
        <ErrorBanner message={error.message} onRetry={refetch} />
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Providers</h1>
          {totalCount > 0 && (
            <p className="text-sm text-muted-foreground">
              {totalCount} provider{totalCount !== 1 ? 's' : ''} total
            </p>
          )}
        </div>
        <Button asChild>
          <Link to="/providers/new">
            <Plus className="mr-2 h-4 w-4" />
            Add Provider
          </Link>
        </Button>
      </div>

      <div className="relative max-w-sm">
        <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
        <Input
          placeholder="Search by name or ABN..."
          value={search}
          onChange={(e) => {
            setSearch(e.target.value);
            setPage(1);
          }}
          className="pl-9"
        />
      </div>

      {providers.length === 0 && !search ? (
        <Card>
          <CardContent className="flex flex-col items-center justify-center py-16">
            <div className="rounded-full bg-primary/10 p-4">
              <Building2 className="h-8 w-8 text-primary" />
            </div>
            <h3 className="mt-4 text-lg font-semibold">No providers yet</h3>
            <p className="mt-1 max-w-sm text-center text-sm text-muted-foreground">
              Get started by adding your first service provider. Providers deliver NDIS support services to your participants.
            </p>
            <Button className="mt-6" asChild>
              <Link to="/providers/new">
                <Plus className="mr-2 h-4 w-4" />
                Add Your First Provider
              </Link>
            </Button>
          </CardContent>
        </Card>
      ) : (
        <>
          <div className="rounded-lg border">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Name</TableHead>
                  <TableHead>ABN</TableHead>
                  <TableHead>Email</TableHead>
                  <TableHead>Phone</TableHead>
                  <TableHead>Status</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {providers.map((p) => (
                  <TableRow
                    key={p.id}
                    className="cursor-pointer transition-colors hover:bg-accent/50"
                    onClick={() => navigate(`/providers/${p.id}`)}
                  >
                    <TableCell className="font-medium">{p.name}</TableCell>
                    <TableCell className="font-mono text-sm">{p.abn ?? '--'}</TableCell>
                    <TableCell className="text-muted-foreground">{p.contactEmail ?? '--'}</TableCell>
                    <TableCell className="text-muted-foreground">{p.contactPhone ?? '--'}</TableCell>
                    <TableCell>
                      {p.isActive ? (
                        <Badge variant="secondary" className="border-emerald-200 bg-emerald-50 text-emerald-700">
                          Active
                        </Badge>
                      ) : (
                        <Badge variant="secondary" className="text-muted-foreground">
                          Inactive
                        </Badge>
                      )}
                    </TableCell>
                  </TableRow>
                ))}
                {providers.length === 0 && search && (
                  <TableRow>
                    <TableCell colSpan={5} className="h-32 text-center">
                      <div className="flex flex-col items-center gap-1">
                        <Search className="h-5 w-5 text-muted-foreground" />
                        <p className="text-sm font-medium">No results found</p>
                        <p className="text-xs text-muted-foreground">
                          Try adjusting your search terms.
                        </p>
                      </div>
                    </TableCell>
                  </TableRow>
                )}
              </TableBody>
            </Table>
          </div>

          {totalPages > 1 && (
            <div className="flex items-center justify-between">
              <p className="text-sm text-muted-foreground">
                Showing {(page - 1) * pageSize + 1}–{Math.min(page * pageSize, totalCount)} of{' '}
                {totalCount}
              </p>
              <div className="flex gap-2">
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => setPage((p) => Math.max(1, p - 1))}
                  disabled={page === 1}
                >
                  Previous
                </Button>
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
                  disabled={page === totalPages}
                >
                  Next
                </Button>
              </div>
            </div>
          )}
        </>
      )}
    </div>
  );
}
