-- STOM.Database/Tables/002-Scores.sql
CREATE TABLE Scores (
    Id INT PRIMARY KEY IDENTITY(1,1),
    GameId INT NOT NULL,
    Value INT NOT NULL,
    CreatedAt DATETIME DEFAULT GETUTCDATE(),
    FOREIGN KEY (GameId) REFERENCES Games(Id) ON DELETE CASCADE
);

CREATE INDEX IX_Scores_GameId ON Scores(GameId);
CREATE INDEX IX_Scores_CreatedAt ON Scores(CreatedAt DESC);
