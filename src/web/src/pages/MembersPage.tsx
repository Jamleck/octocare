import { useState } from 'react';
import { Button } from '@/components/ui/button';
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
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { Skeleton } from '@/components/ui/skeleton';
import { useMembers } from '@/hooks/useMembers';
import { InviteMemberDialog } from '@/components/InviteMemberDialog';
import { ErrorBanner } from '@/components/ErrorBanner';
import { UserPlus, Users, Shield, Briefcase, Calculator } from 'lucide-react';

const roleLabels: Record<string, string> = {
  org_admin: 'Admin',
  plan_manager: 'Plan Manager',
  finance: 'Finance',
};

const roleIcons: Record<string, typeof Shield> = {
  org_admin: Shield,
  plan_manager: Briefcase,
  finance: Calculator,
};

const roleBadgeStyles: Record<string, string> = {
  org_admin: 'border-primary/20 bg-primary/10 text-primary',
  plan_manager: 'border-amber-200 bg-amber-50 text-amber-700',
  finance: 'border-emerald-200 bg-emerald-50 text-emerald-700',
};

export function MembersPage() {
  const { members, isLoading, error, updateRole, deactivateMember, refetch } = useMembers();
  const [dialogOpen, setDialogOpen] = useState(false);

  if (isLoading) {
    return (
      <div className="space-y-4">
        <div className="flex items-center justify-between">
          <Skeleton className="h-8 w-48" />
          <Skeleton className="h-9 w-36" />
        </div>
        <Skeleton className="h-64 w-full rounded-lg" />
      </div>
    );
  }

  if (error) {
    return (
      <div className="space-y-6">
        <div className="flex items-center justify-between">
          <h1 className="text-2xl font-bold tracking-tight">Team Members</h1>
        </div>
        <ErrorBanner message={error.message} onRetry={refetch} />
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Team Members</h1>
          {members.length > 0 && (
            <p className="text-sm text-muted-foreground">
              {members.filter((m) => m.isActive).length} active member{members.filter((m) => m.isActive).length !== 1 ? 's' : ''}
            </p>
          )}
        </div>
        <Button onClick={() => setDialogOpen(true)}>
          <UserPlus className="mr-2 h-4 w-4" />
          Invite Member
        </Button>
      </div>

      {members.length === 0 ? (
        <Card>
          <CardContent className="flex flex-col items-center justify-center py-16">
            <div className="rounded-full bg-primary/10 p-4">
              <Users className="h-8 w-8 text-primary" />
            </div>
            <h3 className="mt-4 text-lg font-semibold">No team members yet</h3>
            <p className="mt-1 max-w-sm text-center text-sm text-muted-foreground">
              Invite plan managers, finance staff, and admins to collaborate on participant management.
            </p>
            <Button className="mt-6" onClick={() => setDialogOpen(true)}>
              <UserPlus className="mr-2 h-4 w-4" />
              Invite Your First Member
            </Button>
          </CardContent>
        </Card>
      ) : (
        <div className="rounded-lg border">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Name</TableHead>
                <TableHead>Email</TableHead>
                <TableHead>Role</TableHead>
                <TableHead>Status</TableHead>
                <TableHead className="w-[200px]">Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {members.map((member) => {
                const RoleIcon = roleIcons[member.role] ?? Briefcase;
                return (
                  <TableRow key={member.userId}>
                    <TableCell className="font-medium">
                      {member.firstName} {member.lastName}
                    </TableCell>
                    <TableCell className="text-muted-foreground">{member.email}</TableCell>
                    <TableCell>
                      <Badge variant="outline" className={roleBadgeStyles[member.role] ?? ''}>
                        <RoleIcon className="mr-1 h-3 w-3" />
                        {roleLabels[member.role] ?? member.role}
                      </Badge>
                    </TableCell>
                    <TableCell>
                      {member.isActive ? (
                        <Badge variant="secondary" className="border-emerald-200 bg-emerald-50 text-emerald-700">
                          Active
                        </Badge>
                      ) : (
                        <Badge variant="secondary" className="text-muted-foreground">
                          Inactive
                        </Badge>
                      )}
                    </TableCell>
                    <TableCell>
                      {member.isActive && (
                        <div className="flex items-center gap-2">
                          <Select
                            value={member.role}
                            onValueChange={(role) => updateRole(member.userId, { role })}
                          >
                            <SelectTrigger className="h-8 w-[140px]">
                              <SelectValue />
                            </SelectTrigger>
                            <SelectContent>
                              <SelectItem value="org_admin">Admin</SelectItem>
                              <SelectItem value="plan_manager">Plan Manager</SelectItem>
                              <SelectItem value="finance">Finance</SelectItem>
                            </SelectContent>
                          </Select>
                          <Button
                            variant="ghost"
                            size="sm"
                            className="text-destructive hover:text-destructive"
                            onClick={() => deactivateMember(member.userId)}
                          >
                            Remove
                          </Button>
                        </div>
                      )}
                    </TableCell>
                  </TableRow>
                );
              })}
            </TableBody>
          </Table>
        </div>
      )}

      <InviteMemberDialog open={dialogOpen} onOpenChange={setDialogOpen} />
    </div>
  );
}
