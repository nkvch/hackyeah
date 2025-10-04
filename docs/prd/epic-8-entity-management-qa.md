# Epic 8: Entity Management & Q&A

**Epic Number:** 8  
**Phase:** Advanced Communication Features (Preferred Functionalities)  
**Priority:** Medium (Supporting features for data management and self-service)  
**Status:** Draft

---

## Epic Goal

Implement entity data management capabilities that allow UKNF to maintain information about supervised entities within the system, integrate with the external Entity Database for updates, and provide a Q&A knowledge base where external users can ask questions and view answers in a searchable FAQ format.

---

## Epic Description

### Context

The Communication Platform needs to maintain accurate, current information about supervised entities. Additionally, external users need a self-service mechanism to ask questions and access answers without requiring individual case creation or messaging.

[Source: docs/prd/functions-of-the-communication-module.md - "Handling the file of entities" and "Maintaining the Q&A database"]

### What This Epic Delivers

This epic implements two distinct but complementary features:

**1. Entity Management:**
- Entity File: Maintain detailed entity information within the system
- CRUD Operations: Add, view, edit entity data by UKNF Employees and System Administrators
- Entity Data Updater Service: Integration with external UKNF Entity Database for data synchronization
- Entity Data Verification: External users can view and report discrepancies
- Change History: Track all modifications to entity data
- Periodic Verification Alerts: Prompt external users to confirm current entity data

**2. Q&A Knowledge Base:**
- Question Submission: External users can anonymously ask questions
- Answer Management: UKNF Employees can respond to and manage questions
- FAQ Browsing: All users can view published Q&A pairs
- Categorization and Tagging: Organize questions for discoverability
- Search and Filtering: Find relevant Q&A by keywords, categories, tags
- Rating System: Users can rate helpfulness of answers

### Key Business Rules

1. **Entity Data Source**: System maintains entity data synchronized with external UKNF Entity Database
2. **Entity Administrator Access**: Entity Administrators and Employees can view their entity data
3. **Change Verification**: External users can report data changes via automatic case creation (category: "Change of registration data")
4. **Data Versioning**: Entity data changes are versioned with history
5. **Q&A Anonymity**: Questions are submitted anonymously (user identity not revealed in published Q&A)
6. **Q&A Moderation**: UKNF Employees manage questions and decide which to publish with answers
7. **Periodic Verification**: System periodically prompts external users to verify entity data accuracy

[Source: docs/prd/functions-of-the-communication-module.md]

---

## User Stories - Entity Management

### Story 8.1: View Entity File (UKNF)

**As a** UKNF Employee or System Administrator  
**I want to** view complete entity information  
**So that** I can access accurate details about supervised entities

**Acceptance Criteria:**
- UKNF Employee/System Administrator can access "Entity File" or "Entity Directory"
- Employee can view list of all entities with: Entity name, UKNF code, Entity type, Status, NIP
- Employee can search entities by: Name, UKNF code, NIP, KRS
- Employee can filter entities by: Entity type, Status, Sector
- Employee can click entity to view complete details
- Detail view displays all entity fields per schema (Story 8.2)
- Employee can view entity change history
- Employee can view list of users assigned to entity
- List supports pagination for large entity counts

### Story 8.2: Add/Edit Entity Data (UKNF)

**As a** System Administrator or UKNF Employee  
**I want to** add or edit entity information  
**So that** entity data in the system is current and accurate

**Acceptance Criteria:**
- System Administrator can add new entity with all fields
- UKNF Employee can edit existing entity data
- Entity data fields per PRD specification:

| Field | Type | Description |
|-------|------|-------------|
| ID | bigint | System-generated unique identifier |
| Type of entity | nvarchar(250) | Role in Entity Database (e.g., loan institution) |
| UKNF code | nvarchar(250) | Code from Entity Database, non-editable |
| Name of the entity | nvarchar(500) | |
| LEI | nvarchar(20) | Legal Entity Identifier |
| NIP | nvarchar(10) | Tax ID |
| KRS | nvarchar(10) | National Court Register number |
| Street | nvarchar(250) | Address line |
| Building number | nvarchar(250) | |
| Number of the premises | nvarchar(250) | Apartment/suite number |
| Postcode | nvarchar(250) | |
| City | nvarchar(250) | |
| Telephone | nvarchar(250) | Validated international format `/^\+(?:[0-9] ?){6,14}[0-9]$/` |
| E-mail | nvarchar(500) | Validated email format |
| UKNF registration number | nvarchar(100) | |
| Entity status | nvarchar(250) | e.g., Entered, Deleted |
| Category of entity | nvarchar(500) | |
| Sector of the operator | nvarchar(500) | |
| Entity sub-sector | nvarchar(500) | |
| Cross-border entity | bit | Checkbox (yes/no) |
| Date of creation | datetime2(7) | Auto-populated on creation |
| Date of update | datetime2(7) | Auto-updated on modification |

