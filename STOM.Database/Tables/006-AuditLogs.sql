-- STOM.Database/Tables/006-AuditLogs.sql
-- Admin operation logging table (immutable audit trail)

CREATE TABLE AuditLogs (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Action NVARCHAR(255) NOT NULL,
    EntityId INT,
    PerformedBy NVARCHAR(255),
    Timestamp DATETIME DEFAULT GETUTCDATE(),
    Details NVARCHAR(MAX),
    IpAddress NVARCHAR(15)
);

CREATE INDEX IX_AuditLogs_Timestamp ON AuditLogs(Timestamp DESC);
CREATE INDEX IX_AuditLogs_PerformedBy ON AuditLogs(PerformedBy);
CREATE INDEX IX_AuditLogs_Action ON AuditLogs(Action);
