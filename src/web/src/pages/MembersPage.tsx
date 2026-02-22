import { useState } from 'react';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
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

const roleLabels: Record<string, string> = {
  org_admin: 'Admin',
  plan_manager: 'Plan Manager',
  finance: 'Finance',
};

const roleBadgeVariant: Record<string, 'default' | 'secondary' | 'outline'> = {
  org_admin: 'default',
  plan_manager: 'secondary',
  finance: 'outline',
};

export function MembersPage() {
  const { members, isLoading, updateRole, deactivateMember } = useMembers();
  const [dialogOpen, setDialogOpen] = useState(false);

  if (isLoading) {
    return (
      <div className="space-y-4">
        <Skeleton className="h-8 w-48" />
        <Skeleton className="h-64 w-full" />
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold">Team Members</h1>
        <Button onClick={() => setDialogOpen(true)}>Invite Member</Button>
      </div>

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
          {members.map((member) => (
            <TableRow key={member.userId}>
              <TableCell className="font-medium">
                {member.firstName} {member.lastName}
              </TableCell>
              <TableCell>{member.email}</TableCell>
              <TableCell>
                <Badge variant={roleBadgeVariant[member.role] ?? 'secondary'}>
                  {roleLabels[member.role] ?? member.role}
                </Badge>
              </TableCell>
              <TableCell>
                {member.isActive ? (
                  <Badge variant="outline" className="border-green-300 text-green-700">
                    Active
                  </Badge>
                ) : (
                  <Badge variant="outline" className="border-red-300 text-red-700">
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
                      className="text-destructive"
                      onClick={() => deactivateMember(member.userId)}
                    >
                      Remove
                    </Button>
                  </div>
                )}
              </TableCell>
            </TableRow>
          ))}
          {members.length === 0 && (
            <TableRow>
              <TableCell colSpan={5} className="text-center text-muted-foreground">
                No team members found.
              </TableCell>
            </TableRow>
          )}
        </TableBody>
      </Table>

      <InviteMemberDialog open={dialogOpen} onOpenChange={setDialogOpen} />
    </div>
  );
}
