-- MoneyFlow tables. Run after 001_create_database.sql.
USE MoneyFlowDb;
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Users')
BEGIN
    CREATE TABLE Users (
        UserId          INT IDENTITY(1,1)   NOT NULL,
        Email           NVARCHAR(256)       NOT NULL,
        PasswordHash    NVARCHAR(256)       NOT NULL,
        DisplayName     NVARCHAR(100)       NOT NULL,
        IsActive        BIT                 NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT (1),
        PeriodStartDay  TINYINT             NOT NULL CONSTRAINT DF_Users_PeriodStartDay DEFAULT (26),
        SkipWeekendPayday BIT               NOT NULL CONSTRAINT DF_Users_SkipWeekendPayday DEFAULT (1),
        CreatedAt       DATETIME2(3)        NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt       DATETIME2(3)        NOT NULL CONSTRAINT DF_Users_UpdatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_Users PRIMARY KEY CLUSTERED (UserId),
        CONSTRAINT UQ_Users_Email UNIQUE (Email),
        CONSTRAINT CK_Users_PeriodStartDay CHECK (PeriodStartDay BETWEEN 1 AND 28)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Categories')
BEGIN
    CREATE TABLE Categories (
        CategoryId      INT IDENTITY(1,1)   NOT NULL,
        UserId          INT                 NOT NULL,
        Name            NVARCHAR(100)       NOT NULL,
        Type            VARCHAR(10)         NOT NULL,
        Classification  VARCHAR(10)         NULL,
        TrackingMode    VARCHAR(20)         NOT NULL CONSTRAINT DF_Categories_TrackingMode DEFAULT ('Simple'),
        Icon            VARCHAR(50)         NULL,
        Color           VARCHAR(20)         NULL,
        SortOrder       INT                 NOT NULL CONSTRAINT DF_Categories_SortOrder DEFAULT (0),
        IsActive        BIT                 NOT NULL CONSTRAINT DF_Categories_IsActive DEFAULT (1),
        CreatedAt       DATETIME2(3)        NOT NULL CONSTRAINT DF_Categories_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_Categories PRIMARY KEY CLUSTERED (CategoryId),
        CONSTRAINT FK_Categories_Users FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE,
        CONSTRAINT CK_Categories_Type CHECK (Type IN ('Income','Expense','Saving')),
        CONSTRAINT CK_Categories_Classification CHECK (Classification IS NULL OR Classification IN ('Fixed','Variable')),
        CONSTRAINT CK_Categories_TrackingMode CHECK (TrackingMode IN ('Simple','BudgetVsActual')),
        CONSTRAINT UQ_Categories_User_Name UNIQUE (UserId, Name)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'MonthlyPeriods')
BEGIN
    CREATE TABLE MonthlyPeriods (
        MonthlyPeriodId INT IDENTITY(1,1)   NOT NULL,
        UserId          INT                 NOT NULL,
        Year            SMALLINT            NOT NULL,
        Month           TINYINT             NOT NULL,
        CreatedAt       DATETIME2(3)        NOT NULL CONSTRAINT DF_MonthlyPeriods_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_MonthlyPeriods PRIMARY KEY CLUSTERED (MonthlyPeriodId),
        CONSTRAINT FK_MonthlyPeriods_Users FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE,
        CONSTRAINT CK_MonthlyPeriods_Month CHECK (Month BETWEEN 1 AND 12),
        CONSTRAINT UQ_MonthlyPeriods_User_Year_Month UNIQUE (UserId, Year, Month)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'SavingGoals')
BEGIN
    CREATE TABLE SavingGoals (
        SavingGoalId                INT IDENTITY(1,1)   NOT NULL,
        UserId                      INT                 NOT NULL,
        CategoryId                  INT                 NULL,
        Name                        NVARCHAR(150)       NOT NULL,
        TargetAmount                DECIMAL(18,2)       NULL,
        TargetDate                  DATE                NULL,
        PlannedMonthlyContribution  DECIMAL(18,2)       NULL,
        IsActive                    BIT                 NOT NULL CONSTRAINT DF_SavingGoals_IsActive DEFAULT (1),
        SortOrder                   INT                 NOT NULL CONSTRAINT DF_SavingGoals_SortOrder DEFAULT (0),
        CreatedAt                   DATETIME2(3)        NOT NULL CONSTRAINT DF_SavingGoals_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_SavingGoals PRIMARY KEY CLUSTERED (SavingGoalId),
        CONSTRAINT FK_SavingGoals_Users FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE,
        CONSTRAINT FK_SavingGoals_Categories FOREIGN KEY (CategoryId) REFERENCES Categories(CategoryId) ON DELETE NO ACTION
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'RecurringTemplates')
BEGIN
    CREATE TABLE RecurringTemplates (
        RecurringTemplateId    INT IDENTITY(1,1)  NOT NULL,
        UserId                  INT                NOT NULL,
        TargetType              VARCHAR(20)        NOT NULL,
        CategoryId              INT                NULL,
        SavingGoalId            INT                NULL,
        Name                    NVARCHAR(150)      NOT NULL,
        Amount                  DECIMAL(18,2)      NOT NULL,
        Classification          VARCHAR(10)        NULL,
        DueDay                  TINYINT            NULL,
        IsActive                BIT                NOT NULL CONSTRAINT DF_RecurringTemplates_IsActive DEFAULT (1),
        SortOrder               INT                NOT NULL CONSTRAINT DF_RecurringTemplates_SortOrder DEFAULT (0),
        CreatedAt               DATETIME2(3)       NOT NULL CONSTRAINT DF_RecurringTemplates_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt               DATETIME2(3)       NOT NULL CONSTRAINT DF_RecurringTemplates_UpdatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_RecurringTemplates PRIMARY KEY CLUSTERED (RecurringTemplateId),
        CONSTRAINT FK_RecurringTemplates_Users FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE,
        CONSTRAINT FK_RecurringTemplates_Categories FOREIGN KEY (CategoryId) REFERENCES Categories(CategoryId) ON DELETE NO ACTION,
        CONSTRAINT FK_RecurringTemplates_SavingGoals FOREIGN KEY (SavingGoalId) REFERENCES SavingGoals(SavingGoalId) ON DELETE NO ACTION,
        CONSTRAINT CK_RecurringTemplates_TargetType CHECK (TargetType IN ('Income','ExpenseEntry','BudgetAllocation','SavingContribution')),
        CONSTRAINT CK_RecurringTemplates_Classification CHECK (Classification IS NULL OR Classification IN ('Fixed','Variable')),
        CONSTRAINT CK_RecurringTemplates_DueDay CHECK (DueDay IS NULL OR DueDay BETWEEN 1 AND 31),
        CONSTRAINT CK_RecurringTemplates_TargetRef CHECK (
            (TargetType = 'SavingContribution' AND SavingGoalId IS NOT NULL AND CategoryId IS NULL)
            OR
            (TargetType <> 'SavingContribution' AND CategoryId IS NOT NULL AND SavingGoalId IS NULL)
        )
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'IncomeEntries')
BEGIN
    CREATE TABLE IncomeEntries (
        IncomeEntryId       INT IDENTITY(1,1)  NOT NULL,
        UserId              INT                NOT NULL,
        MonthlyPeriodId     INT                NOT NULL,
        CategoryId          INT                NOT NULL,
        Name                NVARCHAR(150)      NOT NULL,
        Amount              DECIMAL(18,2)      NOT NULL,
        IncomeDate          DATE               NOT NULL,
        IsRecurring         BIT                NOT NULL CONSTRAINT DF_IncomeEntries_IsRecurring DEFAULT (0),
        RecurringTemplateId INT                NULL,
        Note                NVARCHAR(500)      NULL,
        IsActive            BIT                NOT NULL CONSTRAINT DF_IncomeEntries_IsActive DEFAULT (1),
        CreatedAt           DATETIME2(3)       NOT NULL CONSTRAINT DF_IncomeEntries_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt           DATETIME2(3)       NOT NULL CONSTRAINT DF_IncomeEntries_UpdatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_IncomeEntries PRIMARY KEY CLUSTERED (IncomeEntryId),
        CONSTRAINT FK_IncomeEntries_Users FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE NO ACTION,
        CONSTRAINT FK_IncomeEntries_MonthlyPeriods FOREIGN KEY (MonthlyPeriodId) REFERENCES MonthlyPeriods(MonthlyPeriodId) ON DELETE CASCADE,
        CONSTRAINT FK_IncomeEntries_Categories FOREIGN KEY (CategoryId) REFERENCES Categories(CategoryId) ON DELETE NO ACTION,
        CONSTRAINT FK_IncomeEntries_RecurringTemplates FOREIGN KEY (RecurringTemplateId) REFERENCES RecurringTemplates(RecurringTemplateId) ON DELETE NO ACTION
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ExpenseEntries')
BEGIN
    CREATE TABLE ExpenseEntries (
        ExpenseEntryId      INT IDENTITY(1,1)  NOT NULL,
        UserId              INT                NOT NULL,
        MonthlyPeriodId     INT                NOT NULL,
        CategoryId          INT                NOT NULL,
        Name                NVARCHAR(150)      NOT NULL,
        Amount              DECIMAL(18,2)      NOT NULL,
        Classification      VARCHAR(10)        NOT NULL,
        DueDate             DATE               NULL,
        IsPaid              BIT                NOT NULL CONSTRAINT DF_ExpenseEntries_IsPaid DEFAULT (0),
        PaidDate            DATE               NULL,
        IsRecurring         BIT                NOT NULL CONSTRAINT DF_ExpenseEntries_IsRecurring DEFAULT (0),
        RecurringTemplateId INT                NULL,
        Note                NVARCHAR(500)      NULL,
        SortOrder           INT                NOT NULL CONSTRAINT DF_ExpenseEntries_SortOrder DEFAULT (0),
        CreatedAt           DATETIME2(3)       NOT NULL CONSTRAINT DF_ExpenseEntries_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt           DATETIME2(3)       NOT NULL CONSTRAINT DF_ExpenseEntries_UpdatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_ExpenseEntries PRIMARY KEY CLUSTERED (ExpenseEntryId),
        CONSTRAINT FK_ExpenseEntries_Users FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE NO ACTION,
        CONSTRAINT FK_ExpenseEntries_MonthlyPeriods FOREIGN KEY (MonthlyPeriodId) REFERENCES MonthlyPeriods(MonthlyPeriodId) ON DELETE CASCADE,
        CONSTRAINT FK_ExpenseEntries_Categories FOREIGN KEY (CategoryId) REFERENCES Categories(CategoryId) ON DELETE NO ACTION,
        CONSTRAINT FK_ExpenseEntries_RecurringTemplates FOREIGN KEY (RecurringTemplateId) REFERENCES RecurringTemplates(RecurringTemplateId) ON DELETE NO ACTION,
        CONSTRAINT CK_ExpenseEntries_Classification CHECK (Classification IN ('Fixed','Variable'))
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'BudgetAllocations')
BEGIN
    CREATE TABLE BudgetAllocations (
        BudgetAllocationId  INT IDENTITY(1,1)  NOT NULL,
        UserId              INT                NOT NULL,
        MonthlyPeriodId     INT                NOT NULL,
        CategoryId          INT                NOT NULL,
        AllocatedAmount     DECIMAL(18,2)      NOT NULL,
        IsRecurring         BIT                NOT NULL CONSTRAINT DF_BudgetAllocations_IsRecurring DEFAULT (0),
        RecurringTemplateId INT                NULL,
        Note                NVARCHAR(500)      NULL,
        CreatedAt           DATETIME2(3)       NOT NULL CONSTRAINT DF_BudgetAllocations_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt           DATETIME2(3)       NOT NULL CONSTRAINT DF_BudgetAllocations_UpdatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_BudgetAllocations PRIMARY KEY CLUSTERED (BudgetAllocationId),
        CONSTRAINT FK_BudgetAllocations_Users FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE NO ACTION,
        CONSTRAINT FK_BudgetAllocations_MonthlyPeriods FOREIGN KEY (MonthlyPeriodId) REFERENCES MonthlyPeriods(MonthlyPeriodId) ON DELETE CASCADE,
        CONSTRAINT FK_BudgetAllocations_Categories FOREIGN KEY (CategoryId) REFERENCES Categories(CategoryId) ON DELETE NO ACTION,
        CONSTRAINT FK_BudgetAllocations_RecurringTemplates FOREIGN KEY (RecurringTemplateId) REFERENCES RecurringTemplates(RecurringTemplateId) ON DELETE NO ACTION,
        CONSTRAINT UQ_BudgetAllocations_Period_Category UNIQUE (MonthlyPeriodId, CategoryId)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Transactions')
BEGIN
    CREATE TABLE Transactions (
        TransactionId       INT IDENTITY(1,1)  NOT NULL,
        UserId              INT                NOT NULL,
        MonthlyPeriodId     INT                NOT NULL,
        CategoryId          INT                NOT NULL,
        Amount              DECIMAL(18,2)      NOT NULL,
        TransactionDate     DATE               NOT NULL,
        Description         NVARCHAR(255)      NOT NULL,
        Note                NVARCHAR(500)      NULL,
        CreatedAt           DATETIME2(3)       NOT NULL CONSTRAINT DF_Transactions_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt           DATETIME2(3)       NOT NULL CONSTRAINT DF_Transactions_UpdatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_Transactions PRIMARY KEY CLUSTERED (TransactionId),
        CONSTRAINT FK_Transactions_Users FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE NO ACTION,
        CONSTRAINT FK_Transactions_MonthlyPeriods FOREIGN KEY (MonthlyPeriodId) REFERENCES MonthlyPeriods(MonthlyPeriodId) ON DELETE CASCADE,
        CONSTRAINT FK_Transactions_Categories FOREIGN KEY (CategoryId) REFERENCES Categories(CategoryId) ON DELETE NO ACTION
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'SavingContributions')
BEGIN
    CREATE TABLE SavingContributions (
        SavingContributionId   INT IDENTITY(1,1)  NOT NULL,
        UserId                  INT                NOT NULL,
        MonthlyPeriodId         INT                NOT NULL,
        SavingGoalId            INT                NOT NULL,
        Amount                  DECIMAL(18,2)      NOT NULL,
        ContributionDate        DATE               NOT NULL,
        IsRecurring             BIT                NOT NULL CONSTRAINT DF_SavingContributions_IsRecurring DEFAULT (0),
        RecurringTemplateId     INT                NULL,
        Note                    NVARCHAR(500)      NULL,
        CreatedAt               DATETIME2(3)       NOT NULL CONSTRAINT DF_SavingContributions_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_SavingContributions PRIMARY KEY CLUSTERED (SavingContributionId),
        CONSTRAINT FK_SavingContributions_Users FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE NO ACTION,
        CONSTRAINT FK_SavingContributions_MonthlyPeriods FOREIGN KEY (MonthlyPeriodId) REFERENCES MonthlyPeriods(MonthlyPeriodId) ON DELETE CASCADE,
        CONSTRAINT FK_SavingContributions_SavingGoals FOREIGN KEY (SavingGoalId) REFERENCES SavingGoals(SavingGoalId) ON DELETE NO ACTION,
        CONSTRAINT FK_SavingContributions_RecurringTemplates FOREIGN KEY (RecurringTemplateId) REFERENCES RecurringTemplates(RecurringTemplateId) ON DELETE NO ACTION
    );
END
GO
