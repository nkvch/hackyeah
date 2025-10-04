# **Section 7: Core Workflows**

Let me illustrate the key system workflows using sequence diagrams to show how components interact for critical user journeys.

---

## **Workflow 1: External User Registration and Access Request**

This workflow shows the complete onboarding process for a new external user (entity representative).

```mermaid
sequenceDiagram
    actor EU as External User
    participant UI as Angular SPA
    participant AUTH as Auth Service
    participant REQ as Access Request Service
    participant NOTIF as Notification Service
    participant DB as Database
    participant MAIL as MailDev (SMTP)
    
    EU->>UI: Fill registration form (name, PESEL, email, phone)
    UI->>AUTH: POST /api/auth/register
    AUTH->>DB: Create user account (inactive)
    AUTH->>NOTIF: Send activation email
    NOTIF->>MAIL: SMTP: Activation link
    MAIL-->>EU: Email with activation link
    
    EU->>UI: Click activation link
    UI->>AUTH: POST /api/auth/activate-account
    AUTH->>DB: Activate account
    AUTH->>REQ: Auto-create access request (Working status)
    REQ->>DB: Save access request
    AUTH-->>UI: Success + login redirect
    
    EU->>UI: Login with credentials
    UI->>AUTH: POST /api/auth/login
    AUTH->>DB: Verify credentials
    AUTH-->>UI: JWT token
    
    EU->>UI: Complete access request (select entities, permissions)
    UI->>REQ: PUT /api/access-requests/{id}
    REQ->>DB: Update request details
    
    EU->>UI: Submit access request
    UI->>REQ: POST /api/access-requests/{id}/submit
    REQ->>DB: Change status: Working → New
    REQ->>NOTIF: Notify UKNF employee
    NOTIF->>MAIL: Email to UKNF
    REQ-->>UI: Confirmation message
    
    Note over EU,MAIL: UKNF Employee Review Process
    
    actor IU as UKNF Employee
    participant HUB as SignalR Hub
    
    NOTIF->>HUB: Push notification
    HUB-->>IU: Real-time alert
    
    IU->>UI: Review access request
    UI->>REQ: GET /api/access-requests/{id}
    REQ->>DB: Fetch request details
    REQ-->>UI: Request data
    
    IU->>UI: Approve request
    UI->>REQ: POST /api/access-requests/{id}/approve
    REQ->>DB: Update status: New → Accepted
    REQ->>DB: Activate permission lines
    REQ->>NOTIF: Notify external user
    NOTIF->>MAIL: Approval email
    NOTIF->>HUB: Push to user if online
    HUB-->>EU: Real-time notification
    REQ-->>UI: Success
```

---

## **Workflow 2: Report Submission and Validation**

This demonstrates the async report validation process with status updates.

