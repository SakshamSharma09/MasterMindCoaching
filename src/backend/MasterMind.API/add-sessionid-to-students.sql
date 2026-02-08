-- Add SessionId column to Students table if it doesn't exist
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Students' AND COLUMN_NAME = 'SessionId')
BEGIN
    ALTER TABLE Students ADD SessionId int NULL;
    
    -- Add foreign key constraint if Sessions table exists
    IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Sessions')
    BEGIN
        ALTER TABLE Students ADD CONSTRAINT FK_Students_Sessions_SessionId 
        FOREIGN KEY (SessionId) REFERENCES Sessions(Id) ON DELETE SET NULL;
    END
    
    PRINT 'SessionId column added to Students table successfully';
END
ELSE
BEGIN
    PRINT 'SessionId column already exists in Students table';
END
