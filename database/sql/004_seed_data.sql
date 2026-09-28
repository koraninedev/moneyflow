-- Demo user: demo@moneyflow.app / Demo123!  (BCrypt work factor 11)
USE MoneyFlowDb;
GO

IF EXISTS (SELECT 1 FROM Users WHERE Email = 'demo@moneyflow.app')
BEGIN
    PRINT 'Seed data already present for demo@moneyflow.app - skipping.';
    RETURN;
END
GO

DECLARE @UserId INT;
INSERT INTO Users (Email, PasswordHash, DisplayName)
VALUES ('demo@moneyflow.app', '$2a$11$4BuBnzISmLqF6trWUdF.bOG4GLVNvYAXqFJ5GLMq1gnlFWB/Ao0Ly', 'Demo User');
SET @UserId = SCOPE_IDENTITY();

DECLARE @CatSalary INT, @CatOtherIncome INT, @CatRent INT, @CatPhone INT, @CatInternet INT,
        @CatCreditCard INT, @CatCashCard INT, @CatFood INT, @CatReward INT, @CatSavings INT;

INSERT INTO Categories (UserId, Name, Type, Classification, TrackingMode, Color, SortOrder) VALUES
    (@UserId, N'Salary', 'Income', NULL, 'Simple', '#2F6FED', 1),
    (@UserId, N'Other Income', 'Income', NULL, 'Simple', '#2F6FED', 2),
    (@UserId, N'Rent', 'Expense', 'Variable', 'Simple', '#DC2626', 1),
    (@UserId, N'Phone', 'Expense', 'Fixed', 'Simple', '#DC2626', 2),
    (@UserId, N'Internet', 'Expense', 'Fixed', 'Simple', '#DC2626', 3),
    (@UserId, N'Credit Card', 'Expense', 'Fixed', 'Simple', '#DC2626', 4),
    (@UserId, N'Cash Card', 'Expense', 'Fixed', 'Simple', '#DC2626', 5),
    (@UserId, N'Food', 'Expense', 'Variable', 'BudgetVsActual', '#D97706', 6),
    (@UserId, N'Reward', 'Expense', 'Variable', 'BudgetVsActual', '#D97706', 7),
    (@UserId, N'General Savings', 'Saving', NULL, 'Simple', '#16A34A', 1);

SELECT @CatSalary = CategoryId FROM Categories WHERE UserId = @UserId AND Name = N'Salary';
SELECT @CatOtherIncome = CategoryId FROM Categories WHERE UserId = @UserId AND Name = N'Other Income';
SELECT @CatRent = CategoryId FROM Categories WHERE UserId = @UserId AND Name = N'Rent';
SELECT @CatPhone = CategoryId FROM Categories WHERE UserId = @UserId AND Name = N'Phone';
SELECT @CatInternet = CategoryId FROM Categories WHERE UserId = @UserId AND Name = N'Internet';
SELECT @CatCreditCard = CategoryId FROM Categories WHERE UserId = @UserId AND Name = N'Credit Card';
SELECT @CatCashCard = CategoryId FROM Categories WHERE UserId = @UserId AND Name = N'Cash Card';
SELECT @CatFood = CategoryId FROM Categories WHERE UserId = @UserId AND Name = N'Food';
SELECT @CatReward = CategoryId FROM Categories WHERE UserId = @UserId AND Name = N'Reward';
SELECT @CatSavings = CategoryId FROM Categories WHERE UserId = @UserId AND Name = N'General Savings';

DECLARE @SavingGoalId INT;
INSERT INTO SavingGoals (UserId, CategoryId, Name, TargetAmount, PlannedMonthlyContribution)
VALUES (@UserId, @CatSavings, N'Emergency Fund', 100000.00, 5000.00);
SET @SavingGoalId = SCOPE_IDENTITY();

DECLARE @AugPeriodId INT;
INSERT INTO MonthlyPeriods (UserId, Year, Month) VALUES (@UserId, 2026, 8);
SET @AugPeriodId = SCOPE_IDENTITY();

INSERT INTO IncomeEntries (UserId, MonthlyPeriodId, CategoryId, Name, Amount, IncomeDate)
VALUES (@UserId, @AugPeriodId, @CatSalary, N'August Salary', 40000.00, '2026-08-25');

INSERT INTO ExpenseEntries (UserId, MonthlyPeriodId, CategoryId, Name, Amount, Classification, DueDate, IsPaid, PaidDate) VALUES
    (@UserId, @AugPeriodId, @CatRent, N'ค่าห้อง', 7200.00, 'Variable', '2026-08-05', 1, '2026-08-05'),
    (@UserId, @AugPeriodId, @CatPhone, N'เติมเน็ตโทรศัพท์', 250.00, 'Fixed', '2026-08-05', 1, '2026-08-05'),
    (@UserId, @AugPeriodId, @CatInternet, N'จ่ายค่าเน็ตห้อง', 1068.93, 'Fixed', '2026-08-05', 1, '2026-08-05'),
    (@UserId, @AugPeriodId, @CatCreditCard, N'บัตรเครดิตกรุงศรี', 2500.00, 'Fixed', '2026-08-10', 1, '2026-08-10'),
    (@UserId, @AugPeriodId, @CatCashCard, N'บัตรกดเงินสดกสิกร', 6800.00, 'Fixed', '2026-08-10', 1, '2026-08-10');

