# Epic 8 Story Validation Report

**Date:** 2025-10-04  
**Epic:** 8 - Entity Management & Q&A  
**Validated By:** Scrum Master (AI)  
**Validation Status:** ✅ **PASSED WITH RECOMMENDATIONS**

---

## Executive Summary

All 17 stories for Epic 8 have been validated against:
- PRD: `docs/prd/epic-8-entity-management-qa.md`
- PRD: `docs/prd/functions-of-the-communication-module.md`
- PRD: `docs/prd/b-specification-of-non-functional-requirements.md`
- Database Schema: `docs/architecture/section-9-database-schema.md`

**Overall Assessment:** ✅ Stories align with PRD requirements with minor recommendations for enhancement.

---

## Part 1: Entity Management Stories (8.1 - 8.8)

### ✅ Story 8.1: View Entity File (UKNF)

**PRD Requirement:**
> "a preview of detailed data of the entity supervised by the System Administrator and the UKNF Employee"
> "the possibility of displaying in the form of a list of messages addressed to entities by UKNF Employees"

**Validation:** ✅ **PASS**
- ✅ Covers viewing entity list with search/filter
- ✅ Includes entity detail view
- ✅ Shows change history
- ✅ Shows assigned users
- ✅ Proper authorization (UKNF only)

**Database Alignment:**
- ✅ Uses Entities table (bigint ID) - matches schema
- ✅ References EntityHistory table
- ✅ References UserEntities mapping

### ✅ Story 8.2: Add/Edit Entity Data (UKNF)

**PRD Requirement:**
> "adding and modifying data on the entity supervised by the System Administrator and the UKNF Employee, in accordance with the structure"

**Validation:** ✅ **PASS**
- ✅ All 22 entity fields from PRD schema included
- ✅ Phone validation: `/^\+(?:[0-9] ?){6,14}[0-9]$/` - matches PRD
- ✅ Email validation specified
- ✅ UKNF code immutable after creation - matches PRD "uneditable"
- ✅ Change history tracking
- ✅ Versioning support

**Schema Validation:**
| PRD Field | Story 8.2 | Database Schema | Status |
|-----------|-----------|-----------------|--------|
| ID (bigint) | ✅ bigint | ✅ BIGINT IDENTITY | ✅ Match |
| UKNF code | ✅ nvarchar(250) | ✅ UknfCode NVARCHAR(250) | ✅ Match |
| Name | ✅ nvarchar(500) | ✅ Name NVARCHAR(500) | ✅ Match |
| LEI | ✅ nvarchar(20) | ✅ Lei NVARCHAR(20) | ✅ Match |
| NIP | ✅ nvarchar(10) | ✅ Nip NVARCHAR(10) | ✅ Match |
| KRS | ✅ nvarchar(10) | ✅ Krs NVARCHAR(10) | ✅ Match |
| Telephone | ✅ nvarchar(250) | ✅ Phone NVARCHAR(250) | ✅ Match |
| E-mail | ✅ nvarchar(500) | ✅ Email NVARCHAR(500) | ✅ Match |
| Cross-border entity | ✅ bit | ✅ IsCrossBorder BIT | ✅ Match |

**⚠️ Minor Discrepancy:**
- Database uses EntityHistory table with JSON snapshots
- Story 8.2 proposes EntityChangeHistory with field-level tracking
- **Recommendation:** Align on naming convention. EntityHistory (database) vs EntityChangeHistory (story). Suggest using database naming for consistency.

### ✅ Story 8.3: View Entity Details (External User)

**PRD Requirement:**
> "a preview of the details of the entity by external Users assigned to the entity in order to be able to verify the compliance of the data with the facts"

**Validation:** ✅ **PASS**
- ✅ External users can view their entity (read-only)
- ✅ Multiple entity selection if applicable
- ✅ Change history visible
- ✅ Links to report change workflow (Story 8.5)
- ✅ Proper user-entity access check

### ✅ Story 8.4: View Entity Users (UKNF)

**PRD Requirement:**
> "preview of the list of all Users of the Entity assigned in the system to a given Entity Supervised"

