-- STOM.Database/Tables/004-Players.sql
CREATE TABLE Players (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(255) NOT NULL,
    Email NVARCHAR(255) NOT NULL UNIQUE,
    Role NVARCHAR(50) DEFAULT 'Scorer',
    CreatedAt DATETIME DEFAULT GETUTCDATE()
);

CREATE INDEX IX_Players_Email ON Players(Email);
CREATE INDEX IX_Players_Role ON Players(Role);
