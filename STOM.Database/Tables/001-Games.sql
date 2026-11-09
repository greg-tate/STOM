-- STOM.Database/Tables/001-Games.sql
CREATE TABLE Games (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(255) NOT NULL,
    CreatedAt DATETIME DEFAULT GETUTCDATE()
);

CREATE INDEX IX_Games_CreatedAt ON Games(CreatedAt DESC);
