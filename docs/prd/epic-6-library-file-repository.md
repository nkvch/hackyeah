# Epic 6: Library & File Repository

**Epic Number:** 6  
**Phase:** Core Communication Features (Preferred Functionalities)  
**Priority:** High (Supports reporting workflow)  
**Status:** Draft

---

## Epic Goal

Implement a centralized file repository (Library) that enables UKNF employees to share documents, report templates, instructions, and other resources with supervised entities through a managed, version-controlled system with flexible access control.

---

## Epic Description

### Context

Supervised entities need access to report templates, instructions, guidelines, and other documents provided by UKNF. The Library serves as a centralized repository where UKNF employees can upload and manage files, and external users can access them based on assigned permissions.

The Library is particularly critical for the reporting workflow (Epic 4), as entities download report templates from the Library, complete them, and upload them back as reports.

[Source: docs/prd/functions-of-the-communication-module.md - "Maintaining the local file repository"]

### What This Epic Delivers

This epic implements comprehensive file repository functionality including:
- **File Management**: Upload, update, and organize files by UKNF Employees
- **Metadata Management**: Capture and maintain file information (reporting period, version, description)
- **Access Control**: Share files with all users, specific entities, or groups
- **Versioning**: Maintain current and archival versions of files
- **Categorization**: Organize files by categories for easy discovery
- **Search and Filter**: Enable users to find files efficiently
- **Change History**: Track all changes to files and metadata
- **Download Tracking**: Monitor which users/entities access which files

### Key Business Rules

1. **UKNF Management**: Only UKNF Employees can add, modify, and delete files and metadata
2. **Shared Resource**: Library is a common resource for all external users
3. **Access Control**: UKNF decides which users/entities have access to each file
4. **Versioning**: Files can have current and archival versions with version dates
5. **Template Distribution**: Report templates are primary use case but library supports any file type
6. **Metadata Required**: Files must have metadata (name, reporting period, update date) for discoverability

[Source: docs/prd/functions-of-the-communication-module.md]

---

## User Stories

### Story 6.1: Upload File to Library (UKNF)

**As a** UKNF Employee  
**I want to** upload files to the Library  
**So that** external users can access them

**Acceptance Criteria:**
- UKNF Employee can access "Add File to Library" functionality
- Employee can upload file (any format commonly needed: XLSX, PDF, DOC/DOCX, ZIP, etc.)
- System validates file size (define maximum, e.g., 100 MB)
- Employee must provide metadata:
  - **Filename** (text, required, can differ from uploaded file name)
  - **Description** (text area, optional)
  - **Category** (dropdown, required) - e.g., "Report Templates", "Instructions", "Guidelines", "Forms"
  - **Reporting Period** (text or dropdown, optional) - e.g., "Q1 2025", "Annual 2024"
  - **Model Update Date** (date picker, required) - date of template/document version
- Employee can add the actual file (Annex/Attachment, required)
- System stores file securely
- Upon successful upload, file appears in Library with "Current" version status
- File is initially not shared (no access for external users until permissions are set)
- System logs file upload with timestamp and employee ID

### Story 6.2: Set File Access Permissions (UKNF)

**As a** UKNF Employee  
**I want to** control which users can access each Library file  
**So that** I can ensure appropriate distribution

**Acceptance Criteria:**
- UKNF Employee can select file and access "Manage Permissions"
- Employee can set access level:
  - **All users**: All external users can view and download
  - **Individual entities**: Select specific entities from list
  - **Entity types/groups**: Select by entity type (e.g., "Loan institutions")
  - **Specific users**: Select individual external users
  - **Contact groups**: Select contact groups (integration with Epic 7)
- Employee can combine multiple access rules (e.g., all entities of type X + specific entities Y, Z)
- Changes take effect immediately
- Affected users can immediately see file in their Library view
- Previously accessible users who lose access can no longer see or download file
- System logs permission changes with timestamp and employee ID

### Story 6.3: View and Search Library (External User)

**As an** External User  
**I want to** browse and search files in the Library  
**So that** I can find and download documents I need

