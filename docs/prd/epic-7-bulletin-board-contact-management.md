# Epic 7: Bulletin Board & Contact Management

**Epic Number:** 7  
**Phase:** Advanced Communication Features (Preferred Functionalities)  
**Priority:** Medium (Enhances communication capabilities)  
**Status:** Draft

---

## Epic Goal

Implement a bulletin board system for publishing announcements to supervised entities with read confirmations, and establish a contact management system that enables UKNF employees to organize external users and non-system contacts into groups for targeted communications.

---

## Epic Description

### Context

Beyond individual messaging and cases, UKNF needs to broadcast information to groups of supervised entities through announcements. Additionally, UKNF needs to manage contacts (including people who are not system users) and organize them into groups for efficient, targeted communication.

[Source: docs/prd/functions-of-the-communication-module.md - "Handling messages in the form of a bulletin board" and "Support for addressees, contact groups and contacts"]

### What This Epic Delivers

This epic implements two related but distinct features:

**1. Bulletin Board:**
- Announcement creation and publishing by UKNF Employees
- Read-only announcements for external users
- Read receipt tracking and confirmation
- Priority levels and expiration dates
- Categorization and filtering
- Rich text editing (WYSIWYG)
- File attachments
- Targeted distribution (all users, specific groups, entity types)

**2. Contact Management:**
- Contact Groups: Organized lists of recipients for notifications
- Contacts: Individual recipients who may or may not be system users
- Addressee Types: Four ways to define recipients (Selected entities, Contact groups, Entity types, Selected users)
- Integration with other modules for recipient selection

### Key Business Rules

1. **Read-Only Bulletin Board**: External users can only read announcements, not create or edit them
2. **Read Confirmation Requirement**: High-priority announcements require explicit user confirmation of reading
3. **Non-User Contacts**: Contacts can be people without system accounts (receive email notifications only)
4. **Contact Groups**: Organized collections of contacts/users for targeted communications
5. **Addressee Types**: Four distinct methods to specify recipients across the system
6. **Statistics Tracking**: Monitor how many entities/users have read each announcement

[Source: docs/prd/functions-of-the-communication-module.md]

---

## User Stories - Bulletin Board

### Story 7.1: Create Announcement (UKNF)

**As a** UKNF Employee with appropriate permissions  
**I want to** create announcements to publish on the bulletin board  
**So that** I can communicate important information to supervised entities

**Acceptance Criteria:**
- Authorized UKNF Employee can access "Create Announcement"
- Employee provides announcement details:
  - **Title** (text, required)
  - **Content** (rich text using WYSIWYG editor, required)
  - **Category** (dropdown, required) - e.g., "General Information", "Events", "Regulatory Changes", "System Updates"
  - **Priority** (dropdown, required) - Low, Medium, High
  - **Expiration Date** (date picker, optional) - announcement auto-expires after this date
  - **File Attachments** (optional, multiple files, follow attachment rules from Epic 5)
- Employee can use WYSIWYG editor with formatting: bold, italic, underline, lists, links, headings
- Employee can save announcement as draft (not published, not visible to external users)
- Employee can publish announcement immediately or schedule for future publication
- System validates all required fields
- Published announcements are immediately visible to designated recipients
- System logs announcement creation with timestamp and employee ID

### Story 7.2: Set Announcement Recipients (UKNF)

**As a** UKNF Employee  
**I want to** define who can see each announcement  
**So that** messages reach the appropriate audience

**Acceptance Criteria:**
- During announcement creation/editing, employee selects recipients using Addressee Types (see Story 7.11)
- Employee can choose:
  - **All External Users**: Every external user can see the announcement
  - **Selected Entities**: Choose specific entities from list
  - **Entity Types**: Choose by entity type (e.g., "Loan institutions", "Insurance companies")
  - **Selected Users**: Choose specific external users
  - **Contact Groups**: Choose contact groups (Story 7.12)