**Validation:** ✅ **PASS**
- ✅ Shows Entity Administrators and Employees
- ✅ Displays permissions (Reporting, Cases)
- ✅ Shows status (Active/Blocked)
- ✅ Filter by role and status
- ✅ Links to Epic 3 user management

**Database Alignment:**
- ✅ Uses UserEntities mapping table
- ✅ Uses PermissionLines for permissions

### ✅ Story 8.5: Report Entity Data Change (External User)

**PRD Requirement:**
> "report a change in the registration data (name of the entity, address data, telephone, e-mail) in the event of non-compliance or change in their change, through a case with the category 'Change of registration data'"

**Validation:** ✅ **PASS**
- ✅ Creates case with category "Change of registration data"
- ✅ Auto-populates subject
- ✅ Allows field-by-field change specification
- ✅ Supports attachments
- ✅ Integrates with Epic 5 case system
- ✅ User receives confirmation

**PRD Compliance:**
- ✅ Exactly matches PRD requirement for case-based change reporting
- ✅ Category name matches: "Change of registration data"

### ✅ Story 8.6: View Entity Change History

**PRD Requirement:**
> "reviewing the history of changes in data about the entity"

**Validation:** ✅ **PASS**
- ✅ Chronological order (newest first)
- ✅ Shows: Timestamp, Changed By, Field, Previous/New Value
- ✅ Includes manual changes, external verifications, Entity Data Updater changes
- ✅ Export capability (CSV/PDF)
- ✅ Authorization: UKNF sees all, External sees only their entity

### ✅ Story 8.7: Entity Data Updater Service Integration

**PRD Requirement:**
> "Updating information on supervised entities (entity data updater service) provides: the possibility of updating data on supervised entities (registration data) cooperating with the UKNF contained in the Entities Database"

**Validation:** ✅ **PASS**
- ✅ Pull updates from external Entity Database
- ✅ Push verified changes to Entity Database
- ✅ Versioning and archiving
- ✅ Change source tracking
- ✅ Scheduled and manual sync
- ✅ Error handling
- ✅ Admin monitoring

**PRD Compliance:**
- ✅ "versioning of changes in data about the entity - stored historical data" - covered
- ✅ "verification of updated data by the UKNF Employee and after correct verification recording them in the Entity Database" - covered
- ✅ Bidirectional sync as specified

### ✅ Story 8.8: Periodic Entity Data Verification Prompts

**PRD Requirement:**
> "scheduling, for alerts cyclically appearing during external user access sessions with confirmation of current data of the supervised entity"

**Validation:** ✅ **PASS**
- ✅ Periodic prompts (configurable frequency)
- ✅ During login or on dashboard
- ✅ Actions: Confirm, Report Change, Remind Later
- ✅ Confirmation logged in change history
- ✅ Reminder emails for overdue verifications
- ✅ UKNF can view verification status
- ✅ Configurable settings

---

## Part 2: Q&A Knowledge Base Stories (8.9 - 8.17)

### ✅ Story 8.9: Submit Question (External User)

**PRD Requirement:**
> "Maintaining the Q&A database ensures: Possibility to ask and receive answers to questions and view anonymized questions with answers (so-called. Knowledge base FAQ)"
> "Adding new questions anonymously. Questions should have fields: title, content, category, labels, date added, status."

**Validation:** ✅ **PASS**
- ✅ Anonymous submission (identity tracked internally only)
- ✅ Fields: Title, Content, Category, Tags (labels)
- ✅ Status set to "Pending"
- ✅ UKNF Employee notification
- ✅ Confirmation with question ID

**Database Alignment:**
- ✅ Aligns with FAQs table in schema
- ⚠️ **Naming Discrepancy:** Story uses "Question" entity, Database schema uses "FAQs" table
- **Recommendation:** Rename domain entity from `Question` to `Faq` or update database schema from `FAQs` to `Questions` for consistency

**Schema Comparison:**
| PRD Requirement | Story 8.9 | Database Schema | Status |
|-----------------|-----------|-----------------|--------|
| title | ✅ Title | ✅ Title NVARCHAR(500) | ✅ Match |
| content | ✅ Content | ✅ QuestionContent NVARCHAR(MAX) | ✅ Match |
| category | ✅ Category | ✅ Category NVARCHAR(250) | ✅ Match |
| labels | ✅ Tags | ✅ Tags NVARCHAR(1000) | ✅ Match |
| date added | ✅ SubmittedDate | ✅ CreatedDate | ✅ Match |
| status | ✅ Status enum | ✅ Status (CHECK constraint) | ✅ Match |