**Acceptance Criteria:**
- User can access "Library" section
- User sees only files they have permission to access (based on permissions set in Story 6.2)
- Library displays files with: Filename, Category, Description (preview), Reporting Period, Model Update Date, Version status (Current/Archival)
- User can filter by: Category, Reporting Period, Version status
- User can search by: Filename, Description
- User can sort by: Filename, Upload date, Model Update Date
- Files are grouped or marked by category for easy navigation
- Current versions are prominently displayed; archival versions can be toggled on/off in view
- User can click file to view details and download
- Library supports pagination for large file counts

### Story 6.4: Download File from Library

**As an** External User or UKNF Employee  
**I want to** download files from the Library  
**So that** I can use them (e.g., complete report templates)

**Acceptance Criteria:**
- User can click download button/link for any accessible file
- System validates user has permission to access file
- File downloads with original filename or metadata filename
- Download is logged with: User ID, File ID, Timestamp, IP address (for audit)
- UKNF Employees can download any file regardless of access permissions
- Download doesn't modify file or metadata
- System tracks download count per file (optional analytics)

### Story 6.5: View File Details

**As an** External User or UKNF Employee  
**I want to** view complete details about a Library file  
**So that** I can understand what it contains and its version

**Acceptance Criteria:**
- User can view file details page including:
  - Filename
  - Full description
  - Category
  - Reporting Period
  - Model Update Date
  - Version status (Current/Archival)
  - File size
  - File format
  - Upload date
  - Last modified date
  - Uploaded by (UKNF Employee, optionally name)
  - Download count (optional)
- External users see details only for files they have access to
- UKNF Employees see all file details plus access permissions
- User can download file from details page
- Details page shows change history link

### Story 6.6: Update File Metadata (UKNF)

**As a** UKNF Employee  
**I want to** update file metadata  
**So that** information remains accurate and current

**Acceptance Criteria:**
- UKNF Employee can select file and choose "Edit Metadata"
- Employee can modify: Filename, Description, Category, Reporting Period, Model Update Date
- Employee cannot change the actual file (requires file replacement - Story 6.7)
- Changes save immediately
- Last modified timestamp is updated
- Users see updated metadata immediately in their Library view
- System logs metadata changes with timestamp, employee ID, and changed fields
- Change history captures previous values

### Story 6.7: Replace File (New Version Upload)

**As a** UKNF Employee  
**I want to** upload a new version of an existing file  
**So that** users access the most current documents

**Acceptance Criteria:**
- UKNF Employee can select existing file and choose "Upload New Version"
- Employee uploads new file (can be different filename)
- Employee can update metadata (Reporting Period, Model Update Date, Description)
- System marks previous version as "Archival" with archive date
- New version is marked as "Current"
- Access permissions are inherited from previous version (can be modified after upload)
- Previous version remains accessible (Story 6.10)
- Users see "Updated" indicator or notification for files they previously accessed
- System logs version upload with timestamp and employee ID
- Change history shows version transition

### Story 6.8: Delete File from Library (UKNF)

**As a** UKNF Employee  
**I want to** delete files that are no longer needed  
**So that** the Library remains current and uncluttered

**Acceptance Criteria:**
- UKNF Employee can select file and choose "Delete"
- System prompts confirmation: "Are you sure you want to delete [filename]? This action cannot be undone."
- Upon confirmation, file is deleted (soft delete recommended - mark as deleted but retain in database)
- Deleted files are no longer visible to external users
- Deleted files are no longer visible in standard UKNF Library view (but can be viewed in "Deleted Files" section)
- File download links become invalid
- System logs deletion with timestamp and employee ID
- Deletion is captured in change history
- Consider: Prevent deletion if file has been downloaded recently or is referenced in reports

### Story 6.9: Categorize and Organize Files

**As a** UKNF Employee  
**I want to** organize Library files into categories  
**So that** users can easily find related documents

**Acceptance Criteria:**
- UKNF Employee can create, edit, and delete categories
- Category includes: Name, Description, Sort order (optional)
- Categories are displayed as filters or navigation sections in Library
- Employee can assign files to categories during upload or via metadata edit
- A file can belong to one category (or multiple if designed that way)
- Categories are consistent across all users
- Empty categories can exist but are hidden from external user view
- Category changes take effect immediately
- System logs category management actions

