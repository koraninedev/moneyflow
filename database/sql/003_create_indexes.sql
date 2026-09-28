-- Performance indexes justified by dashboard/list query patterns. PK/UNIQUE already exist.
USE MoneyFlowDb;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Categories_User_Type')
    CREATE INDEX IX_Categories_User_Type ON Categories (UserId, Type, IsActive);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_IncomeEntries_Period')
    CREATE INDEX IX_IncomeEntries_Period ON IncomeEntries (MonthlyPeriodId) INCLUDE (Amount, IsActive);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ExpenseEntries_Period')
    CREATE INDEX IX_ExpenseEntries_Period ON ExpenseEntries (MonthlyPeriodId) INCLUDE (Amount, Classification, IsPaid);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Transactions_Period_Category')
    CREATE INDEX IX_Transactions_Period_Category ON Transactions (MonthlyPeriodId, CategoryId) INCLUDE (Amount);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Transactions_Period_Date')
    CREATE INDEX IX_Transactions_Period_Date ON Transactions (MonthlyPeriodId, TransactionDate DESC);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SavingContributions_Goal')
    CREATE INDEX IX_SavingContributions_Goal ON SavingContributions (SavingGoalId) INCLUDE (Amount);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SavingContributions_Period')
    CREATE INDEX IX_SavingContributions_Period ON SavingContributions (MonthlyPeriodId) INCLUDE (Amount);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_RecurringTemplates_User_Active')
    CREATE INDEX IX_RecurringTemplates_User_Active ON RecurringTemplates (UserId, IsActive);
GO