**Status Values:**
- PRD: Not explicitly specified
- Story: Pending, Answered, Published, Rejected
- Database: Submitted, Answered, Published, Archived
- ⚠️ **Minor Discrepancy:** "Pending" vs "Submitted", "Rejected" vs "Archived"
- **Recommendation:** Align status values. Suggest: Submitted, Answered, Published, Rejected (Story 8.9 values are more descriptive)

### ✅ Story 8.10: Manage Questions (UKNF)

**PRD Requirement:**
> "management of questions and answers - the possibility of modifying or removing questions/answers, changing the content, status and other fields by the UKNF Employee or the System Administrator"

**Validation:** ✅ **PASS**
- ✅ View all questions with filters
- ✅ Search by title, content, tags
- ✅ Filter by status, category, date
- ✅ Actions: Add Answer, Publish, Edit, Reject, Delete
- ✅ Status transitions defined
- ✅ All actions logged

### ✅ Story 8.11: Answer Question (UKNF)

**PRD Requirement:**
> "adding answers to questions by the UKNF Employee"

**Validation:** ✅ **PASS**
- ✅ Rich text editor (WYSIWYG)
- ✅ Preview functionality
- ✅ Save without publishing
- ✅ Edit answer before publishing
- ✅ HTML sanitization (security)
- ✅ Status change: Pending → Answered

**Database Alignment:**
- ✅ Uses AnswerContent field from FAQs table
- ✅ Tracks AnsweredByUserId
- ✅ Tracks AnsweredDate

### ✅ Story 8.12: Publish Q&A

**PRD Requirement:**
> Implicit in "view anonymized questions with answers" - questions must be published

**Validation:** ✅ **PASS**
- ✅ Publish answered questions
- ✅ Status change: Answered → Published
- ✅ Visible to all users (internal/external)
- ✅ Unpublish capability
- ✅ Edits are live
- ✅ Submitter anonymity preserved

### ✅ Story 8.13: Browse Q&A Knowledge Base (All Users)

**PRD Requirement:**
> "view anonymized questions with answers (so-called. Knowledge base FAQ)"
> "categorisation of questions and answers – grouping into categories"
> "sorting the results by: popularity, date added, ratings"

**Validation:** ✅ **PASS**
- ✅ Public access (no authentication)
- ✅ View by categories
- ✅ Most popular (by views)
- ✅ Recently added
- ✅ Highest rated
- ✅ Q&A detail page with full content
- ✅ Rating display
- ✅ "Ask a Question" CTA

**PRD Compliance:**
- ✅ Categorization ✓
- ✅ Sorting by popularity ✓
- ✅ Sorting by date added ✓
- ✅ Sorting by ratings ✓

### ✅ Story 8.14: Search and Filter Q&A

**PRD Requirement:**
> "search by keyword, title, content, category or label"
> "filtering by category, tags, response status"

**Validation:** ✅ **PASS**
- ✅ Keyword search (title, content, answer)
- ✅ Highlighted search results
- ✅ Filter by category, tags
- ✅ Sort by relevance, date, popularity, rating
- ✅ Partial matches and stemming
- ✅ Excerpts with highlights

**PRD Compliance:**
- ✅ Search by keyword ✓
- ✅ Search by title ✓
- ✅ Search by content ✓
- ✅ Search by category ✓
- ✅ Search by label (tags) ✓
- ✅ Filtering by category ✓
- ✅ Filtering by tags ✓

### ✅ Story 8.15: Rate Q&A Answers

**PRD Requirement:**
> "assessment of responses (e.g. asterisks 1-5)"

**Validation:** ✅ **PASS**
- ✅ 1-5 star rating system (matches PRD "asterisks 1-5")
- ✅ Anonymous rating
- ✅ One rating per user
- ✅ Can update rating
- ✅ Average rating calculation
- ✅ UKNF can view statistics
- ✅ Alerts for low-rated Q&A

