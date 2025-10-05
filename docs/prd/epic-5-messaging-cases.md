# Epic 5: Messaging & Cases

**Epic Number:** 5  
**Phase:** Core Communication Features (Preferred Functionalities)  
**Priority:** High (Core platform value)  
**Status:** Draft

---

## Epic Goal

Implement two-way communication between UKNF employees and supervised entities through a messaging system with file attachments, and establish a case management workflow for administrative matters requiring structured tracking and resolution.

---

## Epic Description

### Context

The Communication Platform requires structured communication channels beyond reporting. This epic implements two core communication mechanisms:

1. **Messaging System**: General two-way communication with file attachments, integrated throughout the system (access requests, reports, cases)
2. **Case Management**: Administrative cases for structured handling of entity matters with defined workflows and statuses

[Source: docs/prd/functions-of-the-communication-module.md]

### What This Epic Delivers

This epic implements comprehensive communication functionality including:

- **Two-Way Messaging**: Messages with file attachments between internal and external users
- **Message Integration**: Messaging used within access requests (Epic 2), reports (Epic 4), and cases
- **Case Workflow**: Create, manage, and track administrative cases with statuses
- **Case Categories**: Structured categories for different case types
- **Case Priority**: Low, Medium, High priority levels
- **Case Communication**: Messages and attachments within case context
- **Case Cancellation**: Ability to cancel cases that haven't been viewed
- **Case History**: Complete audit trail of case changes

### Key Business Rules

1. **Message Integration**: Messaging is used in multiple contexts (access requests, reports, cases) - it's a cross-cutting feature
2. **File Attachments**: Allowed formats: PDF, DOC/DOCX, XLS/XLSX, CSV/TXT, MP3, ZIP (max 100 MB total for unpacked files)
3. **Security Scanning**: Reject files with viruses or SPAM
4. **Cases Single Entity**: Each case concerns only one supervised entity
5. **Case Cancellation**: Cases can only be cancelled before the entity views them (status: "New case")
6. **Message Statuses**: Track who needs to respond (UKNF or External User)
7. **Case Draft**: Entities can save case drafts not yet visible to UKNF

[Source: docs/prd/functions-of-the-communication-module.md]

---

## User Stories

### Story 5.1: Send Message with Attachments

**As a** UKNF Employee or External User  
**I want to** send messages with file attachments  
**So that** I can communicate and share documents

**Acceptance Criteria:**

- User can compose message with text content (required)
- User can attach multiple files in allowed formats: PDF, DOC/DOCX, XLS/XLSX, CSV/TXT, MP3, ZIP
- System validates file formats before accepting
- System validates total unpacked file size doesn't exceed 100 MB
- System rejects files exceeding size limit with clear error message
- System scans files for viruses before accepting
- System rejects files with viruses and notifies user
- System rejects messages containing SPAM
- Message requires recipient selection (entity or specific user)
- Message is associated with context (standalone, access request, report, or case)
- Upon sending, message status is set appropriately based on recipient type
- Recipient receives email notification of new message
- System logs message sending with timestamp and user ID

### Story 5.2: Receive and View Messages

**As a** UKNF Employee or External User  
**I want to** view messages sent to me  
**So that** I can read communications and respond

**Acceptance Criteria:**

- User can view list of messages addressed to them
- Message list displays: Sender, Subject/Preview, Date, Status, Context (if part of case/request/report)
- User can filter messages by: Status, Sender, Date range, Context
- User can sort messages by: Date, Sender, Status
- User can mark messages as read/unread
- User can search messages by content or sender
- User can click message to view full details
- Message detail shows: Full content, Sender details, Timestamp, All attachments with download links, Message thread (if part of conversation)
- User can download attachments individually
- Message list supports pagination

### Story 5.3: Reply to Message

**As a** UKNF Employee or External User  
**I want to** reply to messages I received  
**So that** I can continue the conversation