```mermaid
sequenceDiagram
    actor EU as External User
    participant UI as Angular SPA
    participant RPT as Report Service
    participant FILE as File Storage Service
    participant QUEUE as RabbitMQ
    participant WORKER as Report Validator Worker
    participant VAL as Mock Validator
    participant NOTIF as Notification Service
    participant HUB as SignalR Hub
    participant DB as Database
    
    EU->>UI: Select report file (XLSX)
    UI->>UI: Client-side validation (file type, size)
    
    UI->>RPT: POST /api/reports/upload (chunked upload)
    RPT->>FILE: Store file chunks
    FILE->>FILE: Assemble complete file
    FILE-->>RPT: Storage key
    RPT->>DB: Create report record (Status: Working)
    RPT-->>UI: Report ID
    
    EU->>UI: Confirm submission
    UI->>RPT: POST /api/reports/{id}/submit
    RPT->>DB: Update status: Working → Transmitted
    RPT->>QUEUE: Publish ReportValidationJob
    RPT->>NOTIF: Send confirmation
    NOTIF->>HUB: Push status update
    HUB-->>EU: "Report submitted"
    RPT-->>UI: Unique validation ID
    
    Note over QUEUE,WORKER: Async Background Processing
    
    WORKER->>QUEUE: Consume ReportValidationJob
    WORKER->>DB: Update status: Transmitted → Ongoing
    WORKER->>HUB: Push status update
    HUB-->>EU: "Validation in progress"
    
    WORKER->>FILE: Download report file
    FILE-->>WORKER: XLSX stream
    
    WORKER->>VAL: Validate report (mock logic)
    VAL->>VAL: Parse XLSX structure
    VAL->>VAL: Validate data rules
    VAL->>VAL: Generate result PDF
    VAL-->>WORKER: Validation result + PDF
    
    alt Validation Successful
        WORKER->>DB: Update status: Ongoing → Successful
        WORKER->>FILE: Store result PDF
        WORKER->>NOTIF: Notify user of success
    else Validation Errors
        WORKER->>DB: Update status: Ongoing → ValidationErrors
        WORKER->>DB: Store error details
        WORKER->>FILE: Store result PDF with errors
        WORKER->>NOTIF: Notify user of errors
    else Technical Error
        WORKER->>DB: Update status: Ongoing → TechnicalError
        WORKER->>NOTIF: Notify user of failure
    end
    
    NOTIF->>HUB: Push final status
    HUB-->>EU: Real-time notification
    
    EU->>UI: View validation result
    UI->>RPT: GET /api/reports/{id}/validation-result
    RPT->>FILE: Get result PDF
    FILE-->>RPT: PDF stream
    RPT-->>UI: PDF download
```

---

## **Workflow 3: Case Management with Messages**

Shows the case lifecycle with integrated messaging.

```mermaid
sequenceDiagram
    actor EU as External User
    participant UI as Angular SPA
    participant CASE as Case Service
    participant MSG as Message Service
    participant FILE as File Storage Service
    participant NOTIF as Notification Service
    participant HUB as SignalR Hub
    participant DB as Database
    actor IU as UKNF Employee
    
    EU->>UI: Create new case
    UI->>CASE: POST /api/cases (Draft)
    CASE->>DB: Save case (Status: Draft)
    CASE-->>UI: Case ID
    
    EU->>UI: Add case details + attachments
    UI->>FILE: Upload attachments
    FILE-->>UI: Storage keys
    UI->>CASE: PUT /api/cases/{id}
    CASE->>DB: Update case details
    
    EU->>UI: Submit case
    UI->>CASE: POST /api/cases/{id}/submit
    CASE->>DB: Update status: Draft → New
    CASE->>NOTIF: Notify assigned UKNF employee
    NOTIF->>HUB: Push notification
    HUB-->>IU: Real-time alert
    CASE-->>UI: Confirmation
    
    IU->>UI: Open case
    UI->>CASE: GET /api/cases/{id}
    CASE->>DB: Update status: New → Ongoing (first view)
    CASE->>DB: Fetch case details
    CASE-->>UI: Case data with attachments
    
    IU->>UI: Send message in case
    UI->>MSG: POST /api/messages (ContextType: Case, ContextId: {caseId})
    MSG->>DB: Create message
    MSG->>DB: Update case status: AwaitingUserResponse
    MSG->>NOTIF: Notify external user
    NOTIF->>HUB: Push message notification
    HUB-->>EU: Real-time message alert
    MSG-->>UI: Success
    
    EU->>UI: View case message
    UI->>MSG: GET /api/messages?contextId={caseId}
    MSG->>DB: Fetch messages
    MSG->>DB: Mark as read
    MSG-->>UI: Message thread
    
    EU->>UI: Reply to message
    UI->>MSG: POST /api/messages/{id}/reply
    MSG->>DB: Create reply message
    MSG->>DB: Update status: AwaitingUknfResponse
    MSG->>NOTIF: Notify UKNF employee
    NOTIF->>HUB: Push notification
    HUB-->>IU: Real-time alert
    
    IU->>UI: Mark case as completed
    UI->>CASE: POST /api/cases/{id}/change-status (Completed)
    CASE->>DB: Update status + completion date
    CASE->>NOTIF: Notify external user
    NOTIF->>HUB: Push notification
    HUB-->>EU: "Case completed"
    CASE-->>UI: Success
```

