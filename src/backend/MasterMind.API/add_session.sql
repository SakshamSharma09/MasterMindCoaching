CREATE TABLE [Roles] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NULL,
    CONSTRAINT [PK_Roles] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Sessions] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(450) NOT NULL,
    [DisplayName] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NULL,
    [StartDate] datetime2 NOT NULL,
    [EndDate] datetime2 NOT NULL,
    [AcademicYear] nvarchar(max) NOT NULL,
    [IsActive] bit NOT NULL,
    [Status] int NOT NULL,
    [TotalStudents] int NOT NULL,
    [ActiveStudents] int NOT NULL,
    [TotalClasses] int NOT NULL,
    [ActiveClasses] int NOT NULL,
    [TotalTeachers] int NOT NULL,
    [TotalRevenue] decimal(18,2) NOT NULL,
    [TotalExpenses] decimal(18,2) NOT NULL,
    [Settings] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_Sessions] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Subjects] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(450) NOT NULL,
    [Code] nvarchar(max) NULL,
    [Description] nvarchar(max) NULL,
    [Category] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    [IsDeleted] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_Subjects] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Users] (
    [Id] int NOT NULL IDENTITY,
    [Email] nvarchar(450) NOT NULL,
    [Mobile] nvarchar(450) NOT NULL,
    [FirstName] nvarchar(max) NOT NULL,
    [LastName] nvarchar(max) NOT NULL,
    [PasswordHash] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    [IsEmailVerified] bit NOT NULL,
    [IsMobileVerified] bit NOT NULL,
    [LastLoginAt] datetime2 NULL,
    [ProfileImageUrl] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Classes] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NOT NULL,
    [Section] nvarchar(max) NULL,
    [Medium] nvarchar(max) NOT NULL,
    [Board] nvarchar(max) NOT NULL,
    [AcademicYear] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NULL,
    [MaxStudents] int NULL,
    [StartTime] time NULL,
    [EndTime] time NULL,
    [DaysOfWeek] nvarchar(max) NULL,
    [MonthlyFee] decimal(18,2) NULL,
    [IsActive] bit NOT NULL,
    [SessionId] int NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_Classes] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Classes_Sessions_SessionId] FOREIGN KEY ([SessionId]) REFERENCES [Sessions] ([Id])
);
GO


CREATE TABLE [OtpRecords] (
    [Id] int NOT NULL IDENTITY,
    [Identifier] nvarchar(max) NOT NULL,
    [OtpCode] nvarchar(max) NOT NULL,
    [Type] int NOT NULL,
    [Purpose] int NOT NULL,
    [ExpiresAt] datetime2 NOT NULL,
    [IsUsed] bit NOT NULL,
    [AttemptCount] int NOT NULL,
    [UserId] int NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_OtpRecords] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_OtpRecords_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id])
);
GO


CREATE TABLE [RefreshTokens] (
    [Id] int NOT NULL IDENTITY,
    [Token] nvarchar(max) NOT NULL,
    [ExpiresAt] datetime2 NOT NULL,
    [IsRevoked] bit NOT NULL,
    [RevokedAt] datetime2 NULL,
    [ReplacedByToken] nvarchar(max) NULL,
    [ReasonRevoked] nvarchar(max) NULL,
    [CreatedByIp] nvarchar(max) NOT NULL,
    [RevokedByIp] nvarchar(max) NULL,
    [UserId] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_RefreshTokens] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RefreshTokens_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [Students] (
    [Id] int NOT NULL IDENTITY,
    [FirstName] nvarchar(max) NOT NULL,
    [LastName] nvarchar(max) NOT NULL,
    [DateOfBirth] datetime2 NOT NULL,
    [Gender] int NOT NULL,
    [Address] nvarchar(max) NULL,
    [City] nvarchar(max) NULL,
    [State] nvarchar(max) NULL,
    [PinCode] nvarchar(max) NULL,
    [StudentMobile] nvarchar(max) NULL,
    [StudentEmail] nvarchar(max) NULL,
    [ProfileImageUrl] nvarchar(max) NULL,
    [AdmissionNumber] nvarchar(max) NULL,
    [AdmissionDate] datetime2 NOT NULL,
    [IsActive] bit NOT NULL,
    [ParentName] nvarchar(max) NOT NULL,
    [ParentMobile] nvarchar(max) NOT NULL,
    [ParentEmail] nvarchar(max) NULL,
    [ParentOccupation] nvarchar(max) NULL,
    [ParentUserId] int NULL,
    [SessionId] int NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_Students] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Students_Sessions_SessionId] FOREIGN KEY ([SessionId]) REFERENCES [Sessions] ([Id]),
    CONSTRAINT [FK_Students_Users_ParentUserId] FOREIGN KEY ([ParentUserId]) REFERENCES [Users] ([Id])
);
GO


