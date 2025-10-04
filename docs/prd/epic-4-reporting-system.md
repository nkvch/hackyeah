# Epic 4: Reporting System

**Epic Number:** 4  
**Phase:** Core Communication Features (Preferred Functionalities)  
**Priority:** Highest (Core platform value)  
**Status:** Draft

---

## Epic Goal

Implement a comprehensive reporting system that enables supervised entities to submit regulatory reports in Excel format, provides automated validation through an external service, delivers feedback on validation status, and allows UKNF employees to organize and monitor submitted reports.

---

## Epic Description

### Context

The primary function of the Communication Platform is to enable supervised entities to submit regulatory reports to UKNF (Polish Financial Supervision Authority). This epic implements the complete report submission, validation, and management workflow.

[Source: docs/prd/functions-of-the-communication-module.md - "Report acquisition services"]

### What This Epic Delivers

This epic implements the core reporting functionality including:
- **Report Submission**: Upload Excel (.xlsx) files with automatic validation
- **External Validation Integration**: Integration with external validation service
- **Validation Feedback**: Display validation results, errors, and status
- **Report Corrections**: Submit corrections linked to original reports
- **Report Organization**: Categorize reports into registers (Quarterly, Annual, Current, Archival)
- **Report Monitoring**: Track reports, filter by entity and status, view metadata
- **Reporting Calendar**: Inform entities about upcoming deadlines and track completeness
- **Reporting Communication**: Integrate messaging for report-related questions

### Key Business Rules

1. **Report Templates**: Entities download report templates from the Library (Epic 6), fill them in Excel, and upload completed files
2. **Automatic Validation**: Upon upload, reports are automatically validated by an external service
3. **Validation Timeout**: If validation doesn't complete within 24 hours, status is set to "Error - Exceeded time"
4. **Successful Report**: Only reports with status "Successful validation process" are considered officially submitted
5. **Corrections**: Corrections must be linked to the original report they're correcting
6. **UKNF Challenges**: UKNF Employees can manually challenge reports if substantive issues are found
7. **Report Registers**: Reports are organized in registers (e.g., Quarterly, Annual) with current and archival sections

[Source: docs/prd/functions-of-the-communication-module.md]

---

## User Stories

### Story 4.1: Submit Report

**As an** Employee of a Supervised Entity with Reporting permissions  
**I want to** submit a regulatory report by uploading an Excel file  
**So that** I can fulfill reporting obligations to UKNF

**Acceptance Criteria:**
- User can access "Submit Report" functionality (requires Reporting permission from Epic 2)
- User can select entity context if representing multiple entities
- User can upload file in XLSX format only
- System rejects non-XLSX files with clear error message
- System validates file size (define maximum, e.g., 100 MB)
- Upon successful upload, report status is set to "Working"
- System immediately initiates validation process
- Report status changes to "Transmitted" with unique ID confirmation
- User receives confirmation message with report ID and initial status
- User can view the report in their reports list
- System captures metadata: Entity, Submitter (name, email, phone), Upload timestamp
- System logs report submission with timestamp and user ID

### Story 4.2: External Validation Integration

**As the** system  
**I want to** send uploaded reports to the external validation service  
**So that** reports can be checked for technical and substantive compliance

**Acceptance Criteria:**
- System sends report file to external validation service API immediately after upload
- Report status changes from "Working" to "Transmitted" when sent successfully
- System receives unique validation ID from external service
- Report status changes to "Ongoing" when validation processing begins
- System polls external service or receives webhooks for validation progress
- System handles validation service errors gracefully (connection issues, service downtime)
- If validation doesn't complete within 24 hours, status changes to "Error - Exceeded time"
- System stores validation service response/results
- System logs all interactions with validation service

### Story 4.3: Display Validation Results

**As an** Employee of a Supervised Entity or UKNF Employee  
**I want to** view the validation results for a submitted report  
**So that** I can understand if the report was accepted or needs corrections

**Acceptance Criteria:**
- User can view report details including current validation status
- System displays appropriate status from the validation statuses table (see below)
- For "Successful validation process" status, system shows success message and timestamp
- For "Errors from validation rules" status, system displays:
  - List of validation errors with descriptions
  - Line/field references where errors occurred
  - Suggested corrections (if provided by validator)
