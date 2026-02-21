-- Event Store Schema
-- Octocare event-sourced financial ledger

-- Event streams represent aggregates (e.g., participant plan budget, invoice)
CREATE TABLE IF NOT EXISTS event_streams (
    stream_id   UUID            NOT NULL PRIMARY KEY,
    stream_type VARCHAR(256)    NOT NULL,
    tenant_id   UUID            NOT NULL,
    created_at  TIMESTAMPTZ     NOT NULL DEFAULT now()
);

CREATE INDEX IF NOT EXISTS ix_event_streams_tenant_id ON event_streams (tenant_id);
CREATE INDEX IF NOT EXISTS ix_event_streams_stream_type ON event_streams (stream_type);

-- Immutable event log
CREATE TABLE IF NOT EXISTS events (
    id          UUID            NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    stream_id   UUID            NOT NULL REFERENCES event_streams (stream_id),
    stream_type VARCHAR(256)    NOT NULL,
    event_type  VARCHAR(256)    NOT NULL,
    payload     JSONB           NOT NULL,
    metadata    JSONB           NOT NULL DEFAULT '{}',
    version     BIGINT          NOT NULL,
    tenant_id   UUID            NOT NULL,
    created_at  TIMESTAMPTZ     NOT NULL DEFAULT now()
);

-- Optimistic concurrency: only one event per stream per version
ALTER TABLE events ADD CONSTRAINT uq_events_stream_version UNIQUE (stream_id, version);

CREATE INDEX IF NOT EXISTS ix_events_stream_id_version ON events (stream_id, version);
CREATE INDEX IF NOT EXISTS ix_events_event_type ON events (event_type);
CREATE INDEX IF NOT EXISTS ix_events_tenant_id ON events (tenant_id);
CREATE INDEX IF NOT EXISTS ix_events_created_at ON events (created_at);

-- Row-Level Security (activate when tables are populated and tested)
-- ALTER TABLE event_streams ENABLE ROW LEVEL SECURITY;
-- ALTER TABLE events ENABLE ROW LEVEL SECURITY;
--
-- CREATE POLICY tenant_isolation_streams ON event_streams
--     USING (tenant_id = current_setting('app.current_tenant')::UUID);
--
-- CREATE POLICY tenant_isolation_events ON events
--     USING (tenant_id = current_setting('app.current_tenant')::UUID);
