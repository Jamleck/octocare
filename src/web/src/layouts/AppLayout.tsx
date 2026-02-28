import { useAuth } from '@/hooks/useAuth';
import { Link, NavLink, Outlet, useLocation } from 'react-router';
import { Button } from '@/components/ui/button';
import { Avatar, AvatarFallback } from '@/components/ui/avatar';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { useApiToken } from '@/hooks/useApiToken';
import {
  LayoutDashboard,
  Users,
  UserPlus,
  Building2,
  FileText,
  Send,
  Banknote,
  Settings,
  LogOut,
  ChevronRight,
} from 'lucide-react';

const navItems = [
  { to: '/dashboard', label: 'Dashboard', icon: LayoutDashboard },
  { to: '/participants', label: 'Participants', icon: Users },
  { to: '/providers', label: 'Providers', icon: Building2 },
  { to: '/invoices', label: 'Invoices', icon: FileText },
  { to: '/claims', label: 'Claims', icon: Send },
  { to: '/payments', label: 'Payments', icon: Banknote },
  { to: '/members', label: 'Team', icon: UserPlus },
  { to: '/settings', label: 'Settings', icon: Settings },
];

export function AppLayout() {
  const { user, logout } = useAuth();
  useApiToken();
  const location = useLocation();

  const initials = user?.name
    ?.split(' ')
    .map((n) => n[0])
    .join('')
    .toUpperCase()
    .slice(0, 2) ?? '?';

  // Build breadcrumb from path
  const pathSegments = location.pathname.split('/').filter(Boolean);
  const showBreadcrumb = pathSegments.length > 1;

  return (
    <div className="flex min-h-screen flex-col">
      <header className="sticky top-0 z-50 border-b bg-card/80 backdrop-blur-sm">
        <div className="mx-auto flex h-14 max-w-7xl items-center gap-6 px-4">
          <Link to="/dashboard" className="flex items-center gap-2 text-lg font-bold tracking-tight text-primary">
            <div className="flex h-7 w-7 items-center justify-center rounded-lg bg-primary text-[11px] font-black text-primary-foreground">
              O
            </div>
            Octocare
          </Link>

          <nav className="flex items-center gap-0.5">
            {navItems.map((item) => (
              <NavLink
                key={item.to}
                to={item.to}
                className={({ isActive }) =>
                  `flex items-center gap-1.5 rounded-md px-3 py-1.5 text-sm font-medium transition-colors ${
                    isActive
                      ? 'bg-primary/10 text-primary'
                      : 'text-muted-foreground hover:bg-accent hover:text-foreground'
                  }`
                }
              >
                <item.icon className="h-4 w-4" />
                {item.label}
              </NavLink>
            ))}
          </nav>

          <div className="ml-auto">
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button variant="ghost" className="relative h-8 w-8 rounded-full">
                  <Avatar className="h-8 w-8">
                    <AvatarFallback className="bg-primary/10 text-xs font-medium text-primary">
                      {initials}
                    </AvatarFallback>
                  </Avatar>
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end" className="w-56">
                <div className="flex items-center gap-3 p-3">
                  <Avatar className="h-9 w-9">
                    <AvatarFallback className="bg-primary/10 text-sm font-medium text-primary">
                      {initials}
                    </AvatarFallback>
                  </Avatar>
                  <div className="flex flex-col">
                    <p className="text-sm font-medium">{user?.name}</p>
                    <p className="text-xs text-muted-foreground">{user?.email}</p>
                  </div>
                </div>
                <DropdownMenuSeparator />
                <DropdownMenuItem onClick={() => logout({ logoutParams: { returnTo: window.location.origin } })}>
                  <LogOut className="mr-2 h-4 w-4" />
                  Log out
                </DropdownMenuItem>
              </DropdownMenuContent>
            </DropdownMenu>
          </div>
        </div>
      </header>

      {showBreadcrumb && (
        <div className="border-b bg-card/40">
          <div className="mx-auto flex max-w-7xl items-center gap-1 px-4 py-2 text-sm text-muted-foreground">
            {pathSegments.map((segment, i) => {
              const path = '/' + pathSegments.slice(0, i + 1).join('/');
              const isLast = i === pathSegments.length - 1;
              const label = segment.charAt(0).toUpperCase() + segment.slice(1).replace(/-/g, ' ');
              return (
                <span key={path} className="flex items-center gap-1">
                  {i > 0 && <ChevronRight className="h-3.5 w-3.5" />}
                  {isLast ? (
                    <span className="font-medium text-foreground">{label}</span>
                  ) : (
                    <Link to={path} className="hover:text-foreground transition-colors">
                      {label}
                    </Link>
                  )}
                </span>
              );
            })}
          </div>
        </div>
      )}

      <main className="mx-auto w-full max-w-7xl flex-1 px-4 py-6">
        <Outlet />
      </main>
    </div>
  );
}