- Employee can combine multiple recipient types (e.g., all entities of type X + specific additional entities)
- Recipients are calculated when announcement is published
- Only designated recipients see the announcement on their bulletin board
- Changing recipients after publication takes effect immediately
- System logs recipient configuration with timestamp

### Story 7.3: Publish Announcement

**As a** UKNF Employee  
**I want to** publish draft announcements  
**So that** they become visible to designated recipients

**Acceptance Criteria:**
- Employee can select draft announcement and choose "Publish"
- System validates all required fields are completed
- System validates recipients are selected
- Upon publishing, announcement becomes visible to designated external users immediately
- Announcement appears on users' bulletin board/start page
- New announcement indicator appears for users (e.g., badge, highlight)
- High-priority announcements are prominently displayed (e.g., at top, different color)
- Users receive email notification if announcement is High priority (optional: configurable)
- System logs publication with timestamp and employee ID

### Story 7.4: View Bulletin Board (External User)

**As an** External User  
**I want to** view announcements on the bulletin board  
**So that** I stay informed about important communications from UKNF

**Acceptance Criteria:**
- User can access "Bulletin Board" or "Announcements" section
- User sees all announcements addressed to them (based on recipient configuration)
- Announcements are displayed with: Title, Category, Priority, Publication date, Excerpt
- New/unread announcements are marked with indicator (e.g., "NEW", bold text, badge)
- Announcements are sorted by: Priority (High first), then Publication date (newest first)
- User can filter by: Category, Priority, Read/Unread status
- User can search by: Title, Content
- Bulletin board displays on start page/homepage with recent announcements
- Expired announcements are automatically hidden from view
- User can access archive view to see expired announcements

### Story 7.5: Read Announcement Details (External User)

**As an** External User  
**I want to** read full announcement content  
**So that** I can understand the information being communicated

**Acceptance Criteria:**
- User clicks announcement to view full details
- Details page displays: Full title, Full content (with formatting), Category, Priority, Publication date, Expiration date, Attachments
- User can download any attached files
- Rich text content renders properly (formatting, links, etc.)
- Announcement is marked as "read" for this user
- Read status is tracked in system for statistics
- For High-priority announcements, user is prompted to confirm reading (Story 7.6)
- User can navigate back to bulletin board

### Story 7.6: Confirm Reading High-Priority Announcements

**As an** External User  
**I want to** confirm I have read high-priority announcements  
**So that** UKNF knows I am aware of critical information

**Acceptance Criteria:**
- When user opens High-priority announcement, system presents "Confirm Reading" button
- User must click confirmation to acknowledge reading
- Confirmation records: User ID, Announcement ID, Entity represented (if user represents multiple), Timestamp
- Confirmation displays user's name and entity name in confirmation record
- User cannot dismiss High-priority announcement without confirming (or can dismiss but it remains flagged as unread)
- Confirmation is logged in system
- UKNF Employees can view who confirmed reading (Story 7.9)

### Story 7.7: Edit Announcement (UKNF)

**As a** UKNF Employee  
**I want to** edit published announcements  
**So that** I can correct errors or update information

**Acceptance Criteria:**
- Employee can select any announcement (draft or published) and choose "Edit"
- Employee can modify: Title, Content, Category, Priority, Expiration date, Attachments, Recipients
- Changes to published announcements take effect immediately
- Users who already read announcement see "Updated" indicator
- If priority changes to High and announcement was previously Medium/Low, users are re-prompted for read confirmation
- System tracks edit history with timestamps and employee IDs
- Edit action is logged

### Story 7.8: Remove/Expire Announcement (UKNF)

**As a** UKNF Employee  
**I want to** remove announcements from publication  
**So that** outdated information is not visible to users

**Acceptance Criteria:**
- Employee can select announcement and choose:
  - **Remove from publication**: Announcement immediately hidden from external users
  - **Delete**: Announcement removed entirely (soft delete recommended)
  - **Set expiration date**: Announcement auto-expires on specified date
