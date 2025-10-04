# **Section 9: Database Schema**

Let me define the concrete database schema based on the data models, using SQL DDL that works with both PostgreSQL and SQL Server (EF Core will handle minor dialect differences).

---

## **Database Schema Overview**

The database follows a **normalized relational design** with the following characteristics:

- **Primary Keys**: GUIDs for most entities (better for distributed systems, harder to enumerate)
- **Foreign Keys**: Enforced referential integrity
- **Indexes**: Strategic indexes on foreign keys and frequently queried fields
- **Audit Columns**: CreatedDate, UpdatedDate on all tables
- **Soft Deletes**: IsActive, IsArchived flags instead of physical deletes
- **Row-Level Security**: Entity-based data isolation via query filters

---

## **Core Tables**

```sql
-- ============================================
-- Users Table
-- ============================================
CREATE TABLE Users (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(256) NOT NULL UNIQUE,
    Phone NVARCHAR(20) NOT NULL,
    PeselEncrypted NVARCHAR(500) NOT NULL, -- Encrypted, searchable by last 4 digits
    PeselLast4 NVARCHAR(4) NOT NULL, -- For display purposes
    PasswordHash NVARCHAR(500) NOT NULL,
    UserType NVARCHAR(20) NOT NULL CHECK (UserType IN ('Internal', 'External')),
    IsActive BIT NOT NULL DEFAULT 1,
    MustChangePassword BIT NOT NULL DEFAULT 0,
    LastLoginDate DATETIME2 NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    INDEX IX_Users_Email (Email),
    INDEX IX_Users_UserType (UserType),
    INDEX IX_Users_IsActive (IsActive)
);

-- ============================================
-- Entities Table (Supervised Financial Entities)
-- ============================================
CREATE TABLE Entities (
    Id BIGINT PRIMARY KEY IDENTITY(1,1),
    UknfCode NVARCHAR(250) NOT NULL UNIQUE,
    EntityType NVARCHAR(250) NOT NULL,
    Name NVARCHAR(500) NOT NULL,
    Lei NVARCHAR(20) NULL,
    Nip NVARCHAR(10) NULL,
    Krs NVARCHAR(10) NULL,
    Street NVARCHAR(250) NULL,
    BuildingNumber NVARCHAR(250) NULL,
    PremisesNumber NVARCHAR(250) NULL,
    PostalCode NVARCHAR(250) NULL,
    City NVARCHAR(250) NULL,
    Phone NVARCHAR(250) NULL,
    Email NVARCHAR(500) NULL,
    UknfRegistrationNumber NVARCHAR(100) NULL,
    EntityStatus NVARCHAR(250) NOT NULL,
    Category NVARCHAR(500) NULL,
    Sector NVARCHAR(500) NULL,
    SubSector NVARCHAR(500) NULL,
    IsCrossBorder BIT NOT NULL DEFAULT 0,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    INDEX IX_Entities_UknfCode (UknfCode),
    INDEX IX_Entities_EntityType (EntityType),
    INDEX IX_Entities_EntityStatus (EntityStatus),
    INDEX IX_Entities_Name (Name)
);

-- ============================================
-- Entity History (Versioning)
-- ============================================
CREATE TABLE EntityHistory (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    EntityId BIGINT NOT NULL,
    ChangeType NVARCHAR(50) NOT NULL, -- Created, Updated, DataChangeRequested
    ChangedByUserId UUID NOT NULL,
    PreviousValues NVARCHAR(MAX) NULL, -- JSON snapshot of previous state
    NewValues NVARCHAR(MAX) NULL, -- JSON snapshot of new state
    ChangeDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    FOREIGN KEY (EntityId) REFERENCES Entities(Id),
    FOREIGN KEY (ChangedByUserId) REFERENCES Users(Id),
    INDEX IX_EntityHistory_EntityId (EntityId),
    INDEX IX_EntityHistory_ChangeDate (ChangeDate)
);

-- ============================================
-- User-Entity Mapping (Many-to-Many)
-- ============================================
CREATE TABLE UserEntities (
    UserId UUID NOT NULL,
    EntityId BIGINT NOT NULL,
    AssignedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    PRIMARY KEY (UserId, EntityId),
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (EntityId) REFERENCES Entities(Id) ON DELETE CASCADE
);

-- ============================================
-- Roles
-- ============================================
CREATE TABLE Roles (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    Name NVARCHAR(100) NOT NULL UNIQUE,
    Description NVARCHAR(500) NULL,
    IsSystemRole BIT NOT NULL DEFAULT 0, -- System roles can't be deleted
    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

-- ============================================
-- Permissions
-- ============================================
CREATE TABLE Permissions (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    Name NVARCHAR(100) NOT NULL UNIQUE,
    Description NVARCHAR(500) NULL,
    Module NVARCHAR(50) NOT NULL, -- Communication, Auth, Admin
    Resource NVARCHAR(100) NOT NULL, -- Reports, Messages, Cases, etc.
    Action NVARCHAR(50) NOT NULL -- Create, Read, Update, Delete, Approve, etc.
);

-- ============================================
-- Role-Permission Mapping
-- ============================================
CREATE TABLE RolePermissions (
    RoleId UUID NOT NULL,
    PermissionId UUID NOT NULL,
    
    PRIMARY KEY (RoleId, PermissionId),
    FOREIGN KEY (RoleId) REFERENCES Roles(Id) ON DELETE CASCADE,
    FOREIGN KEY (PermissionId) REFERENCES Permissions(Id) ON DELETE CASCADE
);

-- ============================================
-- User-Role Mapping
-- ============================================
CREATE TABLE UserRoles (
    UserId UUID NOT NULL,
    RoleId UUID NOT NULL,
    AssignedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    AssignedByUserId UUID NULL,
    
    PRIMARY KEY (UserId, RoleId),
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (RoleId) REFERENCES Roles(Id) ON DELETE CASCADE,
    FOREIGN KEY (AssignedByUserId) REFERENCES Users(Id)
);

-- ============================================
-- Access Requests
-- ============================================
CREATE TABLE AccessRequests (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    UserId UUID NOT NULL,
    Status NVARCHAR(20) NOT NULL CHECK (Status IN ('Working', 'New', 'Accepted', 'Blocked', 'Updated')),
    SubmittedDate DATETIME2 NULL,
    ReviewedByUserId UUID NULL,
    ReviewedDate DATETIME2 NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    FOREIGN KEY (ReviewedByUserId) REFERENCES Users(Id),
    INDEX IX_AccessRequests_UserId (UserId),
    INDEX IX_AccessRequests_Status (Status)
);

-- ============================================
-- Permission Lines (Entity-specific permissions in Access Request)
-- ============================================
CREATE TABLE PermissionLines (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    AccessRequestId UUID NOT NULL,
    EntityId BIGINT NOT NULL,
    HasReportingAccess BIT NOT NULL DEFAULT 0,
    HasCasesAccess BIT NOT NULL DEFAULT 0,
    IsEntityAdministrator BIT NOT NULL DEFAULT 0,
    IsBlocked BIT NOT NULL DEFAULT 0,
    EntityEmailForNotifications NVARCHAR(500) NULL,
    GrantedByUserId UUID NULL,
    GrantedDate DATETIME2 NULL,
    
    FOREIGN KEY (AccessRequestId) REFERENCES AccessRequests(Id) ON DELETE CASCADE,
    FOREIGN KEY (EntityId) REFERENCES Entities(Id),
    FOREIGN KEY (GrantedByUserId) REFERENCES Users(Id),
    INDEX IX_PermissionLines_AccessRequestId (AccessRequestId),
    INDEX IX_PermissionLines_EntityId (EntityId)
);

-- ============================================
-- Reports
-- ============================================
CREATE TABLE Reports (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    EntityId BIGINT NOT NULL,
    UserId UUID NOT NULL,
    FileName NVARCHAR(500) NOT NULL,
    FileStorageKey NVARCHAR(500) NOT NULL,
    FileSize BIGINT NOT NULL,
    ReportType NVARCHAR(250) NOT NULL,
    ReportingPeriod NVARCHAR(100) NOT NULL,
    ValidationStatus NVARCHAR(50) NOT NULL CHECK (ValidationStatus IN (
        'Working', 'Transmitted', 'Ongoing', 'Successful', 
        'ValidationErrors', 'TechnicalError', 'TimeoutError', 'ContestedByUKNF'
    )),
    ValidationResultFileKey NVARCHAR(500) NULL,
    UniqueValidationId NVARCHAR(100) NULL,
    IsArchived BIT NOT NULL DEFAULT 0,
    IsCorrectionOfReportId UUID NULL,
    SubmittedDate DATETIME2 NOT NULL,
    ValidationStartedDate DATETIME2 NULL,
    ValidationCompletedDate DATETIME2 NULL,
    ErrorDescription NVARCHAR(MAX) NULL,
    ContestedDescription NVARCHAR(MAX) NULL,
    ContestedByUserId UUID NULL,
    ContestedDate DATETIME2 NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    FOREIGN KEY (EntityId) REFERENCES Entities(Id),
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    FOREIGN KEY (IsCorrectionOfReportId) REFERENCES Reports(Id),
    FOREIGN KEY (ContestedByUserId) REFERENCES Users(Id),
    INDEX IX_Reports_EntityId (EntityId),
    INDEX IX_Reports_ValidationStatus (ValidationStatus),
    INDEX IX_Reports_ReportingPeriod (ReportingPeriod),
    INDEX IX_Reports_SubmittedDate (SubmittedDate),
    INDEX IX_Reports_IsArchived (IsArchived)
);

-- ============================================
-- Messages
-- ============================================
CREATE TABLE Messages (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    Subject NVARCHAR(500) NOT NULL,
    Body NVARCHAR(MAX) NOT NULL,
    SenderId UUID NOT NULL,
    MessageStatus NVARCHAR(50) NOT NULL CHECK (MessageStatus IN (
        'AwaitingUknfResponse', 'AwaitingUserResponse', 'Closed'
    )),
    ContextType NVARCHAR(50) NULL CHECK (ContextType IN (
        'AccessRequest', 'Case', 'Report', 'Standalone'
    )),
    ContextId UUID NULL,
    EntityId BIGINT NULL,
    ParentMessageId UUID NULL,
    SentDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    FOREIGN KEY (SenderId) REFERENCES Users(Id),
    FOREIGN KEY (EntityId) REFERENCES Entities(Id),
    FOREIGN KEY (ParentMessageId) REFERENCES Messages(Id),
    INDEX IX_Messages_SenderId (SenderId),
    INDEX IX_Messages_ContextType_ContextId (ContextType, ContextId),
    INDEX IX_Messages_EntityId (EntityId),
    INDEX IX_Messages_ParentMessageId (ParentMessageId),
    INDEX IX_Messages_SentDate (SentDate)
);

-- ============================================
-- Message Recipients (Many-to-Many)
-- ============================================
CREATE TABLE MessageRecipients (
    MessageId UUID NOT NULL,
    RecipientId UUID NOT NULL,
    ReadDate DATETIME2 NULL,
    IsRead BIT NOT NULL DEFAULT 0,
    
    PRIMARY KEY (MessageId, RecipientId),
    FOREIGN KEY (MessageId) REFERENCES Messages(Id) ON DELETE CASCADE,
    FOREIGN KEY (RecipientId) REFERENCES Users(Id),
    INDEX IX_MessageRecipients_RecipientId_IsRead (RecipientId, IsRead)
);

-- ============================================
-- Message Attachments
-- ============================================
CREATE TABLE MessageAttachments (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    MessageId UUID NOT NULL,
    FileName NVARCHAR(500) NOT NULL,
    FileStorageKey NVARCHAR(500) NOT NULL,
    FileSize BIGINT NOT NULL,
    ContentType NVARCHAR(200) NOT NULL,
    UploadedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    FOREIGN KEY (MessageId) REFERENCES Messages(Id) ON DELETE CASCADE,
    INDEX IX_MessageAttachments_MessageId (MessageId)
);

-- ============================================
-- Cases
-- ============================================
CREATE TABLE Cases (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    CaseNumber NVARCHAR(100) NOT NULL UNIQUE,
    Title NVARCHAR(500) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    EntityId BIGINT NOT NULL,
    CreatedByUserId UUID NOT NULL,
    AssignedToUserId UUID NULL,
    Category NVARCHAR(50) NOT NULL CHECK (Category IN (
        'RegistrationDataChange', 'StaffChange', 'EntityCall', 
        'SystemEntitlements', 'Reporting', 'Other'
    )),
    Priority NVARCHAR(20) NOT NULL CHECK (Priority IN ('Low', 'Medium', 'High')),
    Status NVARCHAR(20) NOT NULL CHECK (Status IN (
        'Draft', 'New', 'Ongoing', 'ToBeCompleted', 'Cancelled', 'Completed'
    )),
    CancellationReason NVARCHAR(MAX) NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CompletedDate DATETIME2 NULL,
    
    FOREIGN KEY (EntityId) REFERENCES Entities(Id),
    FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id),
    FOREIGN KEY (AssignedToUserId) REFERENCES Users(Id),
    INDEX IX_Cases_EntityId (EntityId),
    INDEX IX_Cases_Status (Status),
    INDEX IX_Cases_Category (Category),
    INDEX IX_Cases_CaseNumber (CaseNumber),
    INDEX IX_Cases_CreatedDate (CreatedDate)
);

-- ============================================
-- Case Attachments
-- ============================================
CREATE TABLE CaseAttachments (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    CaseId UUID NOT NULL,
    FileName NVARCHAR(500) NOT NULL,
    FileStorageKey NVARCHAR(500) NOT NULL,
    FileSize BIGINT NOT NULL,
    ContentType NVARCHAR(200) NOT NULL,
    UploadedByUserId UUID NOT NULL,
    UploadedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    FOREIGN KEY (CaseId) REFERENCES Cases(Id) ON DELETE CASCADE,
    FOREIGN KEY (UploadedByUserId) REFERENCES Users(Id),
    INDEX IX_CaseAttachments_CaseId (CaseId)
);

-- ============================================
-- Case History (Status changes and modifications)
-- ============================================
CREATE TABLE CaseHistory (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    CaseId UUID NOT NULL,
    ChangedByUserId UUID NOT NULL,
    ChangeType NVARCHAR(50) NOT NULL, -- StatusChange, Update, Comment
    PreviousStatus NVARCHAR(20) NULL,
    NewStatus NVARCHAR(20) NULL,
    Notes NVARCHAR(MAX) NULL,
    ChangeDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    FOREIGN KEY (CaseId) REFERENCES Cases(Id) ON DELETE CASCADE,
    FOREIGN KEY (ChangedByUserId) REFERENCES Users(Id),
    INDEX IX_CaseHistory_CaseId (CaseId),
    INDEX IX_CaseHistory_ChangeDate (ChangeDate)
);

-- ============================================
-- Library Files
-- ============================================
CREATE TABLE LibraryFiles (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    FileName NVARCHAR(500) NOT NULL,
    FileStorageKey NVARCHAR(500) NOT NULL,
    FileSize BIGINT NOT NULL,
    Description NVARCHAR(MAX) NULL,
    ReportingPeriod NVARCHAR(100) NULL,
    Category NVARCHAR(250) NULL,
    Version NVARCHAR(50) NOT NULL,
    IsCurrentVersion BIT NOT NULL DEFAULT 1,
    IsArchived BIT NOT NULL DEFAULT 0,
    UploadedByUserId UUID NOT NULL,
    UploadedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    LastUpdatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    FOREIGN KEY (UploadedByUserId) REFERENCES Users(Id),
    INDEX IX_LibraryFiles_Category (Category),
    INDEX IX_LibraryFiles_IsCurrentVersion (IsCurrentVersion),
    INDEX IX_LibraryFiles_IsArchived (IsArchived)
);

-- ============================================
-- Library File History (Versioning)
-- ============================================
CREATE TABLE LibraryFileHistory (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    LibraryFileId UUID NOT NULL,
    PreviousFileStorageKey NVARCHAR(500) NOT NULL,
    PreviousVersion NVARCHAR(50) NOT NULL,
    ChangedByUserId UUID NOT NULL,
    ChangeDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ChangeNotes NVARCHAR(MAX) NULL,
    
    FOREIGN KEY (LibraryFileId) REFERENCES LibraryFiles(Id) ON DELETE CASCADE,
    FOREIGN KEY (ChangedByUserId) REFERENCES Users(Id),
    INDEX IX_LibraryFileHistory_LibraryFileId (LibraryFileId)
);

-- ============================================
-- Library File Access (Who can access which files)
-- ============================================
CREATE TABLE LibraryFileAccess (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    LibraryFileId UUID NOT NULL,
    AccessType NVARCHAR(20) NOT NULL CHECK (AccessType IN ('User', 'Entity', 'ContactGroup', 'All')),
    TargetId NVARCHAR(100) NULL, -- UserId, EntityId, or ContactGroupId depending on AccessType
    GrantedByUserId UUID NOT NULL,
    GrantedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    FOREIGN KEY (LibraryFileId) REFERENCES LibraryFiles(Id) ON DELETE CASCADE,
    FOREIGN KEY (GrantedByUserId) REFERENCES Users(Id),
    INDEX IX_LibraryFileAccess_LibraryFileId (LibraryFileId),
    INDEX IX_LibraryFileAccess_AccessType_TargetId (AccessType, TargetId)
);

-- ============================================
-- FAQ
-- ============================================
CREATE TABLE FAQs (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    Title NVARCHAR(500) NOT NULL,
    QuestionContent NVARCHAR(MAX) NOT NULL,
    AnswerContent NVARCHAR(MAX) NULL,
    Category NVARCHAR(250) NOT NULL,
    Tags NVARCHAR(1000) NULL, -- Comma-separated tags
    Status NVARCHAR(20) NOT NULL CHECK (Status IN ('Submitted', 'Answered', 'Published', 'Archived')),
    AskedByUserId UUID NULL, -- Anonymous but tracked internally
    AnsweredByUserId UUID NULL,
    AverageRating DECIMAL(3,2) NULL,
    ViewCount INT NOT NULL DEFAULT 0,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    AnsweredDate DATETIME2 NULL,
    UpdatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    FOREIGN KEY (AskedByUserId) REFERENCES Users(Id),
    FOREIGN KEY (AnsweredByUserId) REFERENCES Users(Id),
    INDEX IX_FAQs_Category (Category),
    INDEX IX_FAQs_Status (Status),
    INDEX IX_FAQs_CreatedDate (CreatedDate)
);

-- ============================================
-- FAQ Ratings
-- ============================================
CREATE TABLE FaqRatings (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    FaqId UUID NOT NULL,
    UserId UUID NOT NULL,
    Rating INT NOT NULL CHECK (Rating >= 1 AND Rating <= 5),
    RatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    FOREIGN KEY (FaqId) REFERENCES FAQs(Id) ON DELETE CASCADE,
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    UNIQUE (FaqId, UserId), -- One rating per user per FAQ
    INDEX IX_FaqRatings_FaqId (FaqId)
);

-- ============================================
-- Bulletin Board Messages
-- ============================================
CREATE TABLE BulletinBoardMessages (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    Title NVARCHAR(500) NOT NULL,
    Content NVARCHAR(MAX) NOT NULL, -- HTML from WYSIWYG
    Category NVARCHAR(250) NOT NULL,
    Priority NVARCHAR(20) NOT NULL CHECK (Priority IN ('Low', 'Medium', 'High')),
    PublishedByUserId UUID NOT NULL,
    PublishedDate DATETIME2 NULL,
    ExpiryDate DATETIME2 NULL,
    RequiresReadConfirmation BIT NOT NULL DEFAULT 0,
    IsPublished BIT NOT NULL DEFAULT 0,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    FOREIGN KEY (PublishedByUserId) REFERENCES Users(Id),
    INDEX IX_BulletinBoard_IsPublished (IsPublished),
    INDEX IX_BulletinBoard_PublishedDate (PublishedDate),
    INDEX IX_BulletinBoard_Category (Category)
);

-- ============================================
-- Bulletin Board Attachments
-- ============================================
CREATE TABLE BulletinBoardAttachments (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    BulletinBoardMessageId UUID NOT NULL,
    FileName NVARCHAR(500) NOT NULL,
    FileStorageKey NVARCHAR(500) NOT NULL,
    FileSize BIGINT NOT NULL,
    ContentType NVARCHAR(200) NOT NULL,
    UploadedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    FOREIGN KEY (BulletinBoardMessageId) REFERENCES BulletinBoardMessages(Id) ON DELETE CASCADE,
    INDEX IX_BulletinBoardAttachments_MessageId (BulletinBoardMessageId)
);

-- ============================================
-- Bulletin Board Recipients (Who should receive the announcement)
-- ============================================
CREATE TABLE BulletinBoardRecipients (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    BulletinBoardMessageId UUID NOT NULL,
    RecipientType NVARCHAR(20) NOT NULL CHECK (RecipientType IN ('User', 'Entity', 'EntityType', 'ContactGroup', 'All')),
    TargetId NVARCHAR(100) NULL, -- Depends on RecipientType
    
    FOREIGN KEY (BulletinBoardMessageId) REFERENCES BulletinBoardMessages(Id) ON DELETE CASCADE,
    INDEX IX_BulletinBoardRecipients_MessageId (BulletinBoardMessageId)
);

-- ============================================
-- Bulletin Board Read Confirmations
-- ============================================
CREATE TABLE BulletinBoardReadConfirmations (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    BulletinBoardMessageId UUID NOT NULL,
    UserId UUID NOT NULL,
    EntityId BIGINT NOT NULL, -- User was representing this entity when they read it
    ReadDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    FOREIGN KEY (BulletinBoardMessageId) REFERENCES BulletinBoardMessages(Id) ON DELETE CASCADE,
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    FOREIGN KEY (EntityId) REFERENCES Entities(Id),
    UNIQUE (BulletinBoardMessageId, UserId, EntityId),
    INDEX IX_BulletinBoardReadConfirmations_MessageId (BulletinBoardMessageId)
);

-- ============================================
-- Contact Groups
-- ============================================
CREATE TABLE ContactGroups (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    Name NVARCHAR(250) NOT NULL,
    Description NVARCHAR(500) NULL,
    CreatedByUserId UUID NOT NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id),
    INDEX IX_ContactGroups_Name (Name)
);

-- ============================================
-- Contacts (Non-user email recipients)
-- ============================================
CREATE TABLE Contacts (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(256) NOT NULL,
    Phone NVARCHAR(20) NULL,
    EntityId BIGINT NULL,
    CreatedByUserId UUID NOT NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    FOREIGN KEY (EntityId) REFERENCES Entities(Id),
    FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id),
    INDEX IX_Contacts_Email (Email),
    INDEX IX_Contacts_EntityId (EntityId)
);

-- ============================================
-- Contact Group Members
-- ============================================
CREATE TABLE ContactGroupMembers (
    ContactGroupId UUID NOT NULL,
    MemberType NVARCHAR(20) NOT NULL CHECK (MemberType IN ('User', 'Contact')),
    MemberId UUID NOT NULL, -- UserId or ContactId
    AddedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    PRIMARY KEY (ContactGroupId, MemberType, MemberId),
    FOREIGN KEY (ContactGroupId) REFERENCES ContactGroups(Id) ON DELETE CASCADE
);

-- ============================================
-- Audit Log (Comprehensive audit trail)
-- ============================================
CREATE TABLE AuditLogs (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    UserId UUID NULL,
    EntityId BIGINT NULL, -- Entity context if applicable
    EntityType NVARCHAR(100) NOT NULL, -- Report, Message, Case, etc.
    EntityPrimaryKey NVARCHAR(100) NOT NULL,
    Action NVARCHAR(50) NOT NULL, -- Create, Update, Delete, View, etc.
    Timestamp DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    IpAddress NVARCHAR(50) NULL,
    UserAgent NVARCHAR(500) NULL,
    CorrelationId NVARCHAR(100) NULL,
    BeforeState NVARCHAR(MAX) NULL, -- JSON snapshot before change
    AfterState NVARCHAR(MAX) NULL, -- JSON snapshot after change
    
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    FOREIGN KEY (EntityId) REFERENCES Entities(Id),
    INDEX IX_AuditLogs_UserId (UserId),
    INDEX IX_AuditLogs_EntityType_EntityPrimaryKey (EntityType, EntityPrimaryKey),
    INDEX IX_AuditLogs_Timestamp (Timestamp),
    INDEX IX_AuditLogs_CorrelationId (CorrelationId)
);

-- ============================================
-- Password Policy
-- ============================================
CREATE TABLE PasswordPolicy (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    MinLength INT NOT NULL DEFAULT 8,
    RequireUppercase BIT NOT NULL DEFAULT 1,
    RequireLowercase BIT NOT NULL DEFAULT 1,
    RequireDigit BIT NOT NULL DEFAULT 1,
    RequireSpecialChar BIT NOT NULL DEFAULT 1,
    PasswordHistoryDepth INT NOT NULL DEFAULT 5, -- Prevent reuse of last N passwords
    MaxPasswordAge INT NULL, -- Days until password must be changed
    UpdatedByUserId UUID NOT NULL,
    UpdatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    FOREIGN KEY (UpdatedByUserId) REFERENCES Users(Id)
);

-- ============================================
-- Password History (Prevent password reuse)
-- ============================================
CREATE TABLE PasswordHistory (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    UserId UUID NOT NULL,
    PasswordHash NVARCHAR(500) NOT NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    INDEX IX_PasswordHistory_UserId (UserId)
);
```