CREATE TABLE [Teachers] (
    [Id] int NOT NULL IDENTITY,
    [FirstName] nvarchar(max) NOT NULL,
    [LastName] nvarchar(max) NOT NULL,
    [Email] nvarchar(max) NOT NULL,
    [Mobile] nvarchar(max) NOT NULL,
    [DateOfBirth] datetime2 NULL,
    [Gender] int NOT NULL,
    [Address] nvarchar(max) NULL,
    [City] nvarchar(max) NULL,
    [State] nvarchar(max) NULL,
    [PinCode] nvarchar(max) NULL,
    [Qualification] nvarchar(max) NULL,
    [Specialization] nvarchar(max) NULL,
    [ExperienceYears] int NULL,
    [JoiningDate] datetime2 NOT NULL,
    [LeavingDate] datetime2 NULL,
    [MonthlySalary] decimal(18,2) NULL,
    [ProfileImageUrl] nvarchar(max) NULL,
    [EmployeeId] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    [UserId] int NULL,
    [SessionId] int NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_Teachers] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Teachers_Sessions_SessionId] FOREIGN KEY ([SessionId]) REFERENCES [Sessions] ([Id]),
    CONSTRAINT [FK_Teachers_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id])
);
GO


CREATE TABLE [UserDevices] (
    [Id] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [DeviceId] nvarchar(200) NOT NULL,
    [DeviceName] nvarchar(100) NOT NULL,
    [DeviceType] nvarchar(50) NOT NULL,
    [BrowserInfo] nvarchar(100) NOT NULL,
    [IpAddress] nvarchar(45) NOT NULL,
    [Location] nvarchar(100) NOT NULL,
    [IsTrusted] bit NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [LastUsedAt] datetime2 NULL,
    [ExpiresAt] datetime2 NULL,
    CONSTRAINT [PK_UserDevices] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_UserDevices_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [UserRoles] (
    [UserId] int NOT NULL,
    [RoleId] int NOT NULL,
    [AssignedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_UserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_UserRoles_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_UserRoles_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [ClassSubjects] (
    [ClassId] int NOT NULL,
    [SubjectId] int NOT NULL,
    [TeacherAssigned] nvarchar(max) NULL,
    [MaxStudents] int NULL,
    [IsActive] bit NOT NULL,
    [Id] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_ClassSubjects] PRIMARY KEY ([ClassId], [SubjectId]),
    CONSTRAINT [FK_ClassSubjects_Classes_ClassId] FOREIGN KEY ([ClassId]) REFERENCES [Classes] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ClassSubjects_Subjects_SubjectId] FOREIGN KEY ([SubjectId]) REFERENCES [Subjects] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [FeeStructures] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NOT NULL,
    [Type] int NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [Frequency] int NOT NULL,
    [ClassId] int NULL,
    [Description] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    [AcademicYear] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_FeeStructures] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_FeeStructures_Classes_ClassId] FOREIGN KEY ([ClassId]) REFERENCES [Classes] ([Id])
);
GO


CREATE TABLE [Attendances] (
    [Id] int NOT NULL IDENTITY,
    [StudentId] int NOT NULL,
    [ClassId] int NOT NULL,
    [Date] date NOT NULL,
    [Status] int NOT NULL,
    [CheckInTime] time NULL,
    [CheckOutTime] time NULL,
    [Remarks] nvarchar(max) NULL,
    [MarkedByUserId] int NULL,
    [MarkedAt] datetime2 NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_Attendances] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Attendances_Classes_ClassId] FOREIGN KEY ([ClassId]) REFERENCES [Classes] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Attendances_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [Students] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Attendances_Users_MarkedByUserId] FOREIGN KEY ([MarkedByUserId]) REFERENCES [Users] ([Id])
);
GO


