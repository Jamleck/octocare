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
import { useParticipants } from '@/hooks/useParticipants';
import { Search, UserPlus, Users } from 'lucide-react';

export function ParticipantsListPage() {
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(1);
  const pageSize = 20;
  const { participants, totalCount, isLoading } = useParticipants(page, pageSize, search || undefined);
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

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Participants</h1>
          {totalCount > 0 && (
            <p className="text-sm text-muted-foreground">
              {totalCount} participant{totalCount !== 1 ? 's' : ''} total
            </p>
          )}
        </div>
        <Button asChild>
          <Link to="/participants/new">
            <UserPlus className="mr-2 h-4 w-4" />
            Add Participant
          </Link>
        </Button>
      </div>

      <div className="relative max-w-sm">
        <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
        <Input
          placeholder="Search by name or NDIS number..."
          value={search}
          onChange={(e) => {
            setSearch(e.target.value);
            setPage(1);
          }}
          className="pl-9"
        />
      </div>

      {participants.length === 0 && !search ? (
        <Card>
          <CardContent className="flex flex-col items-center justify-center py-16">
            <div className="rounded-full bg-primary/10 p-4">
              <Users className="h-8 w-8 text-primary" />
            </div>
            <h3 className="mt-4 text-lg font-semibold">No participants yet</h3>
            <p className="mt-1 max-w-sm text-center text-sm text-muted-foreground">
              Get started by adding your first NDIS participant. You'll be able to manage their plans, budgets, and invoices.
            </p>
            <Button className="mt-6" asChild>
              <Link to="/participants/new">
                <UserPlus className="mr-2 h-4 w-4" />
                Add Your First Participant
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
                  <TableHead>NDIS Number</TableHead>
                  <TableHead>Name</TableHead>
                  <TableHead>Date of Birth</TableHead>
                  <TableHead>Email</TableHead>
                  <TableHead>Phone</TableHead>
                  <TableHead>Status</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {participants.map((p) => (
                  <TableRow
                    key={p.id}
                    className="cursor-pointer transition-colors hover:bg-accent/50"
                    onClick={() => navigate(`/participants/${p.id}`)}
                  >
                    <TableCell className="font-mono text-sm">{p.ndisNumber}</TableCell>
                    <TableCell className="font-medium">
                      {p.firstName} {p.lastName}
                    </TableCell>
                    <TableCell>{new Date(p.dateOfBirth).toLocaleDateString('en-AU')}</TableCell>
                    <TableCell className="text-muted-foreground">{p.email ?? '--'}</TableCell>
                    <TableCell className="text-muted-foreground">{p.phone ?? '--'}</TableCell>
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
                {participants.length === 0 && search && (
                  <TableRow>
                    <TableCell colSpan={6} className="h-32 text-center">
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