- System prompts confirmation for deletion
- Removed/expired announcements no longer appear on bulletin board
- Removed announcements can be accessed in "Removed Announcements" admin section
- Deleted announcements remain in database for audit but are not restorable
- System logs removal/deletion with timestamp and employee ID

### Story 7.9: Monitor Announcement Readership (UKNF)

**As a** UKNF Employee  
**I want to** see statistics on who has read announcements  
**So that** I can ensure important communications are being received

**Acceptance Criteria:**
- Employee can view announcement readership statistics:
  - **Total recipients**: Number of users/entities announcement was addressed to
  - **Read count**: Number of users who viewed announcement
  - **Unread count**: Number of users who haven't viewed
  - **Read percentage**: Visual indicator (e.g., "71/100 entities read")
  - **Confirmation count** (for High-priority): Number of users who confirmed reading
- Employee can view list of users who read announcement (names, entities, read timestamps)
- Employee can view list of users who haven't read announcement (names, entities)
- Employee can send reminder to users who haven't read (create message or new notification)
- Statistics update in real-time as users read announcements
- Employee can export readership report (CSV)

### Story 7.10: View Announcement Change History (UKNF)

**As a** UKNF Employee  
**I want to** view complete change history for announcements  
**So that** I can understand all modifications made

**Acceptance Criteria:**
- Employee can access "Change History" for any announcement
- History shows: Timestamp, Action, Changed by (employee name), Previous value, New value
- History includes:
  - Creation (initial values)
  - All edits (field-by-field changes)
  - Publication actions
  - Recipient changes
  - Removal/deletion
- History is chronologically ordered (newest first)
- History is read-only
- History can be exported (PDF or CSV)

---

## User Stories - Contact Management

### Story 7.11: Understand Addressee Types

**As a** UKNF Employee  
**I want to** understand the four ways to specify recipients  
**So that** I can effectively target communications

**Acceptance Criteria:**
- System provides four Addressee Types throughout the platform (announcements, mass messaging, file sharing):

| Type | Description |
|------|-------------|
| **Selected Entities** | Addressee is any external user associated with selected entities from list |
| **Selected Contact Groups** | Addressee is each external user or contact assigned to selected Contact Group(s) |
| **Selected Types of Entities** | Addressee is any external user whose entity is associated with entity type (e.g., "Loan institution") |
| **Selected Users** | Addressee is specific external user(s) selected from list |

- UI provides intuitive selection interface for each type
- Employee can preview recipient list before confirming
- System calculates actual recipients based on addressee configuration
- Recipient calculation is dynamic (reflects current user/entity/group memberships)

### Story 7.12: Create and Manage Contact Groups

**As a** UKNF Employee  
**I want to** create and manage contact groups  
**So that** I can organize recipients for efficient targeted communication

**Acceptance Criteria:**
- Employee can create new Contact Group with:
  - **Group Name** (text, required, unique)
  - **Description** (text area, optional)
  - **Group Type** (optional) - e.g., "Industry Sector", "Communication List", "Report Recipients"
- Employee can add members to Contact Group:
  - External users (system users)
  - Contacts (non-system users, Story 7.13)
- Employee can remove members from Contact Group
- Employee can edit Group Name and Description
- Employee can delete Contact Group (only if not used in active communications)
- Employee can view list of all Contact Groups with member counts
- Employee can search/filter Contact Groups by name or type
- System logs all Contact Group operations with timestamps

### Story 7.13: Add and Manage Contacts (Non-System Users)

**As a** UKNF Employee  
**I want to** add contacts who are not system users  
**So that** I can send email notifications to them via contact groups

**Acceptance Criteria:**
- Employee can create new Contact with:
  - **First Name** (text, required)
  - **Last Name** (text, required)
  - **Email Address** (email, required, validated)
  - **Phone Number** (text, optional, validated)
  - **Organization/Entity** (text or dropdown, optional)
  - **Role/Title** (text, optional)
  - **Notes** (text area, optional)
