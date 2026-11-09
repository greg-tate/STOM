-- STOM.Database/Scripts/003-SeedData.sql
-- Seed initial test data

USE STOM_DB;
GO

-- Insert sample games
INSERT INTO Games (Name) VALUES
    (N'Game 1 - Preliminary Round'),
    (N'Game 2 - Semifinal'),
    (N'Game 3 - Final');
GO

-- Insert sample players
INSERT INTO Players (Name, Email, Role) VALUES
    (N'John Scorer', N'john@tournament.local', N'Scorer'),
    (N'Jane Admin', N'jane@tournament.local', N'Admin'),
    (N'Admin SuperUser', N'super@tournament.local', N'SuperAdmin');
GO

-- Insert sample tournament
INSERT INTO Tournaments (Name, Date, Status) VALUES
    (N'Regional Championship 2026', N'2026-11-15', N'Planning');
GO

-- Log initial setup
INSERT INTO AuditLogs (Action, PerformedBy, Details, Timestamp) VALUES
    (N'Database initialized', N'System', N'Seed data loaded', GETUTCDATE());
GO
