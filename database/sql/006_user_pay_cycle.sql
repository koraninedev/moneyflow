-- Payday-aligned monthly period settings.
USE MoneyFlowDb;
GO

IF COL_LENGTH('dbo.Users', 'PeriodStartDay') IS NULL
BEGIN
    ALTER TABLE Users ADD PeriodStartDay TINYINT NOT NULL CONSTRAINT DF_Users_PeriodStartDay DEFAULT (26);
    ALTER TABLE Users ADD CONSTRAINT CK_Users_PeriodStartDay CHECK (PeriodStartDay BETWEEN 1 AND 28);
END
GO

IF COL_LENGTH('dbo.Users', 'SkipWeekendPayday') IS NULL
BEGIN
    ALTER TABLE Users ADD SkipWeekendPayday BIT NOT NULL CONSTRAINT DF_Users_SkipWeekendPayday DEFAULT (1);
END
GO
