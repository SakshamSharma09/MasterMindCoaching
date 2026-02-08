-- Add SessionId column to Students table
ALTER TABLE [MasterMindCoaching].[dbo].[Students] 
ADD [SessionId] INT NULL;

-- Add foreign key constraint to Sessions table (if Sessions table exists)
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Sessions' AND TABLE_SCHEMA = 'dbo')
BEGIN
    ALTER TABLE [MasterMindCoaching].[dbo].[Students] 
    ADD CONSTRAINT [FK_Students_Sessions_SessionId] 
    FOREIGN KEY ([SessionId]) REFERENCES [MasterMindCoaching].[dbo].[Sessions]([Id]) 
    ON DELETE SET NULL;
    
    PRINT 'SessionId column and foreign key constraint added successfully';
END
ELSE
BEGIN
    PRINT 'SessionId column added successfully (Sessions table not found, foreign key not created)';
END

-- Verify the column was added
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Students' 
    AND COLUMN_NAME = 'SessionId';