- System validates:
  - Phone number format (international)
  - Email format
  - Required fields are populated
- Changes are saved with timestamp and user ID
- System logs all changes in entity change history
- Changed entity data is versioned
- UKNF code cannot be edited once set (comes from Entity Database)

### Story 8.3: View Entity Details (External User)

**As an** External User (Entity Administrator or Employee)  
**I want to** view my entity's information  
**So that** I can verify data accuracy

**Acceptance Criteria:**
- External user can access "My Entity" or "Entity Information"
- User sees entity details for entities they are assigned to
- If user represents multiple entities, they select which entity to view
- User views read-only entity data (cannot edit directly)
- All entity fields are displayed (per Story 8.2 schema)
- User can view entity change history
- User can see when data was last updated
- User can report discrepancy if data is incorrect (Story 8.5)

### Story 8.4: View Entity Users (UKNF)

**As a** UKNF Employee or System Administrator  
**I want to** see all users assigned to a specific entity  
**So that** I can understand who represents each entity

**Acceptance Criteria:**
- Employee can access "Entity Users" from entity detail page
- System displays all users with permissions for this entity:
  - Entity Administrators (with active/blocked status)
  - Entity Employees (with active/blocked status)
- List shows: User name, Email, Role (Administrator/Employee), Permissions (Reporting, Cases), Status (Active/Blocked)
- Employee can filter by role and status
- Employee can click user to view user details (links to Epic 3 user management)
- List is sortable by name, role, status

### Story 8.5: Report Entity Data Change (External User)

**As an** External User  
**I want to** report that my entity's data is incorrect or has changed  
**So that** UKNF can update the information

**Acceptance Criteria:**
- External user viewing entity details can click "Report Data Change" or "Data is Incorrect"
- System automatically creates a case with:
  - Category: "Change of registration data"
  - Subject: Auto-populated with "Entity data change report for [Entity Name]"
  - Description: User provides details of what needs to be changed
  - Pre-filled entity context
- User can specify which fields need updating
- User can provide correct values
- User can attach supporting documents (e.g., updated registration documents)
- Case follows standard case workflow (Epic 5)
- UKNF Employee receives case notification
- Case creation is logged
- User receives confirmation that report was submitted

### Story 8.6: View Entity Change History

**As a** UKNF Employee, System Administrator, or External User  
**I want to** view history of entity data changes  
**So that** I can understand how entity information evolved over time

**Acceptance Criteria:**
- Users can access "Change History" from entity details page
- History shows: Timestamp, Changed by (employee name or "External verification"), Field changed, Previous value, New value
- History includes all entity field updates
- History shows data updates from Entity Data Updater service (Story 8.7)
- History is chronologically ordered (newest first)
- History is read-only
- UKNF Employees can view full history
- External users can view history for their entity only
- History can be exported (CSV or PDF)

### Story 8.7: Entity Data Updater Service Integration

**As the** system  
**I want to** synchronize entity data with the external UKNF Entity Database  
**So that** entity information remains accurate and up-to-date

**Acceptance Criteria:**
- System integrates with external UKNF Entity Database (Entity Data Updater service)
- Integration supports:
  - **Pull updates**: System periodically fetches entity data updates from Entity Database
  - **Push updates**: System sends verified entity changes back to Entity Database
- System can be configured for update frequency (e.g., nightly, weekly)
- When entity data is updated from Entity Database:
  - System creates new version of entity record
  - Previous version is archived
  - Change is recorded in entity change history
  - Changed by is marked as "Entity Data Updater Service"
- When external user reports data change (Story 8.5) and UKNF verifies:
  - UKNF Employee updates entity in system
  - System pushes verified update to Entity Database (if configured)
- Integration logs all synchronization events
- Integration handles errors gracefully (connection failures, data validation errors)
- System Administrator can view sync logs and status

### Story 8.8: Periodic Entity Data Verification Prompts