**Database Alignment:**
- ✅ FaqRatings table in schema
- ✅ Rating (1-5) with CHECK constraint
- ✅ Unique constraint (FaqId, UserId)
- ✅ AverageRating in FAQs table

### ✅ Story 8.16: Edit Published Q&A (UKNF)

**PRD Requirement:**
> "the possibility of modifying or removing questions/answers, changing the content, status and other fields"

**Validation:** ✅ **PASS**
- ✅ Edit title, content, answer, category, tags
- ✅ Live updates (immediate)
- ✅ Edit history tracking
- ✅ Logged with timestamp and employee ID
- ✅ Optional notification for significant changes

### ✅ Story 8.17: Q&A Analytics (UKNF)

**PRD Requirement:**
> Not explicitly required in PRD, but valuable for system management

**Validation:** ✅ **PASS (ENHANCEMENT)**
- ✅ Total questions by period
- ✅ Questions by category
- ✅ Questions by status
- ✅ Most viewed, highest/lowest rated
- ✅ Average time to answer
- ✅ Trending topics
- ✅ Date range filtering
- ✅ Export capability

**Assessment:** This story adds significant value beyond PRD requirements for operational insights.

---

## Database Schema Validation

### Entity Management Tables

| Table | PRD Required | Schema Present | Story Coverage |
|-------|--------------|----------------|----------------|
| Entities | ✅ Yes | ✅ Yes | ✅ Stories 8.1, 8.2 |
| EntityHistory | ✅ Yes | ✅ Yes | ✅ Story 8.6 |
| UserEntities | ✅ Yes | ✅ Yes | ✅ Story 8.4 |

**Missing Tables (Need to Add):**
- ❌ **EntityVerifications** - for Story 8.8 (periodic verification)
- ❌ **EntitySyncLogs** - for Story 8.7 (Entity Data Updater logging)
- ❌ **EntityVersions** - for Story 8.7 (version archiving)

### Q&A Tables

| Table | PRD Required | Schema Present | Story Coverage |
|-------|--------------|----------------|----------------|
| FAQs | ✅ Yes | ✅ Yes | ✅ Stories 8.9-8.17 |
| FaqRatings | ✅ Yes | ✅ Yes | ✅ Story 8.15 |

**Missing Tables (Need to Add):**
- ❌ **QuestionEditHistory** - for Story 8.16 (edit history)
- ⚠️ **Note:** Database schema doesn't include field for separate edit history. Can be added or use AuditLogs table.

---

## PRD Coverage Analysis

### ✅ All PRD Requirements Covered

**Entity Management:**
1. ✅ Viewing entity file (UKNF) - Story 8.1
2. ✅ Adding/editing entity data - Story 8.2
3. ✅ Viewing entity details (External) - Story 8.3
4. ✅ Viewing entity users - Story 8.4
5. ✅ Reporting entity changes via case - Story 8.5
6. ✅ Viewing change history - Story 8.6
7. ✅ Entity Data Updater service sync - Story 8.7
8. ✅ Periodic verification prompts - Story 8.8

**Q&A Knowledge Base:**
1. ✅ Anonymous question submission - Story 8.9
2. ✅ Question management (UKNF) - Story 8.10
3. ✅ Answering questions - Story 8.11
4. ✅ Publishing Q&A - Story 8.12
5. ✅ Browsing FAQ - Story 8.13
6. ✅ Search and filter - Story 8.14
7. ✅ Rating answers - Story 8.15
8. ✅ Editing published Q&A - Story 8.16
9. ✅ Analytics (enhancement) - Story 8.17

---

## Issues and Recommendations

### 🔴 Critical Issues

**None identified.** All PRD requirements are covered.

### ⚠️ Medium Priority Issues

1. **Entity Table Naming Discrepancy**
   - **Issue:** Domain entity not yet created, but Story 8.2 proposes creating Entity.cs
   - **Database Schema:** Entities table exists
   - **Status:** Alignment needed
   - **Recommendation:** Domain entity should map to existing Entities table. Update EF Core configuration.

