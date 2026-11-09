-- STOM.Database/Scripts/001-CreateDatabase.sql
-- Creates the STOM_DB database (LocalDB)

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = N'STOM_DB')
BEGIN
    CREATE DATABASE STOM_DB;
END
GO

USE STOM_DB;
