-- MoneyFlow database creation. Safe to re-run.
IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = N'MoneyFlowDb')
BEGIN
    CREATE DATABASE MoneyFlowDb;
END
GO