- Email address must be unique among contacts
- Contacts do NOT have system user accounts (cannot log in)
- Contacts can receive email notifications when part of Contact Groups used in announcements or communications
- Employee can edit contact information
- Employee can delete contacts (only if not member of any active Contact Group)
- Employee can view list of all contacts
- Employee can search/filter contacts by name, email, organization
- System logs all contact operations with timestamps

### Story 7.14: Assign Contacts to Contact Groups

**As a** UKNF Employee  
**I want to** assign contacts and users to contact groups  
**So that** I can use groups for targeted communications

**Acceptance Criteria:**
- Employee can select Contact Group and add members:
  - External users (from system user list)
  - Contacts (from contacts list)
- Employee can add multiple members at once (bulk add)
- Employee can remove members from Contact Group
- Employee can view all members of a Contact Group
- Members list displays: Name, Email, Type (System User / Contact), Organization
- Employee can search members within group
- System prevents duplicate memberships (same person added twice)
- System logs membership changes with timestamps

### Story 7.15: Use Contact Groups for Recipient Selection

**As a** UKNF Employee  
**I want to** use contact groups when selecting recipients for announcements, messages, and file sharing  
**So that** I can efficiently target communications

**Acceptance Criteria:**
- When selecting recipients for announcements (Story 7.2), mass messages (Epic 5), or file permissions (Epic 6), employee can choose "Selected Contact Groups"
- Employee selects one or more Contact Groups from list
- System automatically includes all members of selected groups as recipients
- System users in group receive in-app notifications and emails
- Contacts (non-system users) in group receive email notifications only
- Employee can preview calculated recipient list showing all users and contacts
- Recipient calculation is dynamic (reflects current group membership at time of action)
- System logs use of Contact Groups in communications

### Story 7.16: View Contact Group Usage

**As a** UKNF Employee  
**I want to** see where Contact Groups are being used  
**So that** I can understand impact before modifying groups

**Acceptance Criteria:**
- Employee can view Contact Group details including usage:
  - Active announcements using this group
  - File permissions using this group
  - Scheduled communications using this group
- System warns employee before deleting group if it's actively used
- Employee can view history of when group was used in communications
- Usage report can be exported (CSV)

---

## Technical Considerations

### Bulletin Board Data Model

Announcements should capture:
- Announcement ID
- Title
- Content (rich text HTML)
- Category
- Priority (Low, Medium, High)
- Publication Date
- Expiration Date (optional)
- Created By (UKNF Employee user ID)
- Created Date
- Last Modified Date
- Last Modified By (UKNF Employee user ID)
- Status (Draft, Published, Removed, Deleted)
- Attachments (references to files)
- Recipients Configuration (addressee types and selections)

### Read Tracking

- Track individual user reads: User ID, Announcement ID, Read Timestamp
- Track read confirmations (High-priority): User ID, Announcement ID, Entity ID, Confirmation Timestamp
- Efficient queries for read statistics (consider denormalized counts)

### Contact Management Data Model

**Contact Groups:**
- Group ID
- Group Name (unique)
- Description
- Group Type (optional)
- Created Date
- Created By (UKNF Employee user ID)

**Contacts:**
- Contact ID
- First Name
- Last Name
- Email (unique)
- Phone Number
- Organization/Entity
- Role/Title
- Notes
- Created Date
- Created By (UKNF Employee user ID)

**Contact Group Memberships:**
- Membership ID
- Group ID
- Member Type (System User / Contact)
- Member ID (User ID or Contact ID)
- Added Date
- Added By (UKNF Employee user ID)

### Addressee Type Implementation

- Create reusable component/service for recipient calculation
- Support all four addressee types consistently across modules
- Cache calculated recipients for performance (invalidate on changes)
- Provide recipient preview function for UI

### WYSIWYG Editor

- Use established rich text editor library (e.g., TinyMCE, CKEditor, Quill)
- Sanitize HTML input to prevent XSS attacks
- Support formatting: bold, italic, underline, lists, links, headings, alignment
- Limit or disable potentially dangerous HTML elements

### Performance Considerations