- For "Technical error in validation process", system displays technical error details
- For "Error - Exceeded time", system displays timeout message
- For "Contested by UKNF", system displays UKNF's description of irregularities
- System attaches validation result file (if provided by external service) for download
- Validation result file includes: UKNF markings, receipt date, validation date, entity name
- System displays validation history if report was resubmitted

### Story 4.4: Submit Report Correction

**As an** Employee of a Supervised Entity  
**I want to** submit a correction to a previously submitted report  
**So that** I can fix errors identified during validation

**Acceptance Criteria:**
- User can select an existing report and choose "Submit Correction"
- User can upload corrected XLSX file
- System creates a new report entry linked to original report
- Correction report goes through same validation process
- System displays relationship between original and correction reports
- Both original and correction are visible in report history
- Report details view shows if a correction has been submitted
- Original report maintains its status (not modified by correction)
- User can submit multiple corrections if needed (each linked to original)
- System logs correction submission with reference to original report

### Story 4.5: View Reports List (External User)

**As an** Employee of a Supervised Entity  
**I want to** view all reports submitted by my entity  
**So that** I can monitor submission status and history

**Acceptance Criteria:**
- User can view list of all reports for their selected entity
- List displays: Report name, Reporting period, Submission date, Submitter name, Validation status, Corrections indicator
- List is sorted by submission date (newest first) by default
- User can sort by: Submission date, Validation status, Reporting period
- User can filter by: Validation status, Reporting period, Report type
- User can search by report name or submitter name
- User can click report to view detailed information
- List supports pagination for large datasets
- User can export report list (CSV or Excel)

### Story 4.6: View Reports List (UKNF Employee)

**As a** UKNF Employee  
**I want to** view reports organized in registers with filtering capabilities  
**So that** I can efficiently monitor and manage regulatory reporting

**Acceptance Criteria:**
- UKNF Employee can view reports in organized registers (e.g., "Quarterly Reports", "Annual Reports")
- Quick filter "My Entities" shows only reports from entities assigned to the employee
- Employee can filter by: Validation status, Reporting period, Entity, Report type, Archival status
- List displays: Entity name, Report name, Reporting period, Submitter (name, email, phone), Validation status, Corrections indicator, Submission date
- Employee can sort by any column
- Employee can view both current and archival reports
- Employee can search by entity name, report name, or submitter details
- List supports pagination
- Employee can export filtered report list (CSV or Excel)

### Story 4.7: View Report Details

**As a** UKNF Employee or External User  
**I want to** view complete information about a specific report  
**So that** I can understand all relevant details and metadata

**Acceptance Criteria:**
- User can view comprehensive report details including:
  - Report file (download link)
  - Report name and number
  - Entity submitting
  - Person submitting (name, surname, email, telephone)
  - Reporting period
  - Submission timestamp
  - Validation status with status history
  - Validation result file (if available)
  - Corrections indicator (if corrections were submitted)
  - List of corrections with links (if any)
  - Messages related to this report (if any)
  - Metadata fields extracted from file (if applicable)
- User can download original report file
- User can download validation result file
- User can navigate to linked corrections
- User can access messaging for this report (if implemented in Epic 5)

### Story 4.8: Organize Reports into Registers

**As a** UKNF Employee  
**I want to** categorize reports into registers based on report type  
**So that** reports are organized logically for review and analysis

**Acceptance Criteria:**
- UKNF Employee can assign reports to registers (e.g., "Quarterly Reports", "Annual Reports")
- Register assignment can be based on metadata from report file
- System supports both automatic categorization (based on rules) and manual assignment
- Employee can create new registers with names and descriptions
- Employee can move reports between registers
- Registers can be viewed as separate filtered views
- Each register shows report count
- Register assignments are logged with timestamp and employee ID

### Story 4.9: Archive Reports

**As a** UKNF Employee  
**I want to** mark reports as archival  
**So that** I can separate current reports from historical ones

**Acceptance Criteria:**
- UKNF Employee can select one or multiple reports
- Employee can use "Archive Report" action
- Archived reports are marked with archival status
- Archived reports remain accessible but are separated in UI
- Default view shows only current (non-archived) reports
- User can toggle view to include or show only archived reports
- Archive action is logged with timestamp and employee ID
- Archiving doesn't modify report data or validation status

