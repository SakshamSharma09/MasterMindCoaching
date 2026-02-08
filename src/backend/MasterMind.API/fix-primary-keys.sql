-- Add primary key constraints for junction tables if they don't exist

-- Check and add primary key for UserRoles table
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE CONSTRAINT_NAME = 'PK_UserRoles' AND TABLE_NAME = 'UserRoles')
BEGIN
    ALTER TABLE UserRoles ADD CONSTRAINT PK_UserRoles PRIMARY KEY (UserId, RoleId);
END

-- Check and add primary key for StudentClasses table
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE CONSTRAINT_NAME = 'PK_StudentClasses' AND TABLE_NAME = 'StudentClasses')
BEGIN
    ALTER TABLE StudentClasses ADD CONSTRAINT PK_StudentClasses PRIMARY KEY (StudentId, ClassId);
END

-- Check and add primary key for TeacherClasses table
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE CONSTRAINT_NAME = 'PK_TeacherClasses' AND TABLE_NAME = 'TeacherClasses')
BEGIN
    ALTER TABLE TeacherClasses ADD CONSTRAINT PK_TeacherClasses PRIMARY KEY (TeacherId, ClassId);
END

-- Check and add primary key for ClassSubjects table
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE CONSTRAINT_NAME = 'PK_ClassSubjects' AND TABLE_NAME = 'ClassSubjects')
BEGIN
    ALTER TABLE ClassSubjects ADD CONSTRAINT PK_ClassSubjects PRIMARY KEY (ClassId, SubjectId);
END

PRINT 'Primary key constraints added successfully';