INSERT INTO BudgetAllocations (UserId, MonthlyPeriodId, CategoryId, AllocatedAmount) VALUES
    (@UserId, @AugPeriodId, @CatFood, 7000.00),
    (@UserId, @AugPeriodId, @CatReward, 2000.00);

INSERT INTO Transactions (UserId, MonthlyPeriodId, CategoryId, Amount, TransactionDate, Description) VALUES
    (@UserId, @AugPeriodId, @CatFood, 610.00, '2026-08-04', N'Week 1 groceries'),
    (@UserId, @AugPeriodId, @CatFood, 585.00, '2026-08-11', N'Week 2 groceries'),
    (@UserId, @AugPeriodId, @CatFood, 620.00, '2026-08-18', N'Week 3 groceries'),
    (@UserId, @AugPeriodId, @CatFood, 560.00, '2026-08-25', N'Week 4 groceries'),
    (@UserId, @AugPeriodId, @CatReward, 800.00, '2026-08-15', N'New headphones');

INSERT INTO SavingContributions (UserId, MonthlyPeriodId, SavingGoalId, Amount, ContributionDate)
VALUES (@UserId, @AugPeriodId, @SavingGoalId, 5000.00, '2026-08-25');

DECLARE @SepPeriodId INT;
INSERT INTO MonthlyPeriods (UserId, Year, Month) VALUES (@UserId, 2026, 9);
SET @SepPeriodId = SCOPE_IDENTITY();

INSERT INTO IncomeEntries (UserId, MonthlyPeriodId, CategoryId, Name, Amount, IncomeDate)
VALUES (@UserId, @SepPeriodId, @CatSalary, N'September Salary', 40000.00, '2026-09-25');

INSERT INTO ExpenseEntries (UserId, MonthlyPeriodId, CategoryId, Name, Amount, Classification, DueDate, IsPaid, PaidDate) VALUES
    (@UserId, @SepPeriodId, @CatRent, N'ค่าห้อง', 7200.00, 'Variable', '2026-09-25', 1, '2026-09-25'),
    (@UserId, @SepPeriodId, @CatPhone, N'เติมเน็ตโทรศัพท์', 250.00, 'Fixed', '2026-09-25', 1, '2026-09-25'),
    (@UserId, @SepPeriodId, @CatInternet, N'จ่ายค่าเน็ตห้อง', 1068.93, 'Fixed', '2026-09-25', 1, '2026-09-25'),
    (@UserId, @SepPeriodId, @CatCreditCard, N'จ่ายค่าบัตรเครดิตกรุงศรี', 2614.00, 'Fixed', '2026-09-25', 1, '2026-09-25'),
    (@UserId, @SepPeriodId, @CatCashCard, N'จ่ายค่าบัตรกดเงินสดกสิกร', 7111.92, 'Fixed', '2026-09-25', 0, NULL),
    (@UserId, @SepPeriodId, @CatCreditCard, N'จ่ายค่าบัตรเครดิตกสิกร', 3285.48, 'Fixed', '2026-09-28', 0, NULL);

INSERT INTO BudgetAllocations (UserId, MonthlyPeriodId, CategoryId, AllocatedAmount) VALUES
    (@UserId, @SepPeriodId, @CatFood, 7000.00),
    (@UserId, @SepPeriodId, @CatReward, 2000.00);

INSERT INTO Transactions (UserId, MonthlyPeriodId, CategoryId, Amount, TransactionDate, Description) VALUES
    (@UserId, @SepPeriodId, @CatFood, 600.00, '2026-09-04', N'Week 1 groceries'),
    (@UserId, @SepPeriodId, @CatFood, 520.00, '2026-09-11', N'Week 2 groceries'),
    (@UserId, @SepPeriodId, @CatFood, 640.00, '2026-09-18', N'Week 3 groceries'),
    (@UserId, @SepPeriodId, @CatFood, 570.00, '2026-09-25', N'Week 4 groceries');

INSERT INTO SavingContributions (UserId, MonthlyPeriodId, SavingGoalId, Amount, ContributionDate)
VALUES (@UserId, @SepPeriodId, @SavingGoalId, 5000.00, '2026-09-25');

INSERT INTO RecurringTemplates (UserId, TargetType, CategoryId, Name, Amount, Classification, DueDay) VALUES
    (@UserId, 'Income', @CatSalary, N'Monthly Salary', 40000.00, NULL, 25),
    (@UserId, 'ExpenseEntry', @CatPhone, N'เติมเน็ตโทรศัพท์', 250.00, 'Fixed', 25),
    (@UserId, 'ExpenseEntry', @CatInternet, N'จ่ายค่าเน็ตห้อง', 1068.93, 'Fixed', 25),
    (@UserId, 'BudgetAllocation', @CatFood, N'Food Budget', 7000.00, NULL, NULL),
    (@UserId, 'BudgetAllocation', @CatReward, N'Reward Budget', 2000.00, NULL, NULL);

INSERT INTO RecurringTemplates (UserId, TargetType, SavingGoalId, Name, Amount, DueDay)
VALUES (@UserId, 'SavingContribution', @SavingGoalId, N'Emergency Fund Contribution', 5000.00, 25);

PRINT 'Seed data inserted for demo@moneyflow.app.';
GO
