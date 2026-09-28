-- Clear demo seed from 004_seed_data.sql (demo@moneyflow.app).
-- Does NOT drop tables. Does NOT delete other users.
-- Child rows of MonthlyPeriods cascade; remaining FKs are NO ACTION so delete order matters.
USE MoneyFlowDb;
GO

SET NOCOUNT ON;

DECLARE @UserId INT;
SELECT @UserId = UserId FROM Users WHERE Email = N'demo@moneyflow.app';

IF @UserId IS NULL
BEGIN
    PRINT 'No seed user demo@moneyflow.app - nothing to clear.';
    RETURN;
END

BEGIN TRANSACTION;

DELETE FROM RecurringTemplates WHERE UserId = @UserId;
DELETE FROM SavingContributions WHERE UserId = @UserId;
DELETE FROM Transactions WHERE UserId = @UserId;
DELETE FROM BudgetAllocations WHERE UserId = @UserId;
DELETE FROM ExpenseEntries WHERE UserId = @UserId;
DELETE FROM IncomeEntries WHERE UserId = @UserId;
DELETE FROM SavingGoals WHERE UserId = @UserId;
DELETE FROM MonthlyPeriods WHERE UserId = @UserId;
DELETE FROM Categories WHERE UserId = @UserId;
DELETE FROM Users WHERE UserId = @UserId;

COMMIT TRANSACTION;

PRINT 'Cleared seed data for demo@moneyflow.app.';
GO
