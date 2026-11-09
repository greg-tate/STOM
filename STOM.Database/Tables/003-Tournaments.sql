-- STOM.Database/Tables/003-Tournaments.sql
CREATE TABLE Tournaments (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(255) NOT NULL,
    Date DATETIME NOT NULL,
    Status NVARCHAR(50) DEFAULT 'Planning',
    CreatedAt DATETIME DEFAULT GETUTCDATE()
);

CREATE INDEX IX_Tournaments_Date ON Tournaments(Date);
CREATE INDEX IX_Tournaments_Status ON Tournaments(Status);