---

## **Workflow 4: Bulletin Board with Read Confirmation**

Demonstrates mass announcement distribution with read tracking.

```mermaid
sequenceDiagram
    actor IU as UKNF Employee
    participant UI as Angular SPA
    participant BB as Bulletin Board Service
    participant CG as Contact Group Service
    participant NOTIF as Notification Service
    participant HUB as SignalR Hub
    participant DB as Database
    actor EU1 as External User 1
    actor EU2 as External User 2
    
    IU->>UI: Create bulletin announcement
    UI->>BB: POST /api/bulletin-board
    BB->>DB: Create announcement (unpublished)
    BB-->>UI: Announcement ID
    
    IU->>UI: Edit content (WYSIWYG), add attachments
    UI->>BB: PUT /api/bulletin-board/{id}
    BB->>DB: Update content
    
    IU->>UI: Select recipients (entity types, groups)
    UI->>CG: GET /api/contact-groups
    CG->>DB: Fetch groups
    CG-->>UI: Contact groups
    UI->>BB: PUT /api/bulletin-board/{id} (set recipients)
    BB->>DB: Store recipient criteria
    
    IU->>UI: Set priority = High, expiry date
    UI->>BB: PUT /api/bulletin-board/{id}
    BB->>DB: Update priority and expiry
    
    IU->>UI: Publish announcement
    UI->>BB: POST /api/bulletin-board/{id}/publish
    BB->>DB: Set IsPublished = true
    BB->>CG: Resolve recipients (expand groups to users)
    CG->>DB: Query users matching criteria
    CG-->>BB: List of recipient users
    
    BB->>DB: Create read tracking records
    BB->>NOTIF: Send notifications to all recipients
    
    par Notify Multiple Users
        NOTIF->>HUB: Push to EU1
        HUB-->>EU1: "New announcement: [Title]"
        NOTIF->>HUB: Push to EU2
        HUB-->>EU2: "New announcement: [Title]"
    end
    
    BB-->>UI: Published successfully
    
    Note over EU1,DB: External User Interactions
    
    EU1->>UI: Login and see notification badge
    UI->>BB: GET /api/bulletin-board (unread filter)
    BB->>DB: Fetch announcements + read status
    BB-->>UI: Announcements list (1 unread)
    
    EU1->>UI: Click announcement
    UI->>BB: GET /api/bulletin-board/{id}
    BB->>DB: Fetch announcement content
    BB-->>UI: Announcement with "Confirm Read" button
    
    EU1->>UI: Click "Confirm Read" (required for High priority)
    UI->>BB: POST /api/bulletin-board/{id}/confirm-read
    BB->>DB: Record read confirmation (user, timestamp)
    BB->>HUB: Update read statistics
    BB-->>UI: Confirmed
    
    IU->>UI: View announcement statistics
    UI->>BB: GET /api/bulletin-board/{id}/statistics
    BB->>DB: Count read confirmations
    BB-->>UI: "71/100 entities have read this announcement"
```

---

## **Workflow 5: Entity Context Selection (Multi-tenancy)**

Shows how users representing multiple entities switch context.