- Bulletin board queries should be efficient (indexed by publication date, priority, expiration)
- Read tracking should not slow down announcement viewing
- Recipient calculation should be optimized for large groups
- Consider caching announcement lists for external users

### Security Requirements

- Only authorized UKNF Employees can create/edit announcements and manage contacts
- External users can only read announcements, never modify
- Validate all HTML input from WYSIWYG editor
- File attachments follow same security rules as Epic 5 (virus scanning, format validation)
- Contact email addresses must be validated
- Prevent email injection attacks in notifications

### Audit Requirements

[Source: docs/prd/b-specification-of-non-functional-requirements.md#22-audit-and-login]

- Log all announcement creations, edits, publications, removals
- Log all read tracking events (announcement views, confirmations)
- Log all Contact Group operations (create, edit, delete, membership changes)
- Log all Contact operations (create, edit, delete)
- Include timestamps and user IDs for all actions

---

## Dependencies

### Prerequisites
- **Epic 1**: Authentication (authenticated users)
- **Epic 2**: Authorization (external users and entities)
- **Email service**: For notifications to contacts

### Integrates With
- **Epic 5**: Messaging & Cases (Contact Groups can be used for mass messaging)
- **Epic 6**: Library (Contact Groups can be used for file permissions)

### Blocks
- Can be developed after Epic 1-2 are complete
- Enhances Epic 5 and 6 but not blocking them

---

## Definition of Done

- [ ] UKNF Employees can create announcements with rich text content
- [ ] UKNF Employees can set announcement recipients using addressee types
- [ ] UKNF Employees can publish, edit, and remove announcements
- [ ] External users can view bulletin board with announcements addressed to them
- [ ] External users can read full announcement details
- [ ] External users must confirm reading High-priority announcements
- [ ] UKNF Employees can monitor announcement readership statistics
- [ ] UKNF Employees can view announcement change history
- [ ] UKNF Employees can create and manage Contact Groups
- [ ] UKNF Employees can add and manage Contacts (non-system users)
- [ ] UKNF Employees can assign users and contacts to Contact Groups
- [ ] Contact Groups can be used for recipient selection in announcements, messages, file sharing
- [ ] UKNF Employees can view Contact Group usage
- [ ] All four Addressee Types work consistently across modules
- [ ] WYSIWYG editor works properly with safe HTML sanitization
- [ ] File attachments are supported and secure
- [ ] Announcement expiration works automatically
- [ ] Email notifications are sent to contacts when used in communications
- [ ] All bulletin board and contact operations are logged for audit
- [ ] Unit tests cover announcement workflows and contact management
- [ ] Integration tests verify read tracking and recipient calculation
- [ ] Security testing confirms HTML sanitization and authorization
- [ ] Performance testing confirms efficient queries for large datasets
- [ ] Documentation includes addressee type definitions and contact group usage

---

## Related PRD Sections

- [Functions of the Communication Module - Handling Messages in the Form of a Bulletin Board](./functions-of-the-communication-module.md#handling-messages-in-the-form-of-a-bulletin-board-ensures)
- [Functions of the Communication Module - Support for Addressees, Contact Groups and Contacts](./functions-of-the-communication-module.md#support-for-addressees-contact-groups-and-contacts-provides)
- [Preferred Functionalities](./preferred-functionalities.md)

---

## Notes

- Bulletin Board is a broadcast mechanism (one-to-many), different from two-way messaging (Epic 5)
- Contact Groups provide reusable recipient targeting across the platform
- Non-system Contacts are important for UKNF to reach people outside the system
- Read confirmation for High-priority announcements ensures critical messages are acknowledged
- WYSIWYG editor must balance functionality with security (sanitize HTML carefully)
- Addressee Types are used across multiple epics - design as reusable component
- Consider implementing announcement templates for common announcement types
- Expiration should be automatic (background job or query-time filtering)
- Statistics provide valuable feedback on communication effectiveness
- Contact management could be a separate epic but is small enough to combine with bulletin board