2. **Q&A Entity Naming**
   - **Issue:** Story 8.9 uses "Question" entity name, Database uses "FAQs" table
   - **Recommendation:** Choose one convention:
     - Option A: Rename domain entity to `Faq` (matches database)
     - Option B: Rename database table to `Questions` (matches story)
   - **Preferred:** Option A (keep database schema stable)

3. **Q&A Status Values**
   - **Story 8.9:** Pending, Answered, Published, Rejected
   - **Database Schema:** Submitted, Answered, Published, Archived
   - **Recommendation:** Update database schema CHECK constraint to match Story values (more descriptive)

4. **Missing Database Tables**
   - **EntityVerifications** table needed for Story 8.8
   - **EntitySyncLogs** table needed for Story 8.7
   - **QuestionEditHistory** table needed for Story 8.16 (or use AuditLogs)
   - **Action:** Add migrations for these tables

### 💡 Low Priority Recommendations

1. **Entity Change History Structure**
   - Database has EntityHistory with JSON snapshots
   - Story 8.2 suggests field-level EntityChangeHistory
   - **Recommendation:** Both approaches are valid. JSON is more flexible, field-level is more queryable. Consider hybrid approach.

2. **Phone Number Field Name**
   - **PRD:** "Telephone"
   - **Database:** "Phone"
   - **Stories:** "Telephone"
   - **Recommendation:** Keep database as-is (Phone is more common), update EF mapping to handle both.

3. **Entity Data Updater API Contract**
   - Story 8.7 mentions API contract "to be defined"
   - **Recommendation:** Coordinate with external UKNF Entity Database team early to define:
     - Authentication method
     - API endpoints
     - Data format
     - Rate limits

4. **Q&A WYSIWYG Editor Choice**
   - Story 8.11 suggests Quill, TinyMCE, or CKEditor
   - **Recommendation:** Choose Quill (lightweight, modern, good Angular support via ngx-quill)

5. **Full-Text Search Implementation**
   - Story 8.14 includes PostgreSQL full-text search examples
   - **Recommendation:** Implement database-level search for MVP, consider Elasticsearch for future enhancement

---

## Security Validation

### ✅ All Security Requirements Met

1. ✅ **Authorization:**
   - UKNF role checks for entity management
   - External user entity access restrictions
   - Permission checks for Q&A management

2. ✅ **Data Protection:**
   - UKNF code immutable (Story 8.2)
   - Q&A anonymity preserved (Story 8.9)
   - Change history audit (all stories)

3. ✅ **Input Validation:**
   - Phone format validation (Story 8.2)
   - Email format validation (Story 8.2)
   - HTML sanitization for Q&A (Story 8.11)
   - File upload validation (Story 8.5)

4. ✅ **Audit Logging:**
   - All entity changes logged (Story 8.6)
   - All Q&A operations logged (Stories 8.9-8.17)

---

## Non-Functional Requirements Validation

### ✅ Performance

- ✅ Pagination (Stories 8.1, 8.6, 8.10, 8.13)
- ✅ Indexing strategy specified (Stories 8.1, 8.2, 8.14)
- ✅ Caching mentioned (Story 8.17)
- ✅ Optimized queries (Story 8.14 - full-text search)

### ✅ Scalability

- ✅ Background jobs for Entity Data Updater (Story 8.7)
- ✅ Scheduled tasks for verification reminders (Story 8.8)
- ✅ Event-driven architecture (notifications)

### ✅ Usability

- ✅ User-friendly interfaces described
- ✅ Clear error messages
- ✅ Help text and info boxes
- ✅ Responsive design mentioned

---

## Test Coverage Validation

### ✅ All Stories Include Testing Tasks

**Unit Tests:** ✅ All stories include unit test specifications  
**Integration Tests:** ✅ All stories include integration test specifications  
**E2E Tests:** ⚠️ Not specified but can be added

**Coverage Areas:**
- ✅ Command/Query handlers
- ✅ Validators
- ✅ Domain methods
- ✅ API endpoints
- ✅ Authorization checks
- ✅ Business logic

---

## Dependency Validation

### Prerequisites (All Identified)

- ✅ **Epic 1:** Authentication (all stories)
- ✅ **Epic 2:** Authorization (Stories 8.1, 8.2, 8.4, 8.5)
- ✅ **Epic 5:** Messaging & Cases (Story 8.5)
- ⚠️ **External Entity Database:** Story 8.7 requires external coordination