**Acceptance Criteria:**

- User can reply to any message addressed to them
- Reply form pre-populates with original message context
- User can add text content (required)
- User can attach files following same rules as Story 5.1
- Reply is threaded with original message
- Upon sending reply, original message status changes to "Closed"
- New reply has status set based on recipient type
- Recipient receives email notification
- Message thread shows complete conversation history
- System logs reply with timestamp and user ID

### Story 5.4: Message Status Management

**As the** system  
**I want to** track message statuses based on sender type  
**So that** users can see which messages require their response

**Acceptance Criteria:**

- Message sent by External User to UKNF has status "Awaits UKNF's response"
- Message sent by UKNF Employee to External User has status "Awaiting the User's Response"
- When recipient replies, message status changes to "Closed"
- Users can filter messages by status
- UKNF Employees can use quick filter "Requires UKNF responses" to see all messages with status "Awaits UKNF's response"
- Dashboard shows count of messages requiring user's response
- Closed messages remain accessible in message history

### Story 5.5: View Messages List with Quick Filters (UKNF)

**As a** UKNF Employee  
**I want to** view and filter messages across all entities  
**So that** I can efficiently manage communications

**Acceptance Criteria:**

- UKNF Employee can view list of all messages
- Quick filter "My Entities" shows messages related to entities assigned to employee
- Quick filter "Requires UKNF responses" shows messages with status "Awaits UKNF's response"
- Employee can filter by: Component (access request, case, report, standalone), Status, Entity, Date range
- Employee can search by sender name, entity name, or message content
- List displays: Entity, Sender, Subject/Preview, Date, Status, Component
- Employee can sort by any column
- Employee can export message list (CSV)
- List supports pagination

### Story 5.6: Mass Messaging to Entities

**As a** UKNF Employee  
**I want to** send messages to multiple entities at once  
**So that** I can communicate efficiently with groups

**Acceptance Criteria:**

- UKNF Employee can compose message to multiple recipients
- Employee can select recipients by:
  - Individual entities from list
  - Entity types/market types (e.g., all "Loan institutions")
  - Contact groups (integration with Epic 7)
- Employee composes message once with text and attachments
- System creates individual message for each selected entity
- Each message follows standard message rules (statuses, notifications)
- Employee receives confirmation showing how many messages were sent
- System logs mass message action with timestamp, employee ID, and recipient count

### Story 5.7: Create Case (External User)

**As an** Employee of Supervised Entity with Cases permission  
**I want to** create an administrative case  
**So that** I can formally communicate with UKNF about entity matters

**Acceptance Criteria:**

- User can access "Create Case" functionality (requires Cases permission from Epic 2)
- User selects entity (if representing multiple)
- User provides case details:
  - Category (dropdown): "Change of registration data", "Change of staff composition", "Call to the Supervised Entity", "Entitlements to the System", "Reporting", "Other"
  - Priority (dropdown): Low, Medium, High
  - Subject/Title (text, required)
  - Description (text area, required)
- User can attach files following attachment rules (Story 5.1)
- User can save case as "Draft" (not submitted, not visible to UKNF)
- User can submit case (status changes from Draft to "New case")
- Upon submission, UKNF Employee receives notification
- Submitter receives confirmation email
- Case is visible in user's cases list
- System logs case creation with timestamp and user ID

### Story 5.8: View and Filter Cases (External User)

**As an** Employee of Supervised Entity  
**I want to** view all cases for my entity  
**So that** I can track status and history

**Acceptance Criteria:**

- User can view list of all cases for their entity (including Drafts)
- List displays: Case number, Subject, Category, Priority, Status, Created date, Last updated
- User can filter by: Status, Category, Priority, Date range
- User can sort by: Case number, Created date, Status, Priority
- User can search by subject or case number
- User can click case to view details
- Draft cases are clearly marked and separated
- List supports pagination

### Story 5.9: View and Filter Cases (UKNF Employee)