### Story 6.10: Access Archival File Versions

**As an** External User or UKNF Employee  
**I want to** access previous versions of files  
**So that** I can reference historical documents if needed

**Acceptance Criteria:**
- Users can toggle view to include archival versions in Library
- Archival files display: Filename, Version date, Archival date, Model Update Date
- Users with access to current version automatically have access to archival versions
- User can download archival versions same as current
- File details clearly indicate version status and date
- Archival versions are listed chronologically (newest to oldest)
- Downloads of archival versions are logged same as current

### Story 6.11: View File Change History (UKNF)

**As a** UKNF Employee  
**I want to** view complete change history for Library files  
**So that** I can understand all modifications and access changes

**Acceptance Criteria:**
- UKNF Employee can access "Change History" for any file
- History shows: Timestamp, Action, Changed by (employee name), Previous value, New value
- History includes:
  - File uploads (new and version replacements)
  - Metadata changes (all fields)
  - Permission changes (who gained/lost access)
  - Category changes
  - Archiving actions
  - Deletion actions
- History is chronologically ordered (newest first)
- History is read-only
- History can be exported (PDF or CSV)

### Story 6.12: Monitor File Access (UKNF)

**As a** UKNF Employee  
**I want to** see which users have downloaded files  
**So that** I can understand usage patterns and ensure important files are accessed

**Acceptance Criteria:**
- UKNF Employee can access "Access Report" for any file
- Report shows: User name, Entity, Download timestamp, IP address
- Report can be filtered by: Date range, Entity, User
- Report can be sorted by: Download date, User, Entity
- Report shows total download count
- Employee can export access report (CSV)
- For important files (e.g., mandatory report templates), employee can identify entities that haven't downloaded
- Report respects user privacy (internal analytics use only)

### Story 6.13: Notify Users of New/Updated Files

**As a** UKNF Employee  
**I want to** notify users when important files are added or updated  
**So that** they are aware and can access them promptly

**Acceptance Criteria:**
- UKNF Employee can select file and choose "Notify Users"
- Employee can compose notification message (optional)
- System sends notification to all users with access to the file
- Notification includes: File name, Description, Link to file in Library
- Users receive email notification
- Users see in-app notification/indicator (e.g., badge on Library menu item)
- Notification is logged with timestamp and employee ID

### Story 6.14: Bulk Operations on Files (UKNF)

**As a** UKNF Employee  
**I want to** perform bulk operations on multiple files  
**So that** I can manage the Library efficiently

**Acceptance Criteria:**
- UKNF Employee can select multiple files using checkboxes
- Employee can perform bulk actions:
  - Change category
  - Update access permissions (add/remove recipients)
  - Archive files (mark as archival)
  - Delete files
- System prompts confirmation for bulk actions showing affected file count
- All bulk actions are logged with timestamp, employee ID, and affected files
- Changes take effect immediately for all selected files
- If any file fails validation, error is shown and operation is rolled back or partially applied with report

---

## Technical Considerations

### File Storage