CREATE TABLE [Leads] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NOT NULL,
    [Mobile] nvarchar(max) NOT NULL,
    [Email] nvarchar(max) NULL,
    [ParentName] nvarchar(max) NULL,
    [ParentMobile] nvarchar(max) NULL,
    [Address] nvarchar(max) NULL,
    [City] nvarchar(max) NULL,
    [Source] int NOT NULL,
    [SourceDetails] nvarchar(max) NULL,
    [InterestedClass] nvarchar(max) NULL,
    [InterestedSubject] nvarchar(max) NULL,
    [Status] int NOT NULL,
    [Priority] int NOT NULL,
    [NextFollowupDate] datetime2 NULL,
    [Remarks] nvarchar(max) NULL,
    [AssignedToUserId] int NULL,
    [ConvertedStudentId] int NULL,
    [ConvertedAt] datetime2 NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_Leads] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Leads_Students_ConvertedStudentId] FOREIGN KEY ([ConvertedStudentId]) REFERENCES [Students] ([Id]),
    CONSTRAINT [FK_Leads_Users_AssignedToUserId] FOREIGN KEY ([AssignedToUserId]) REFERENCES [Users] ([Id])
);
GO


CREATE TABLE [StudentClasses] (
    [StudentId] int NOT NULL,
    [ClassId] int NOT NULL,
    [EnrollmentDate] datetime2 NOT NULL,
    [EndDate] datetime2 NULL,
    [IsActive] bit NOT NULL,
    [Remarks] nvarchar(max) NULL,
    CONSTRAINT [PK_StudentClasses] PRIMARY KEY ([StudentId], [ClassId]),
    CONSTRAINT [FK_StudentClasses_Classes_ClassId] FOREIGN KEY ([ClassId]) REFERENCES [Classes] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_StudentClasses_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [Students] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [StudentPerformances] (
    [Id] int NOT NULL IDENTITY,
    [StudentId] int NOT NULL,
    [ClassId] int NULL,
    [Subject] nvarchar(max) NULL,
    [TestName] nvarchar(max) NOT NULL,
    [Type] int NOT NULL,
    [Score] decimal(18,2) NOT NULL,
    [MaxScore] decimal(18,2) NOT NULL,
    [Percentage] decimal(18,2) NULL,
    [Grade] nvarchar(max) NULL,
    [Rank] int NULL,
    [TestDate] date NOT NULL,
    [Remarks] nvarchar(max) NULL,
    [EvaluatedByTeacherId] int NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_StudentPerformances] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_StudentPerformances_Classes_ClassId] FOREIGN KEY ([ClassId]) REFERENCES [Classes] ([Id]),
    CONSTRAINT [FK_StudentPerformances_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [Students] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_StudentPerformances_Teachers_EvaluatedByTeacherId] FOREIGN KEY ([EvaluatedByTeacherId]) REFERENCES [Teachers] ([Id])
);
GO


CREATE TABLE [StudentRemarks] (
    [Id] int NOT NULL IDENTITY,
    [StudentId] int NOT NULL,
    [TeacherId] int NOT NULL,
    [ClassId] int NULL,
    [Subject] nvarchar(max) NULL,
    [ChapterName] nvarchar(max) NULL,
    [TopicName] nvarchar(max) NULL,
    [Remarks] nvarchar(max) NOT NULL,
    [Type] int NOT NULL,
    [Rating] int NULL,
    [RemarkDate] datetime2 NOT NULL,
    [IsVisibleToParent] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_StudentRemarks] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_StudentRemarks_Classes_ClassId] FOREIGN KEY ([ClassId]) REFERENCES [Classes] ([Id]),
    CONSTRAINT [FK_StudentRemarks_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [Students] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_StudentRemarks_Teachers_TeacherId] FOREIGN KEY ([TeacherId]) REFERENCES [Teachers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [TeacherAttendances] (
    [Id] int NOT NULL IDENTITY,
    [TeacherId] int NOT NULL,
    [Date] date NOT NULL,
    [Status] int NOT NULL,
    [CheckInTime] time NULL,
    [CheckOutTime] time NULL,
    [Remarks] nvarchar(max) NULL,
    [MarkedByUserId] int NULL,
    [MarkedAt] datetime2 NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_TeacherAttendances] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_TeacherAttendances_Teachers_TeacherId] FOREIGN KEY ([TeacherId]) REFERENCES [Teachers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_TeacherAttendances_Users_MarkedByUserId] FOREIGN KEY ([MarkedByUserId]) REFERENCES [Users] ([Id])
);
GO