**As a** UKNF Employee  
**I want to** view and filter cases across all entities  
**So that** I can manage and respond to administrative matters

**Acceptance Criteria:**

- UKNF Employee can view list of all cases (excluding Drafts which are not visible to UKNF)
- Quick filter "My Entities" shows cases for entities assigned to employee
- Quick filter "Requires Action" shows cases with statuses: "New case", "To be completed"
- Employee can filter by: Status, Category, Priority, Entity, Date range
- Employee can search by entity name, case subject, submitter name
- List displays: Entity, Case number, Subject, Category, Priority, Status, Submitter (name, email, phone), Created date
- Employee can sort by any column
- Employee can export case list (CSV)
- List supports pagination

### Story 5.10: View Case Details

**As a** UKNF Employee or External User  
**I want to** view complete details of a case  
**So that** I can understand the matter and take appropriate action

**Acceptance Criteria:**

- User can view comprehensive case details:
  - Case number
  - Entity name
  - Submitter information (name, email, phone)
  - Case handler (UKNF Employee, if assigned)
  - Category
  - Priority
  - Status
  - Subject/Title
  - Description
  - Created date
  - Last updated date
  - All attachments with download links
  - Message thread (all communications within case)
  - Status history
- User can download attachments
- User can view complete message thread
- User can send message within case context (Story 5.1-5.3)
- External users can view their own entity's cases
- UKNF Employees can view all cases
- Status history shows all transitions with timestamps

### Story 5.11: Manage Case Status (UKNF Employee)

**As a** UKNF Employee  
**I want to** update case statuses as I work on them  
**So that** the workflow is tracked and visible to entities

**Acceptance Criteria:**

- UKNF Employee can change case status
- Available status transitions:
  - "New case" → "Ongoing" (automatically when Employee opens case)
  - "New case" → "Cancelled" (before entity views it)
  - "Ongoing" → "To be completed" (requires entity to supplement information)
  - "Ongoing" → "Completed" (case is resolved)
  - "To be completed" → "Ongoing" (after entity supplements)
- Status changes are validated (only allowed transitions)
- When status changes to "To be completed", employee must provide a description of what's needed
- When status changes to "Completed", case is marked as closed
- Entity receives email notification for all status changes except automatic "Ongoing"
- Status changes are logged with timestamp and employee ID
- Previous statuses are maintained in history

### Story 5.12: Assign Case Handler (UKNF)

**As a** UKNF Employee with appropriate permissions  
**I want to** assign cases to specific UKNF Employees  
**So that** responsibilities are clear and cases are managed effectively

**Acceptance Criteria:**

- Authorized UKNF Employee can assign case handler
- Can select handler from list of UKNF Employees
- Assigned handler receives notification
- Case handler is displayed in case details
- Case handler can be reassigned
- Handler assignments are logged with timestamp

### Story 5.13: Request Case Completion (UKNF)

**As a** UKNF Employee  
**I want to** request that entity supplement information for a case  
**So that** I can obtain additional details needed for resolution

**Acceptance Criteria:**

- UKNF Employee changes case status to "To be completed"
- Employee provides description of what information/attachments are needed (required text field)
- Entity receives email notification with description
- Case status is prominently displayed as "To be completed"
- Description is visible in case details
- Entity can add messages and attachments to supplement information
- Entity can indicate completion (case handler notified)
- Employee can then review and change status to "Ongoing" or "Completed"

### Story 5.14: Cancel Case (UKNF)

**As a** UKNF Employee  
**I want to** cancel a case created in error before entity views it  
**So that** I can prevent confusion

**Acceptance Criteria:**

- UKNF Employee can select case with status "New case"
- Employee chooses "Cancel" action
- System verifies entity hasn't viewed case yet
- If entity hasn't viewed: Case status changes to "Cancelled", entity sees "cancelled message" placeholder
- If entity already viewed: System prevents cancellation with error message
- Entity receives notification that case was cancelled
- Cancelled cases are visible to UKNF with "Cancelled" status
- Cancelled cases cannot be edited or reopened
- Case details and attachments remain in system (read-only)
- Cancellation is logged with timestamp and employee ID