### Integration Points (All Identified)

- ✅ Story 8.5 → Epic 5 (Case creation)
- ✅ Story 8.4 → Epic 3 (User management)
- ✅ Story 8.7 → External Entity Database API

---

## Final Validation Checklist

| Criterion | Status | Notes |
|-----------|--------|-------|
| **PRD Coverage** | ✅ Complete | All requirements covered |
| **Database Alignment** | ⚠️ Minor Issues | Naming conventions need alignment |
| **Security Requirements** | ✅ Complete | All security measures included |
| **Authorization** | ✅ Complete | Proper role checks |
| **Audit Logging** | ✅ Complete | All actions logged |
| **Test Coverage** | ✅ Complete | Unit & integration tests specified |
| **Dependencies Identified** | ✅ Complete | All dependencies noted |
| **Technical Feasibility** | ✅ Complete | All technically sound |
| **User Experience** | ✅ Complete | Well-designed UX |
| **Performance Considerations** | ✅ Complete | Pagination, caching, indexing |

---

## Recommendations for Implementation

### Before Starting Implementation:

1. **Resolve Naming Conventions:**
   - [ ] Align domain entity names with database tables
   - [ ] Decide on EntityHistory vs EntityChangeHistory
   - [ ] Decide on Question vs Faq entity naming
   - [ ] Align Q&A status values

2. **Add Missing Database Tables:**
   - [ ] EntityVerifications (Story 8.8)
   - [ ] EntitySyncLogs (Story 8.7)
   - [ ] QuestionEditHistory (Story 8.16) or use AuditLogs

3. **External Coordination:**
   - [ ] Define API contract with UKNF Entity Database team (Story 8.7)
   - [ ] Establish authentication mechanism
   - [ ] Define sync frequency and data format

4. **Technology Choices:**
   - [ ] Confirm WYSIWYG editor choice (recommend Quill)
   - [ ] Confirm search implementation (PostgreSQL FTS for MVP)
   - [ ] Confirm file storage service (Local for MVP, Azure/S3 for production)

### During Implementation:

1. **Follow Clean Architecture:**
   - Domain entities in Domain layer
   - CQRS with MediatR in Application layer
   - EF Core in Infrastructure layer
   - Controllers in API layer

2. **Testing:**
   - Write unit tests first (TDD)
   - Integration tests with Testcontainers
   - Aim for 80%+ code coverage

3. **Security:**
   - HTML sanitize all user input
   - Validate file uploads
   - Enforce authorization at API level
   - Log all sensitive operations

4. **Performance:**
   - Add database indexes as specified
   - Implement pagination early
   - Use async/await throughout
   - Cache analytics data (Story 8.17)

---

## Conclusion

**Validation Result:** ✅ **APPROVED FOR IMPLEMENTATION**

All 17 stories for Epic 8 successfully align with PRD requirements and follow established patterns from previous stories. Minor naming convention issues and missing database tables should be addressed before implementation begins.

**Quality Assessment:** **Excellent** - Stories are comprehensive, well-structured, and implementation-ready.

**Next Steps:**
1. ✅ Stories approved - proceed to implementation
2. ⚠️ Resolve naming conventions first
3. ⚠️ Add missing database tables
4. ✅ Coordinate with external Entity Database team
5. ✅ Begin implementation following recommended order

**Recommended Implementation Order:**
1. Stories 8.1, 8.2 (Entity viewing and editing - foundation)
2. Story 8.6 (Change history - needed by others)
3. Stories 8.3, 8.4, 8.5 (External user entity features)
4. Story 8.7 (Entity Data Updater - can be done in parallel)
5. Story 8.8 (Periodic verification - depends on 8.3)
6. Stories 8.9, 8.10, 8.11, 8.12 (Q&A core features)
7. Stories 8.13, 8.14, 8.15 (Q&A browsing and rating)
8. Stories 8.16, 8.17 (Q&A enhancement features)

---

**Validated By:** Scrum Master (AI)  
**Date:** 2025-10-04  
**Signature:** ✅ Approved with recommendations