```mermaid
sequenceDiagram
    actor EU as External User
    participant UI as Angular SPA
    participant AUTH as Auth Service
    participant AUTHZ as Authorization Service
    participant CACHE as Redis Cache
    participant DB as Database
    
    EU->>UI: Login
    UI->>AUTH: POST /api/auth/login
    AUTH->>DB: Verify credentials
    AUTH->>DB: Fetch user's entities
    AUTH->>AUTH: Generate JWT with user claims
    AUTH-->>UI: JWT token + list of entities
    
    UI->>UI: Show entity selection dialog
    EU->>UI: Select entity to represent
    
    UI->>AUTHZ: POST /api/authorization/select-entity
    Note over AUTHZ: Store entity context in session
    AUTHZ->>CACHE: Set entity context (userId → entityId)
    AUTHZ->>DB: Fetch entity-specific permissions
    AUTHZ-->>UI: Entity context set + permissions
    
    UI->>UI: Display entity name in header
    UI->>UI: Enable/disable features based on permissions
    
    Note over EU,DB: All subsequent requests include entity context
    
    EU->>UI: View reports (scoped to selected entity)
    UI->>AUTHZ: Middleware extracts entity context
    AUTHZ->>CACHE: Get entity context for user
    CACHE-->>AUTHZ: EntityId
    AUTHZ->>AUTHZ: Inject into request context
    
    UI->>UI: Reports filtered to current entity
    
    Note over EU,DB: User switches entity
    
    EU->>UI: Click entity dropdown, select different entity
    UI->>AUTHZ: POST /api/authorization/select-entity (new entityId)
    AUTHZ->>CACHE: Update entity context
    AUTHZ->>DB: Fetch new entity permissions
    AUTHZ-->>UI: Context switched
    
    UI->>UI: Refresh page data with new entity scope
    UI->>UI: Update UI to reflect new entity and permissions
```

---

## **Workflow 6: File Upload with Virus Scanning (Simplified)**

Shows file handling with validation (virus scanning stubbed for hackathon).

```mermaid
sequenceDiagram
    actor User as User
    participant UI as Angular SPA
    participant FILE as File Storage Service
    participant MINIO as MinIO Storage
    participant DB as Database
    
    User->>UI: Select file for upload
    UI->>UI: Client validation (type, size)
    
    alt Large File (>10MB)
        UI->>FILE: POST /api/files/upload (chunk 1 of N)
        FILE->>MINIO: Store chunk 1
        UI->>FILE: POST /api/files/upload (chunk 2 of N)
        FILE->>MINIO: Store chunk 2
        Note over UI,FILE: Continue for all chunks...
        UI->>FILE: POST /api/files/upload (final chunk)
        FILE->>MINIO: Store final chunk
        FILE->>MINIO: Assemble complete file
    else Small File (<10MB)
        UI->>FILE: POST /api/files/upload (single request)
        FILE->>MINIO: Store file
    end
    
    FILE->>FILE: Validate file extension (PDF, XLSX, etc.)
    
    alt Invalid Extension
        FILE-->>UI: Error: File type not allowed
    else Valid Extension
        FILE->>DB: Create file record (Status: Validated)
        Note over FILE: Virus scanning stubbed - instant pass for demo
        FILE->>DB: Update status: Scanning → Clean
        FILE-->>UI: Storage key + metadata
    end
    
    User->>UI: Download file later
    UI->>FILE: GET /api/files/{key}
    FILE->>MINIO: Get pre-signed URL
    MINIO-->>FILE: Temporary download URL
    FILE-->>UI: Redirect to URL
    UI->>MINIO: Direct download (browser)
    MINIO-->>User: File stream
```

---

## **Rationale:**

1. **Real-time Updates**: SignalR hub integration shown in multiple workflows - critical for UX (report status, messages, notifications)

2. **Async Processing**: Report validation and file operations are queued via RabbitMQ, keeping API responses fast

3. **Multi-tenancy**: Entity context selection workflow demonstrates how row-level security works

4. **Error Handling**: Alternative paths shown (validation failures, technical errors) to demonstrate robust error handling

5. **Security**: All workflows go through authentication/authorization layers with JWT tokens

6. **Hackathon-Appropriate**: Workflows focus on demonstrating PRD requirements without unnecessary complexity