### Story 4.10: Challenge Report (UKNF)

**As a** UKNF Employee  
**I want to** challenge a report that passed technical validation but has substantive issues  
**So that** the entity is notified to correct or clarify the report

**Acceptance Criteria:**
- UKNF Employee can select a report and choose "Challenge" action
- Employee must provide "Description of irregularities" (mandatory text field)
- Report validation status changes to "Contested by UKNF"
- Description of irregularities is visible to the entity in report details
- Entity receives email notification that report was challenged
- Challenged status is prominently displayed in report lists and details
- Challenge action is logged with timestamp, employee ID, and description
- Entity can respond by submitting a correction or via messaging

### Story 4.11: Reporting Calendar/Timetable

**As an** Employee of a Supervised Entity  
**I want to** view upcoming reporting deadlines  
**So that** I can submit reports on time

**Acceptance Criteria:**
- User can access "Reporting Calendar" showing upcoming deadlines
- Calendar displays: Report type, Reporting period, Due date, Submission status
- Calendar highlights upcoming deadlines (e.g., within 7 days)
- Calendar shows overdue reports (past due date, not submitted)
- User receives reminders for upcoming deadlines (configurable, e.g., 7 days, 3 days, 1 day before)
- Calendar shows submission status: Not submitted, Submitted, Validated successfully, Errors
- User can filter calendar by report type or period
- Calendar can display in month view or list view

### Story 4.12: Reporting Calendar Management (UKNF)

**As a** UKNF Employee  
**I want to** configure reporting calendar deadlines and monitor compliance  
**So that** I can track which entities have submitted required reports

**Acceptance Criteria:**
- UKNF Employee can create reporting periods with: Report type, Reporting period name, Due date, Applicable entities (all or specific types)
- Employee can view compliance dashboard showing submission progress
- Dashboard displays: Total entities required to submit, Submitted count, Not submitted count, Percentage complete
- Employee can view list of entities that haven't submitted for a specific period
- Employee can send reminder messages to non-submitting entities (integration with Epic 5)
- Employee can generate and send message to all non-submitters at once
- Employee can export compliance report (CSV or Excel)

### Story 4.13: Report Status Tracking

**As an** Employee of Supervised Entity or UKNF Employee  
**I want to** track the status changes of a report over time  
**So that** I can understand the complete lifecycle of the report

**Acceptance Criteria:**
- System maintains complete status history for each report
- User can view status history showing: Timestamp, Previous status, New status, Triggered by (user or system)
- Status history includes all transitions: Working → Transmitted → Ongoing → Final Status
- Manual status changes (e.g., Challenge by UKNF) show which employee made the change
- Status history is read-only
- Status history can be exported as part of report details

---

## Validation Statuses

[Source: docs/prd/functions-of-the-communication-module.md]

| Status | Description |
|--------|-------------|
| **Working** | Transitional status set after adding a report file |
| **Transmitted** | Transitional status set after validation process starts. Confirmed with unique ID. |
| **Ongoing** | Transitional status when report processing is in progress |
| **Successful validation process** | Report processing successful with no validation errors. Report recorded and data will be analyzed. |
| **Errors from validation rules** | Report processing completed but validation errors were detected |
| **Technical error in validation process** | Validation process encountered an error |
| **Error - Exceeded time** | Automatically set when validation doesn't complete within 24 hours |
| **Contested by UKNF** | Set manually by UKNF Employee using "Challenge" action with description of irregularities |

---

## Technical Considerations

### File Handling