### Story 5.15: View Case History

**As a** UKNF Employee or External User  
**I want to** view complete history of case changes  
**So that** I can understand the full lifecycle and all actions taken

**Acceptance Criteria:**

- Users can view case change history
- History shows: Timestamp, Action/Change, Changed by, Previous value, New value
- History includes: Status changes, Handler assignments, Priority changes, Messages sent, Attachments added
- History is chronologically ordered
- History is read-only (cannot be modified)
- History can be exported (PDF or CSV)

### Story 5.16: Edit Draft Case

**As an** Employee of Supervised Entity  
**I want to** edit cases saved as draft  
**So that** I can refine them before submission

**Acceptance Criteria:**

- User can open draft cases from their cases list
- User can edit all case fields (category, priority, subject, description)
- User can add or remove attachments
- User can save changes to draft (remains in Draft status)
- User can delete draft case
- User can submit draft case (status changes to "New case")
- Draft cases are not visible to UKNF Employees
- Changes to drafts don't trigger notifications
- System logs draft edits with timestamp

---

## Case Statuses

[Source: docs/prd/functions-of-the-communication-module.md]

| Status              | Description                                                                            |
| ------------------- | -------------------------------------------------------------------------------------- |
| **Draft**           | Case saved but not yet submitted. Not visible to UKNF Employee.                        |
| **New case**        | Case submitted/started. Visible to UKNF Employee.                                      |
| **Ongoing**         | Set automatically after case opened by UKNF Employee or External User.                 |
| **To be completed** | Set by UKNF Employee, indicates entity needs to supplement information or attachments. |
| **Cancelled**       | Set by UKNF Employee, possible only if entity hasn't yet viewed it.                    |
| **Completed**       | Set by UKNF Employee, indicates case is resolved.                                      |

## Message Statuses

[Source: docs/prd/functions-of-the-communication-module.md]

| Status                           | Description                                                            |
| -------------------------------- | ---------------------------------------------------------------------- |
| **Awaits UKNF's response**       | Message added by External User                                         |
| **Awaiting the User's Response** | Message added by UKNF Employee                                         |
| **Closed**                       | Message for which there is a response from UKNF Employee/External User |

---

## Technical Considerations

### File Attachment Rules

[Source: docs/prd/functions-of-the-communication-module.md]

- **Allowed formats**: PDF, DOC/DOCX, XLS/XLSX, CSV/TXT, MP3, ZIP
- **Size limit**: 100 MB total for unpacked files (if ZIP, extract and validate total size)
- **Validation**: Check file format by content (magic numbers), not just extension
- **Security**: Scan all files for viruses and malware
- **SPAM**: Implement SPAM detection for message content
- **Storage**: Store files securely with backup

### Message Data Model

Messages should capture:

- Message ID
- Sender (user ID and type: Internal/External)
- Recipient (user ID or entity ID)
- Context (standalone, access request ID, report ID, case ID)
- Subject/Content
- Attachments (references to files)
- Status (Awaits UKNF's response, Awaiting the User's Response, Closed)
- Thread ID (for conversation threading)
- Timestamp
- Read status

### Case Data Model

Cases should capture:

- Case number (unique identifier, auto-generated)
- Entity ID
- Submitter user ID and contact details
- Case handler (UKNF Employee user ID, optional)
- Category
- Priority
- Status
- Subject
- Description
- Created timestamp
- Last updated timestamp
- Attachments (references to files)
- Messages (thread of communications)
- Status history (audit trail)

### Integration Points

- **Epic 2 - Access Requests**: Messaging used for communication within access requests
- **Epic 4 - Reports**: Messaging used for report-related questions (e.g., asking entities to clarify reports)
- **Epic 7 - Contact Groups**: Mass messaging can use contact groups as recipients

