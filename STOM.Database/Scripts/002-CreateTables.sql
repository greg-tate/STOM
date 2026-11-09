-- STOM.Database/Scripts/002-CreateTables.sql
-- Create all tables in proper dependency order

USE STOM_DB;
GO

-- Games table
CREATE TABLE Games (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(255) NOT NULL,
    CreatedAt DATETIME DEFAULT GETUTCDATE()
);

CREATE INDEX IX_Games_CreatedAt ON Games(CreatedAt DESC);
GO

-- Scores table (depends on Games)
CREATE TABLE Scores (
    Id INT PRIMARY KEY IDENTITY(1,1),
    GameId INT NOT NULL,
    Value INT NOT NULL,
    CreatedAt DATETIME DEFAULT GETUTCDATE(),
    FOREIGN KEY (GameId) REFERENCES Games(Id) ON DELETE CASCADE
);

CREATE INDEX IX_Scores_GameId ON Scores(GameId);
CREATE INDEX IX_Scores_CreatedAt ON Scores(CreatedAt DESC);
GO

-- Tournaments table
CREATE TABLE Tournaments (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(255) NOT NULL,
    Date DATETIME NOT NULL,
    Status NVARCHAR(50) DEFAULT 'Planning',
    CreatedAt DATETIME DEFAULT GETUTCDATE()
);

CREATE INDEX IX_Tournaments_Date ON Tournaments(Date);
CREATE INDEX IX_Tournaments_Status ON Tournaments(Status);
GO

-- Players table
CREATE TABLE Players (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(255) NOT NULL,
    Email NVARCHAR(255) NOT NULL UNIQUE,
    Role NVARCHAR(50) DEFAULT 'Scorer',
    CreatedAt DATETIME DEFAULT GETUTCDATE()
);

CREATE INDEX IX_Players_Email ON Players(Email);
CREATE INDEX IX_Players_Role ON Players(Role);
GO

-- AuditLogs table (admin operations)
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
GO