CREATE TABLE [TeacherClasses] (
    [TeacherId] int NOT NULL,
    [ClassId] int NOT NULL,
    [AssignedDate] datetime2 NOT NULL,
    [EndDate] datetime2 NULL,
    [IsActive] bit NOT NULL,
    [IsPrimary] bit NOT NULL,
    CONSTRAINT [PK_TeacherClasses] PRIMARY KEY ([TeacherId], [ClassId]),
    CONSTRAINT [FK_TeacherClasses_Classes_ClassId] FOREIGN KEY ([ClassId]) REFERENCES [Classes] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_TeacherClasses_Teachers_TeacherId] FOREIGN KEY ([TeacherId]) REFERENCES [Teachers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [TeacherSalaries] (
    [Id] int NOT NULL IDENTITY,
    [TeacherId] int NOT NULL,
    [BasicSalary] decimal(18,2) NOT NULL,
    [Allowances] decimal(18,2) NULL,
    [Deductions] decimal(18,2) NULL,
    [NetSalary] decimal(18,2) NOT NULL,
    [Month] nvarchar(max) NOT NULL,
    [Year] int NOT NULL,
    [PaymentDate] datetime2 NULL,
    [PaymentMethod] int NULL,
    [TransactionId] nvarchar(max) NULL,
    [Status] int NOT NULL,
    [Remarks] nvarchar(max) NULL,
    [ProcessedByUserId] int NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_TeacherSalaries] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_TeacherSalaries_Teachers_TeacherId] FOREIGN KEY ([TeacherId]) REFERENCES [Teachers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_TeacherSalaries_Users_ProcessedByUserId] FOREIGN KEY ([ProcessedByUserId]) REFERENCES [Users] ([Id])
);
GO


CREATE TABLE [UserSessions] (
    [Id] int NOT NULL IDENTITY,
    [UserDeviceId] int NOT NULL,
    [Token] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [ExpiresAt] datetime2 NOT NULL,
    [IsActive] bit NOT NULL,
    [LastActivityAt] datetime2 NULL,
    CONSTRAINT [PK_UserSessions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_UserSessions_UserDevices_UserDeviceId] FOREIGN KEY ([UserDeviceId]) REFERENCES [UserDevices] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [StudentFees] (
    [Id] int NOT NULL IDENTITY,
    [StudentId] int NOT NULL,
    [FeeStructureId] int NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [DiscountAmount] decimal(18,2) NULL,
    [DiscountReason] nvarchar(max) NULL,
    [FinalAmount] decimal(18,2) NOT NULL,
    [PaidAmount] decimal(18,2) NOT NULL,
    [DueDate] date NOT NULL,
    [Status] int NOT NULL,
    [Remarks] nvarchar(max) NULL,
    [Month] nvarchar(max) NULL,
    [AcademicYear] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_StudentFees] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_StudentFees_FeeStructures_FeeStructureId] FOREIGN KEY ([FeeStructureId]) REFERENCES [FeeStructures] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_StudentFees_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [Students] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [LeadFollowups] (
    [Id] int NOT NULL IDENTITY,
    [LeadId] int NOT NULL,
    [FollowupDate] datetime2 NOT NULL,
    [Type] int NOT NULL,
    [Notes] nvarchar(max) NOT NULL,
    [StatusAfter] int NULL,
    [NextFollowupDate] datetime2 NULL,
    [NextFollowupNotes] nvarchar(max) NULL,
    [FollowedByUserId] int NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_LeadFollowups] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_LeadFollowups_Leads_LeadId] FOREIGN KEY ([LeadId]) REFERENCES [Leads] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_LeadFollowups_Users_FollowedByUserId] FOREIGN KEY ([FollowedByUserId]) REFERENCES [Users] ([Id])
);
GO


CREATE TABLE [Payments] (
    [Id] int NOT NULL IDENTITY,
    [StudentFeeId] int NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [PaymentDate] datetime2 NOT NULL,
    [Method] int NOT NULL,
    [TransactionId] nvarchar(max) NULL,
    [ReceiptNumber] nvarchar(max) NULL,
    [Remarks] nvarchar(max) NULL,
    [Status] int NOT NULL,
    [ReceivedByUserId] int NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_Payments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Payments_StudentFees_StudentFeeId] FOREIGN KEY ([StudentFeeId]) REFERENCES [StudentFees] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Payments_Users_ReceivedByUserId] FOREIGN KEY ([ReceivedByUserId]) REFERENCES [Users] ([Id])
);
GO


IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Description', N'Name') AND [object_id] = OBJECT_ID(N'[Roles]'))
    SET IDENTITY_INSERT [Roles] ON;
INSERT INTO [Roles] ([Id], [Description], [Name])
VALUES (1, N'Administrator with full access', N'Admin'),
(2, N'Teacher with limited access', N'Teacher'),
(3, N'Parent with view-only access', N'Parent');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Description', N'Name') AND [object_id] = OBJECT_ID(N'[Roles]'))
    SET IDENTITY_INSERT [Roles] OFF;
GO


CREATE INDEX [IX_Attendances_ClassId] ON [Attendances] ([ClassId]);
GO