**As an** External User  
**I want to** be periodically prompted to verify my entity's data is current  
**So that** UKNF maintains accurate information

**Acceptance Criteria:**
- System periodically prompts external users to verify entity data (configurable frequency, e.g., every 6 months)
- Prompt appears during user login or prominently on dashboard
- Prompt displays: "Please verify your entity information is current"
- User can:
  - **Confirm**: Data is current and accurate (confirmation recorded with timestamp and user ID)
  - **Report Change**: Opens report change workflow (Story 8.5)
  - **Remind Later**: Dismiss prompt, will reappear next login
- Confirmation is logged in entity change history
- UKNF Employees can view which entities have/haven't verified data recently
- System can send reminder emails if entity hasn't verified after X days past due date
- System Administrator can configure verification frequency and reminder settings

---

## User Stories - Q&A Knowledge Base

### Story 8.9: Submit Question (External User)

**As an** External User  
**I want to** anonymously submit a question  
**So that** I can get answers to issues without creating a case

**Acceptance Criteria:**
- User can access "Q&A" or "Ask a Question" section
- User provides:
  - **Title** (text, required, short question summary)
  - **Content** (text area, required, detailed question)
  - **Category** (dropdown, required) - e.g., "Reporting", "System Access", "Technical Issues", "General"
  - **Tags/Labels** (optional, multi-select or free text) - for better categorization