---

## **Entity Framework Core Configuration Notes**

Since we're using EF Core, here are key configuration points:

1. **Global Query Filters** (Multi-tenancy):
```csharp
// Applied automatically to queries
modelBuilder.Entity<Report>()
    .HasQueryFilter(r => r.EntityId == _currentEntityContext.EntityId);
```

2. **Soft Delete Filter**:
```csharp
modelBuilder.Entity<LibraryFile>()
    .HasQueryFilter(lf => !lf.IsArchived);
```

3. **Computed Columns**:
- `CaseNumber` auto-generated: `CASE-{Year}-{SequentialNumber}`
- Indexes on foreign keys created automatically by EF Core

4. **JSON Columns** (EF Core 7+):
- `BeforeState`, `AfterState` in AuditLogs can use JSON column type for better querying

5. **Concurrency Control**:
- Add `RowVersion` (timestamp) columns for optimistic concurrency on critical tables

---

## **Rationale:**

1. **GUID Primary Keys**: Better for distributed systems, prevents enumeration attacks, easier for import/export

2. **Comprehensive Indexes**: Strategic indexes on foreign keys and frequently queried columns (status fields, dates, entity IDs)

3. **Audit Trail**: Separate AuditLogs table with before/after JSON snapshots for complete change tracking

4. **Versioning**: EntityHistory and LibraryFileHistory tables track changes over time as required by PRD

5. **Multi-tenancy**: EntityId columns throughout enable row-level security via EF Core query filters

6. **Soft Deletes**: IsActive, IsArchived flags preserve data for audit purposes

7. **Flexible Relationships**: Polymorphic patterns (ContextType/ContextId, RecipientType/TargetId) for flexible associations

8. **Performance**: Strategic indexes on commonly queried fields, normalized design to avoid data duplication

9. **Data Integrity**: Foreign keys with appropriate CASCADE rules, CHECK constraints for enums

10. **Hackathon-Ready**: Schema covers all PRD requirements without over-engineering; can be refined later