CREATE INDEX [IX_Attendances_MarkedByUserId] ON [Attendances] ([MarkedByUserId]);
GO


CREATE INDEX [IX_Attendances_StudentId] ON [Attendances] ([StudentId]);
GO


CREATE INDEX [IX_Classes_SessionId] ON [Classes] ([SessionId]);
GO


CREATE INDEX [IX_ClassSubjects_SubjectId] ON [ClassSubjects] ([SubjectId]);
GO


CREATE INDEX [IX_FeeStructures_ClassId] ON [FeeStructures] ([ClassId]);
GO


CREATE INDEX [IX_LeadFollowups_FollowedByUserId] ON [LeadFollowups] ([FollowedByUserId]);
GO


CREATE INDEX [IX_LeadFollowups_LeadId] ON [LeadFollowups] ([LeadId]);
GO


CREATE INDEX [IX_Leads_AssignedToUserId] ON [Leads] ([AssignedToUserId]);
GO


CREATE INDEX [IX_Leads_ConvertedStudentId] ON [Leads] ([ConvertedStudentId]);
GO


CREATE INDEX [IX_OtpRecords_UserId] ON [OtpRecords] ([UserId]);
GO


CREATE INDEX [IX_Payments_ReceivedByUserId] ON [Payments] ([ReceivedByUserId]);
GO


CREATE INDEX [IX_Payments_StudentFeeId] ON [Payments] ([StudentFeeId]);
GO


CREATE INDEX [IX_RefreshTokens_UserId] ON [RefreshTokens] ([UserId]);
GO


CREATE INDEX [IX_Sessions_IsActive] ON [Sessions] ([IsActive]);
GO


CREATE UNIQUE INDEX [IX_Sessions_Name] ON [Sessions] ([Name]);
GO


CREATE INDEX [IX_StudentClasses_ClassId] ON [StudentClasses] ([ClassId]);
GO


CREATE INDEX [IX_StudentFees_FeeStructureId] ON [StudentFees] ([FeeStructureId]);
GO


CREATE INDEX [IX_StudentFees_StudentId] ON [StudentFees] ([StudentId]);
GO


CREATE INDEX [IX_StudentPerformances_ClassId] ON [StudentPerformances] ([ClassId]);
GO


CREATE INDEX [IX_StudentPerformances_EvaluatedByTeacherId] ON [StudentPerformances] ([EvaluatedByTeacherId]);
GO


CREATE INDEX [IX_StudentPerformances_StudentId] ON [StudentPerformances] ([StudentId]);
GO


CREATE INDEX [IX_StudentRemarks_ClassId] ON [StudentRemarks] ([ClassId]);
GO


CREATE INDEX [IX_StudentRemarks_StudentId] ON [StudentRemarks] ([StudentId]);
GO


CREATE INDEX [IX_StudentRemarks_TeacherId] ON [StudentRemarks] ([TeacherId]);
GO


CREATE INDEX [IX_Students_ParentUserId] ON [Students] ([ParentUserId]);
GO


CREATE INDEX [IX_Students_SessionId] ON [Students] ([SessionId]);
GO


CREATE UNIQUE INDEX [IX_Subjects_Name] ON [Subjects] ([Name]);
GO


CREATE INDEX [IX_TeacherAttendances_MarkedByUserId] ON [TeacherAttendances] ([MarkedByUserId]);
GO


CREATE INDEX [IX_TeacherAttendances_TeacherId] ON [TeacherAttendances] ([TeacherId]);
GO


CREATE INDEX [IX_TeacherClasses_ClassId] ON [TeacherClasses] ([ClassId]);
GO


CREATE INDEX [IX_Teachers_SessionId] ON [Teachers] ([SessionId]);
GO


CREATE INDEX [IX_Teachers_UserId] ON [Teachers] ([UserId]);
GO


CREATE INDEX [IX_TeacherSalaries_ProcessedByUserId] ON [TeacherSalaries] ([ProcessedByUserId]);
GO


CREATE INDEX [IX_TeacherSalaries_TeacherId] ON [TeacherSalaries] ([TeacherId]);
GO


CREATE INDEX [IX_UserDevices_UserId] ON [UserDevices] ([UserId]);
GO


CREATE INDEX [IX_UserRoles_RoleId] ON [UserRoles] ([RoleId]);
GO


CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]);
GO


CREATE UNIQUE INDEX [IX_Users_Mobile] ON [Users] ([Mobile]);
GO


CREATE INDEX [IX_UserSessions_UserDeviceId] ON [UserSessions] ([UserDeviceId]);
GO


