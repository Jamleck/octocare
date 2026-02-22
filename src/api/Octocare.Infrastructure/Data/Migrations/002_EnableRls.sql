-- Enable Row-Level Security for tenant-scoped tables
-- Run after EF Core migrations create the tables

-- Organisations
ALTER TABLE organisations ENABLE ROW LEVEL SECURITY;
CREATE POLICY tenant_isolation_organisations ON organisations
    USING (tenant_id = current_setting('app.current_tenant')::UUID);

-- User org memberships
ALTER TABLE user_org_memberships ENABLE ROW LEVEL SECURITY;
CREATE POLICY tenant_isolation_memberships ON user_org_memberships
    USING (tenant_id = current_setting('app.current_tenant')::UUID);

-- Participants
ALTER TABLE participants ENABLE ROW LEVEL SECURITY;
CREATE POLICY tenant_isolation_participants ON participants
    USING (tenant_id = current_setting('app.current_tenant')::UUID);

-- Note: The 'users' table does NOT have RLS because users are global
-- (a user can belong to multiple organisations).

-- Note: The database superuser bypasses RLS by default.
-- The application should connect as a non-superuser role for RLS to take effect.