### Performance Considerations

- Message lists and case lists should paginate for large datasets
- File uploads should handle large files efficiently (chunked uploads if needed)
- Virus scanning should not block user experience (async processing acceptable)
- Database indexes on entity, status, category, created date

### Security Requirements

- Only users with Cases permission can create and view cases
- External users can only view cases for their entities
- UKNF Employees can view all cases (or filtered by "My Entities")
- File downloads require authentication and authorization
- Validate message recipients have appropriate permissions
- Audit all message sends, case creations, and status changes

### Audit Requirements

[Source: docs/prd/b-specification-of-non-functional-requirements.md#22-audit-and-login]

- Log all messages sent with sender, recipient, and context
- Log all case creations, edits, and submissions
- Log all case status changes
- Log all case handler assignments
- Log all file attachments
- Include timestamps and user IDs

---

## Dependencies

### Prerequisites

- **Epic 1**: Authentication (authenticated users)
- **Epic 2**: Authorization (Cases permission)
- **File storage and security**: Virus scanning, secure storage

### Integrates With

- **Epic 2**: Authorization & Access Requests (messaging within requests)
- **Epic 4**: Reporting System (messaging about reports)
- **Epic 7**: Bulletin Board & Contact Management (contact groups for mass messaging)

### Blocks

- Can begin development after Epic 1-2 are complete
- Mass messaging to contact groups enhanced by Epic 7 (but can work with entity selection initially)

---

## Definition of Done

- [ ] Users can send messages with file attachments in allowed formats
- [ ] File size limits and format validations are enforced
- [ ] Virus scanning rejects infected files
- [ ] Users can receive, view, and reply to messages
- [ ] Message statuses track who needs to respond
- [ ] Message threading shows conversation history
- [ ] UKNF Employees can use quick filters for efficient message management
- [ ] UKNF Employees can send mass messages to multiple entities
- [ ] External users can create cases with categories and priorities
- [ ] External users can save case drafts
- [ ] External users can view and filter their entity's cases
- [ ] UKNF Employees can view and filter all cases
- [ ] Case details show complete information and message thread
- [ ] UKNF Employees can manage case statuses with proper workflow
- [ ] UKNF Employees can assign case handlers
- [ ] UKNF Employees can request case completion with description
- [ ] UKNF Employees can cancel cases before entity views them
- [ ] Complete case history is tracked and viewable
- [ ] Messaging is integrated with access requests (Epic 2)
- [ ] Messaging is integrated with reports (Epic 4)
- [ ] All file operations are secure and validated
- [ ] All messaging and case events are logged for audit
- [ ] Unit tests cover messaging and case workflows
- [ ] Integration tests verify file handling and status transitions
- [ ] Security testing confirms proper authorization enforcement
- [ ] Performance testing confirms system handles many messages and cases
- [ ] Documentation includes message and case status definitions and workflows

---

## Related PRD Sections

- [Functions of the Communication Module - Message Handling](./functions-of-the-communication-module.md#message-handling-provides)
- [Functions of the Communication Module - Case Handling](./functions-of-the-communication-module.md#handling-and-handling-cases-in-administrative-mode-provides)
- [B. Specification of Non-Functional Requirements - File Management](./b-specification-of-non-functional-requirements.md#1-file-management)
- [Preferred Functionalities](./preferred-functionalities.md)

---

## Notes

- Messaging is a cross-cutting feature used in multiple contexts - design for reusability
- Message threading is important for conversation context
- Case cancellation has specific rules - only before entity views it
- Draft cases allow entities to prepare submissions without committing
- Consider implementing case templates for common categories
- Case handler assignment might need permission controls (who can assign)
- Mass messaging is powerful - ensure proper logging and confirmation UI
- File attachment security is critical - thorough validation and scanning required