[Source: docs/prd/b-specification-of-non-functional-requirements.md#1-file-management]

- Store files securely on server or cloud storage (e.g., AWS S3, Azure Blob)
- Implement virus scanning on upload
- Backup file storage regularly
- Define file retention policy for deleted files
- Consider file deduplication for efficiency

### File Metadata Schema

Files should capture:
- File ID (unique identifier)
- Uploaded file (reference to storage location)
- Filename (display name, can differ from storage name)
- Description
- Category ID (foreign key to Categories table)
- Reporting Period
- Model Update Date
- Version Status (Current/Archival)
- Archival Date (if archival)
- File Size
- File Format/Extension
- Upload Date
- Last Modified Date
- Uploaded By (UKNF Employee user ID)
- Download Count
- Access Permissions (many-to-many relationships with entities, users, groups)

### Access Control Model

- **File-Permission Relationship**: Many-to-many (files can be shared with multiple recipients, recipients can access multiple files)
- **Permission Types**: All users, Specific entities, Entity types, Specific users, Contact groups
- **Inheritance**: Consider if contact group members inherit access or if access is evaluated dynamically
- **Performance**: Index permissions for fast lookup when users browse Library

### Versioning Strategy

- **Version Chain**: Link files in version chain (current → archival v2 → archival v1)
- **Version Identifier**: Use version number or version date
- **Archival Trigger**: When new version is uploaded, previous current becomes archival
- **Single Current Version**: Only one "Current" version per file chain

### Performance Considerations

[Source: docs/prd/b-specification-of-non-functional-requirements.md#3-performance-and-scalability]

- Library lists should paginate for large file counts
- File uploads should handle large files efficiently
- Downloads should be optimized (consider CDN for frequently accessed files)
- Database indexes on category, version status, model update date
- Cache frequently accessed file metadata

### Security Requirements

- Only UKNF Employees can upload, modify, delete files
- External users can only download files they have permissions for
- Validate all file uploads (virus scan, file type validation)
- Secure file storage with proper access controls
- Audit all file access (downloads) for compliance

### Audit Requirements

[Source: docs/prd/b-specification-of-non-functional-requirements.md#22-audit-and-login]

- Log all file uploads, updates, deletions
- Log all metadata changes
- Log all permission changes
- Log all file downloads (user, file, timestamp)
- Include timestamps and user IDs for all actions

---

## Dependencies

### Prerequisites
- **Epic 1**: Authentication (authenticated users)
- **Epic 2**: Authorization (to identify entities for access control)
- **File storage infrastructure**: Secure storage, virus scanning

### Integrates With
- **Epic 4**: Reporting System (report templates are downloaded from Library)
- **Epic 7**: Bulletin Board & Contact Management (contact groups can be used for file access permissions)

### Blocks
- **Epic 4**: Reporting workflow depends on Library for templates (can be developed in parallel)

---

## Definition of Done

- [ ] UKNF Employees can upload files to Library with metadata
- [ ] UKNF Employees can set access permissions for files (all users, specific entities, groups, etc.)
- [ ] External users can view and search files they have access to
- [ ] Users can download files from Library
- [ ] Users can view complete file details
- [ ] UKNF Employees can update file metadata
- [ ] UKNF Employees can upload new versions (replace files)
- [ ] Previous versions are archived and remain accessible
- [ ] UKNF Employees can delete files
- [ ] Files can be organized into categories
- [ ] External users can filter and search by category, reporting period, etc.
- [ ] UKNF Employees can view complete file change history
- [ ] UKNF Employees can monitor file access (download tracking)
- [ ] UKNF Employees can notify users of new/updated files
- [ ] UKNF Employees can perform bulk operations on files
- [ ] All file uploads are virus scanned
- [ ] All file operations are logged for audit
- [ ] Unit tests cover file management and access control
- [ ] Integration tests verify versioning and permissions
- [ ] Security testing confirms proper authorization enforcement
- [ ] Performance testing confirms efficient file handling
- [ ] Documentation includes file metadata schema and permission model

---

## Related PRD Sections

- [Functions of the Communication Module - Maintaining the Local File Repository](./functions-of-the-communication-module.md#maintaining-the-local-file-repository-ensures)
- [B. Specification of Non-Functional Requirements - File Management](./b-specification-of-non-functional-requirements.md#1-file-management)
- [Preferred Functionalities](./preferred-functionalities.md)

---

## Notes

- Library is closely tied to Epic 4 (Reporting) - entities download report templates from Library
- Consider implementing templates or placeholders if Library is implemented before Epic 4
- Versioning strategy should be clearly documented for developers
- Access permissions can be complex - consider starting with simple model (All users vs. Specific entities) and expanding
- File notifications could be a separate story or integrated with Epic 7 (Bulletin Board)
- Download tracking provides valuable analytics for UKNF to monitor engagement
- Consider implementing "Featured" or "Important" files that are highlighted in Library
- Change history is valuable for compliance and troubleshooting
- Bulk operations improve efficiency but require careful UI design and confirmation flows