[Source: docs/prd/b-specification-of-non-functional-requirements.md#1-file-management]

- Accept only XLSX format (Microsoft Excel)
- Validate file type by content, not just extension (prevent malicious files)
- Define maximum file size (recommend 100 MB)
- Store files securely with virus scanning
- Ensure file storage is backed up
- Consider file retention policies for archival reports

### External Validation Service Integration

- Define API contract with external validation service
- Handle async validation (polling or webhooks)
- Implement retry logic for service failures
- Set 24-hour timeout for validation completion
- Store all validation service responses
- Log all API interactions for troubleshooting

### Report Metadata

Reports should capture:
- Entity ID and name
- Submitter user ID, name, email, phone
- Report file name
- Report type/category (extracted from file or metadata)
- Reporting period
- Submission timestamp
- Validation status and timestamps
- Validation result file reference
- Link to original report (for corrections)
- Register assignment
- Archival status

### Performance Considerations

[Source: docs/prd/b-specification-of-non-functional-requirements.md#3-performance-and-scalability]

- File uploads should support large files efficiently
- Report lists should paginate to handle thousands of reports
- Validation service integration should not block user experience
- Consider background jobs for validation processing
- Database indexes on entity, status, reporting period, submission date

### Security Requirements

- Only users with Reporting permission can submit reports
- Users can only view reports for their entities (external users)
- UKNF Employees can view all reports (or filtered by "My Entities")
- File downloads should be authenticated and authorized
- Audit all report submissions, status changes, and access

### Audit Requirements

[Source: docs/prd/b-specification-of-non-functional-requirements.md#22-audit-and-login]

- Log all report submissions with user and entity
- Log all validation status changes
- Log all report views and file downloads
- Log all challenge actions by UKNF
- Log all archive actions
- Include timestamps and user IDs for all actions

---

## Dependencies

### Prerequisites
- **Epic 1**: Authentication (authenticated users)
- **Epic 2**: Authorization (Reporting permission)
- **Epic 6**: Library (for report templates) - can be implemented in parallel
- **External Validation Service**: Must be available and API documented

### Integrates With
- **Epic 5**: Messaging & Cases (for report-related communication)
- **Epic 6**: Library (report templates stored in Library)

### Blocks
- Can begin development after Epic 1-3 are complete
- Some features (messaging for reports) depend on Epic 5

---

## Definition of Done

- [ ] External users with Reporting permission can submit reports (XLSX files)
- [ ] Reports are automatically sent to external validation service
- [ ] Validation results are retrieved and displayed to users
- [ ] Users can view validation errors and success messages
- [ ] External users can submit corrections linked to original reports
- [ ] External users can view their entity's reports with filtering and sorting
- [ ] UKNF Employees can view all reports organized in registers
- [ ] UKNF Employees can filter reports by "My Entities", status, and period
- [ ] UKNF Employees can view detailed report information including metadata
- [ ] UKNF Employees can organize reports into registers
- [ ] UKNF Employees can archive reports
- [ ] UKNF Employees can challenge reports with description of irregularities
- [ ] Reporting calendar displays upcoming deadlines to external users
- [ ] UKNF Employees can configure reporting calendar and monitor compliance
- [ ] Report status history is tracked and viewable
- [ ] All file operations are secure and validated
- [ ] 24-hour validation timeout is enforced
- [ ] All reporting events are logged for audit
- [ ] Unit tests cover report submission and status management
- [ ] Integration tests verify external validation service integration
- [ ] Security testing confirms proper authorization enforcement
- [ ] Performance testing confirms system handles large files and many reports
- [ ] Documentation includes API specifications and validation status definitions

---

## Related PRD Sections

- [Functions of the Communication Module - Report Acquisition](./functions-of-the-communication-module.md#description-of-the-function)
- [Functions of the Communication Module - Validation Statuses](./functions-of-the-communication-module.md#examples-of-validation-statuses)
- [B. Specification of Non-Functional Requirements - File Management](./b-specification-of-non-functional-requirements.md#1-file-management)
- [B. Specification of Non-Functional Requirements - Performance](./b-specification-of-non-functional-requirements.md#3-performance-and-scalability)
- [Preferred Functionalities](./preferred-functionalities.md)

---

## Notes

- This is the HIGHEST PRIORITY epic as reporting is the core platform function
- External validation service integration is critical - early API contract definition is essential
- Sample files referenced in PRD: `G. RIP100000_Q1_2025.xlsx` (correct), `G. RIP100000_Q2_2025.xlsx` (incorrect) - request these from stakeholders for testing
- Report corrections create a chain of reports - ensure UI clearly shows this relationship
- Consider implementing "Draft" reports that can be saved before submission for complex reports
- Reporting calendar might be a separate story or sub-epic depending on complexity
- Integration with Epic 5 (Messaging) should be designed but messaging features can be added incrementally