- Question is submitted anonymously (user's identity is not visible in published Q&A)
- System tracks submitter internally for follow-up but doesn't publish identity
- Upon submission, question status is set to "Pending" (not yet published)
- User receives confirmation message with question ID
- UKNF Employee receives notification of new question
- Question is added to UKNF Employee question queue
- System logs question submission with timestamp (internal user ID for tracking)

### Story 8.10: Manage Questions (UKNF)

**As a** UKNF Employee  
**I want to** view, manage, and respond to submitted questions  
**So that** I can build a helpful FAQ knowledge base

**Acceptance Criteria:**
- UKNF Employee can view list of all questions with: Title, Category, Status, Submission date, Rating (if published)
- Employee can filter by: Status (Pending, Answered, Published, Rejected), Category, Date range
- Employee can search by: Title, Content, Tags
- Employee can click question to view full details
- Question details show: Full title, Full content, Category, Tags, Submission date, Status
- Employee can perform actions:
  - **Add Answer**: Provide answer text (rich text editor)
  - **Publish**: Make Q&A pair visible to all users
  - **Edit**: Modify question title/content/category/tags (for clarity or grammar)
  - **Reject**: Mark question as not suitable for FAQ (with reason)
  - **Delete**: Remove question entirely
- Status transitions:
  - Pending → Answered (when answer added) → Published (when published)
  - Pending → Rejected (if not suitable for FAQ)
- All actions are logged with timestamp and employee ID

### Story 8.11: Answer Question (UKNF)

**As a** UKNF Employee  
**I want to** provide answers to submitted questions  
**So that** users can find helpful information

**Acceptance Criteria:**
- Employee selects question and clicks "Add Answer"
- Employee provides answer using rich text editor (formatting, lists, links)
- Employee can preview how Q&A will appear in published FAQ
- Employee can save answer without publishing (status: Answered, not Published)
- Employee can edit answer before publishing
- Upon adding answer, question status changes from "Pending" to "Answered"
- Answer is stored with: Answer text, Created date, Created by (employee ID)
- Answer is not visible to external users until published

### Story 8.12: Publish Q&A

**As a** UKNF Employee  
**I want to** publish answered questions  
**So that** all users can benefit from the knowledge

**Acceptance Criteria:**
- Employee selects answered question and clicks "Publish"
- Question status changes to "Published"
- Q&A pair becomes visible to all users (internal and external) in FAQ section
- Published Q&A displays: Question title, Question content, Category, Tags, Answer, Publication date
- Submitter's identity remains anonymous
- Published Q&A can be edited by UKNF Employees (edits are live)
- Published Q&A can be unpublished (hidden from users) if needed
- Publication is logged with timestamp and employee ID

### Story 8.13: Browse Q&A Knowledge Base (All Users)

**As a** User (External or Internal)  
**I want to** browse published Q&A pairs  
**So that** I can find answers to my questions

**Acceptance Criteria:**
- All users can access "FAQ" or "Q&A Knowledge Base" section
- User sees list of published Q&A pairs with: Question title, Category, Publication date, Rating (average)
- User can view Q&A organized by categories
- User can see most popular/highest-rated Q&A
- User can see recently added Q&A
- User can click Q&A to view full question and answer
- Q&A detail page displays: Full question, Full answer (with formatting), Category, Tags, Publication date, Average rating
- User can rate the answer (Story 8.14)
- User can navigate back to Q&A list
- If user doesn't find answer, prominent "Ask a Question" link is available

### Story 8.14: Search and Filter Q&A

**As a** User  
**I want to** search and filter Q&A  
**So that** I can quickly find relevant answers

**Acceptance Criteria:**
- User can search Q&A by keywords (searches title, content, answer text)
- Search highlights matching terms in results
- User can filter by: Category, Tags
- User can sort results by:
  - Relevance (for search results)
  - Date added (newest/oldest)
  - Popularity (most viewed)
  - Rating (highest/lowest)
- Filter and sort selections are preserved during browsing
- Search results show excerpts with highlighted keywords
- Empty search/filter results show helpful message and suggestion to ask question
- Search supports partial matches and basic stemming (e.g., "report" matches "reporting")

### Story 8.15: Rate Q&A Answers

**As an** External User or UKNF Employee  
**I want to** rate how helpful answers are  
**So that** I can provide feedback and help others find the best answers

**Acceptance Criteria:**
- User viewing published Q&A can provide rating (e.g., 1-5 stars or thumbs up/down)
- Rating is submitted anonymously
- User can rate each Q&A only once (tracked per user)
- User can change their rating
- System calculates and displays average rating for each Q&A
- Average rating is displayed in Q&A lists and detail pages
- UKNF Employees can view rating statistics: Number of ratings, Average rating, Rating distribution
- High-rated Q&A can be featured or highlighted in FAQ
- Low-rated Q&A can alert UKNF to improve answer
- Rating submission is logged (timestamp, user ID internally)

### Story 8.16: Edit Published Q&A (UKNF)

**As a** UKNF Employee  
**I want to** edit published Q&A pairs  
**So that** I can keep information accurate and up-to-date

**Acceptance Criteria:**
- Employee can select published Q&A and click "Edit"
- Employee can modify: Question title, Question content, Answer, Category, Tags
- Changes take effect immediately (live update)
- Users see updated content on next view
- System maintains edit history showing all changes
- Edit is logged with timestamp and employee ID
- If significant changes are made, consider notifying users who previously rated the Q&A

### Story 8.17: Q&A Analytics (UKNF)

**As a** UKNF Employee or System Administrator  
**I want to** view analytics on Q&A usage  
**So that** I can understand what topics are most important to users

**Acceptance Criteria:**
- Employee can access "Q&A Analytics" dashboard showing:
  - Total questions submitted (all time, by period)
  - Questions by category (breakdown)
  - Questions by status (Pending, Answered, Published, Rejected)
  - Most viewed Q&A
  - Highest-rated Q&A
  - Lowest-rated Q&A (need improvement)
  - Average time to answer questions
  - Trending topics (based on recent submissions)
- Analytics can be filtered by date range
- Analytics can be exported (PDF or CSV)
- Dashboard highlights areas needing attention (e.g., many pending questions, low-rated answers)

---

## Technical Considerations

### Entity Data Schema

Implement per specification in Story 8.2. Key points:
- **UKNF code**: Non-editable, comes from Entity Database
- **Phone number**: Validate international format `/^\+(?:[0-9] ?){6,14}[0-9]$/`
- **Email**: Validate email format
- **Dates**: Auto-populate creation date, auto-update modification date
- **Versioning**: Maintain historical versions of entity records
- **Change tracking**: Log all field-level changes

### Entity Data Updater Service Integration

- Define API contract with external UKNF Entity Database
- Support bidirectional sync (pull updates, push verified changes)
- Handle data conflicts (system version vs. database version)
- Implement scheduling for periodic sync (cron jobs or scheduled tasks)
- Log all sync operations
- Provide admin interface to trigger manual sync and view sync status

### Q&A Data Model

**Questions:**
- Question ID
- Title
- Content
- Category
- Tags (comma-separated or separate tags table)
- Status (Pending, Answered, Published, Rejected)
- Submitter User ID (internal tracking, not published)
- Submission Date
- Answer (text, can be null)
- Answered By (UKNF Employee user ID, optional)
- Answered Date (optional)
- Published Date (optional)
- View Count (track popularity)
- Rating Sum, Rating Count (for average calculation)

**Q&A Ratings:**
- Rating ID
- Question ID
- User ID (internal tracking)
- Rating Value (1-5 or binary)
- Rated Date

### Search Implementation

- Full-text search on question title, content, answer text
- Consider search engine (Elasticsearch) or database full-text search features
- Support relevance scoring
- Implement stemming for better matches

### Performance Considerations

- Entity list should paginate for large entity counts
- Q&A list should paginate for many questions
- Search should be efficient (indexed or dedicated search engine)
- Rating aggregation should be cached or use denormalized counts
- Entity change history queries should be optimized

### Security Requirements

- Only UKNF Employees and System Administrators can add/edit entity data
- External users can only view their own entity data (read-only)
- Entity data updates from external users go through case workflow (not direct edit)
- Q&A questions are submitted anonymously but tracked internally
- Only UKNF Employees can manage questions and answers
- All users can view published Q&A
- Audit all entity data changes and Q&A operations

### Audit Requirements

[Source: docs/prd/b-specification-of-non-functional-requirements.md#22-audit-and-login]

- Log all entity data changes (field-level)
- Log entity data syncs with Entity Database
- Log entity verification confirmations
- Log all Q&A submissions
- Log all Q&A answers and publications
- Log all Q&A edits
- Log Q&A ratings (anonymized)
- Include timestamps and user IDs

---

## Dependencies

### Prerequisites
- **Epic 1**: Authentication (authenticated users)
- **Epic 2**: Authorization (external users assigned to entities)
- **Epic 5**: Messaging & Cases (entity data change reports create cases)
- **External UKNF Entity Database**: For Entity Data Updater service integration

### Integrates With
- **Epic 5**: Messaging & Cases (reporting entity data changes creates cases)

### Blocks
- Can be developed after Epic 1, 2, and 5 are complete
- Entity Data Updater service requires coordination with external Entity Database team

---

## Definition of Done

- [ ] UKNF Employees can view, add, and edit entity data
- [ ] Entity data follows defined schema with field validations
- [ ] External users can view their entity data (read-only)
- [ ] External users can report entity data changes (creates case)
- [ ] UKNF Employees can view entity users assigned to each entity
- [ ] Complete entity change history is tracked and viewable
- [ ] Entity Data Updater service integrates with external UKNF Entity Database
- [ ] Bidirectional sync works (pull updates from database, push verified changes)
- [ ] System periodically prompts external users to verify entity data
- [ ] External users can submit questions anonymously
- [ ] UKNF Employees can view, manage, answer, and publish questions
- [ ] All users can browse published Q&A knowledge base
- [ ] Users can search and filter Q&A by keywords, categories, tags
- [ ] Users can sort Q&A by date, popularity, rating
- [ ] Users can rate Q&A answers
- [ ] UKNF Employees can edit published Q&A
- [ ] UKNF Employees can view Q&A analytics
- [ ] All entity and Q&A operations are logged for audit
- [ ] Unit tests cover entity management and Q&A workflows
- [ ] Integration tests verify Entity Data Updater sync
- [ ] Security testing confirms proper authorization enforcement
- [ ] Performance testing confirms efficient queries for large datasets
- [ ] Documentation includes entity schema and Entity Data Updater API contract

---

## Related PRD Sections

- [Functions of the Communication Module - Handling the File of Entities](./functions-of-the-communication-module.md#handling-the-file-of-entities-and-updating-information-on-supervised-entities-ensures)
- [Functions of the Communication Module - Entity Data Updater Service](./functions-of-the-communication-module.md#updating-information-on-supervised-entities-entity-data-updater-service-provides)
- [Functions of the Communication Module - Maintaining the Q&A Database](./functions-of-the-communication-module.md#maintaining-the-qa-database-ensures)
- [Preferred Functionalities](./preferred-functionalities.md)

---

## Notes

- Entity Management and Q&A are distinct features but combined in one epic due to size
- Could be split into two epics if implementation team prefers separate delivery
- Entity Data Updater service integration is critical and requires coordination with external team
- Entity schema is defined in PRD - ensure exact field names and types are implemented
- Q&A anonymity is important for encouraging question submission
- Q&A rating system provides valuable feedback and highlights best content
- Entity verification prompts help maintain data accuracy over time
- Change history for entities is crucial for compliance and audit
- Search functionality for Q&A is essential for user experience
- Consider gamification for Q&A (e.g., most helpful answers, contributor recognition) in future iterations

