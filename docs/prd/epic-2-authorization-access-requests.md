# Epic 2: Authorization & Access Requests

**Epic Number:** 2  
**Phase:** Foundation & Access (Additional Functionalities - Prerequisites)  
**Priority:** High (Prerequisite for communication features)  
**Status:** Draft

---

## Epic Goal

Implement a comprehensive access request and authorization system that enables external users to request permissions for specific supervised entities, allows UKNF employees and Entity Administrators to approve/manage these requests, and provides entity context selection for user sessions.

---

## Epic Description

### Context

After external users register and authenticate (Epic 1), they need to request and receive specific permissions to access system functions for supervised entities. The system must support a structured workflow where:
- External users submit access requests
- Requests are reviewed and approved by authorized personnel
- Users can represent multiple entities and select which entity context they're working in
- Permissions can be managed, updated, and blocked as needed

[Source: docs/prd/functions-of-the-authentication-and-authorization-module.md]

### What This Epic Delivers

This epic implements the complete authorization framework including:
- **Access request workflow** from submission to approval
- **Permission assignment** by UKNF Employees and Entity Administrators
- **Entity context selection** for multi-entity users
- **Communication within access requests** for clarifications
- **Permission management** including blocking and updates
- **Access request status tracking** and history

### Key Business Rules

1. **Automatic Request Generation**: After account activation (registration), an access request is automatically created with status "Working"
2. **Two-Level Approval**: 
   - Entity Administrator requests approved by UKNF Employees
   - Entity Employee requests approved by Entity Administrators (or UKNF if no admin exists)
3. **Entity Administrator Blocking**: Blocking an Entity Administrator doesn't affect previously approved permissions for Entity Employees
4. **Multi-Entity Support**: Users can have permissions for multiple entities and must select context per session

[Source: docs/prd/functions-of-the-authentication-and-authorization-module.md]

---

## User Stories

### Story 2.1: Automatic Access Request Creation

**As a** newly registered external user who activated their account  
**I want to** have an access request automatically created  
**So that** I can begin the process of requesting permissions

**Acceptance Criteria:**
- After successful account activation (Epic 1), system automatically creates an access request
- Access request status is set to "Working"
- Access request pre-populates user data: First name, Last name, PESEL (masked, last 4 digits visible), phone, email
- User can view their access request in "Working" status
- Request is not visible to UKNF Employees until status changes to "New"

### Story 2.2: Complete and Submit Access Request

**As an** external user with a "Working" access request  
**I want to** complete my access request by selecting entities and permissions  
**So that** it can be reviewed and approved by authorized personnel

**Acceptance Criteria:**
- User can edit their "Working" access request
- User can select supervised entities from the UKNF Directory of Entities
- User can request permissions by checking checkboxes for: Reporting, Cases, Entity Administrator
- User can add entity email address (for automatic notifications to that entity)
- User can add attachments to support their request
- All mandatory fields must be completed before submission
- Upon submission, request status changes from "Working" to "New"
- User sees confirmation message: "Your access request has been submitted"
- User receives automatic email confirmation of submission
- Request becomes visible to appropriate approvers (UKNF Employees or Entity Administrators)

### Story 2.3: View and Filter Access Requests (UKNF Employee)

**As a** UKNF Employee  
**I want to** view and filter access requests  
**So that** I can efficiently manage and approve them

**Acceptance Criteria:**
- UKNF Employee can view list of all access requests with status "New" or "Updated"
- List displays: User name, Entity, Requested permissions, Submission date, Status
- Quick filter "My Entities" shows only requests for entities assigned to the employee
- Quick filter "Requires Action" shows requests with status "New" or "Updated"
- Employee can search/filter by: User name, Entity, Status, Date range
- Employee can sort by: Submission date, Status, Entity
- Employee can click request to view details

### Story 2.4: Review and Approve Access Request (UKNF Employee)

**As a** UKNF Employee  
**I want to** review and approve access requests for Entity Administrators  
**So that** they can gain appropriate system access

**Acceptance Criteria:**
- UKNF Employee can view complete access request details
- Employee can review: User information, Requested entities, Requested permissions, Attachments
- Employee can communicate with user via messages within the request (add message with attachments)
- Employee can approve individual permission lines
- Upon approval, request status changes from "New" to "Accepted"
- User receives automatic email notification of approval
- Approved permissions are immediately active
- System logs approval action with timestamp and employee ID
- Employee can view permission line history for the user

### Story 2.5: Entity Administrator Approval for Entity Employees

**As an** Entity Administrator  
**I want to** review and approve access requests from employees of my entity  
**So that** they can gain appropriate access to work with our supervised entity

**Acceptance Criteria:**
- Entity Administrator can view access requests for their entity only
- Administrator sees requests from Entity Employees for their entity
- Administrator can approve permissions for: Reporting, Cases (but not Entity Administrator permission)
- Administrator can assign entity email address for notifications
- Upon approval, request status changes to "Accepted"
- Employee receives automatic email notification
- Approved permissions are immediately active
- If Entity has no approved Administrator, UKNF Employees handle employee requests

### Story 2.6: Request Additional Permissions (Update Request)

**As an** external user with existing permissions  
**I want to** request additional permissions or access to additional entities  
**So that** I can expand my system access as my role evolves

**Acceptance Criteria:**
- User can create a new permission request while having active permissions
- User can add new entities from Directory of Entities
- User can request additional permission types
- Upon submission, request status changes to "Updated"
- Request goes through same approval workflow
- Existing permissions remain active during approval process
- Approver can see history of previous permissions
- User receives confirmation of submission and upon approval

### Story 2.7: Communicate Within Access Request

**As a** UKNF Employee or Entity Administrator reviewing an access request  
**I want to** communicate with the requesting user via messages  
**So that** I can request clarifications or additional information

**Acceptance Criteria:**
- Reviewer can create messages within the access request context
- Messages support text content and file attachments
- User receives email notification of new messages
- User can respond with messages and attachments
- All messages are stored with the access request
- Message thread is visible to all parties (user and reviewers)
- Messages include timestamp and sender information

### Story 2.8: Block Entity Administrator Permissions

**As a** UKNF Employee  
**I want to** block an Entity Administrator's permissions  
**So that** they lose system access while maintaining system integrity

**Acceptance Criteria:**
- UKNF Employee can select "Block" action for Entity Administrator
- System changes all permission lines for that administrator to "Blocked" status
- Administrator immediately loses system access (cannot log in)
- Other administrators for the same entity retain their access
- Entity Employees previously approved by blocked administrator retain their permissions
- If no other administrators exist, UKNF takes over approval for that entity's employee requests
- System logs blocking action with timestamp and employee ID
- Blocked administrator receives email notification

### Story 2.9: Manage Entity Employee Permissions (Entity Administrator)

**As an** Entity Administrator  
**I want to** manage permissions for employees of my supervised entity  
**So that** I can control access appropriate to their roles

**Acceptance Criteria:**
- Administrator can view all employees with permissions for their entity
- Administrator can modify employee permissions (Reporting, Cases access)
- Administrator can block employee access to the system
- Administrator cannot modify permissions beyond their entity scope
- Changes take effect immediately
- Modified user receives email notification
- System logs all permission changes with timestamp
- Administrator can view permission change history

### Story 2.10: Select Entity Context for Session

**As an** external user with permissions for multiple entities  
**I want to** select which entity I'm representing in my current session  
**So that** I can work in the appropriate entity context

**Acceptance Criteria:**
- Upon successful login, user with multiple entities sees entity selection screen
- Entity selection shows all entities user has permissions for
- User selects one entity to establish session context
- Selected entity and user's role are displayed throughout the application (in header/navigation)
- User can switch entity context without logging out (if system design allows)
- All actions performed are associated with the selected entity
- System logs entity selection with timestamp and user ID

### Story 2.11: View Access Request History

**As an** external user, Entity Administrator, or UKNF Employee  
**I want to** view the complete history of an access request  
**So that** I can understand all changes and actions taken

**Acceptance Criteria:**
- Users can view access request history showing all status changes
- History shows: Timestamp, Action, Performed by, Previous value, New value
- History includes all permission line changes
- History includes all messages exchanged
- History is read-only (cannot be modified)
- History is sortable by date
- Export history option available (PDF or CSV)

---

## Access Request Statuses

[Source: docs/prd/functions-of-the-authentication-and-authorization-module.md]

| Status | Description |
|--------|-------------|
| **Working** | Request created but not yet submitted for acceptance |
| **New** | Request completed and submitted for approval |
| **Accepted** | All permission lines have been accepted |
| **Blocked** | All permission lines have been blocked |
| **Updated** | Request has been modified and awaits re-acceptance |

---

## Technical Considerations

### Permission Types

- **Reporting**: Access to reporting module functions
- **Cases**: Access to case management functions
- **Entity Administrator**: Broadest permissions within entity, can approve employee requests

### Data Model Requirements

- **Users**: Links to authenticated user accounts (Epic 1)
- **Entities**: References UKNF Directory of Entities (external system)
- **Access Requests**: Main workflow entity with status tracking
- **Permission Lines**: Individual permissions with entity and type
- **Request Messages**: Communication thread within requests

### Entity Assignment

- Entity data comes from UKNF Directory of Entities (external source)
- System must maintain reference to entities but not duplicate entity data
- Entity email addresses can be added and associated with entities through access requests

### Security Requirements

- Users can only view and manage requests related to their entities
- UKNF Employees have broader access based on "My Entities" assignments
- Entity Administrators cannot grant themselves Entity Administrator permissions (must come from UKNF)
- All permission changes must be audited

### Audit Requirements

[Source: docs/prd/b-specification-of-non-functional-requirements.md#22-audit-and-login]

- Log all access request status changes
- Log all permission grants, modifications, and blocks
- Log all messages within requests
- Include timestamp, user ID, action, and affected entities

---

## Dependencies

### Prerequisites
- **Epic 1**: Authentication & User Registration (requires authenticated users)
- **UKNF Directory of Entities**: External system providing entity list
- **Email service**: For notifications (configured in Epic 1)

### Blocks
- **Epic 3**: Administrative Module (needs authorization framework)
- **Epic 4-8**: All communication module features (require authorized users)

---

## Definition of Done

- [ ] Access requests are automatically created upon user account activation
- [ ] External users can complete and submit access requests
- [ ] UKNF Employees can view, filter, and approve Entity Administrator requests
- [ ] Entity Administrators can approve Entity Employee requests
- [ ] Users can request additional permissions (update requests)
- [ ] Communication within access requests works with messages and attachments
- [ ] UKNF Employees can block Entity Administrator permissions
- [ ] Entity Administrators can manage employee permissions
- [ ] Users with multiple entities can select session context
- [ ] Complete access request history is viewable
- [ ] All email notifications are sent appropriately
- [ ] All authorization events are logged for audit
- [ ] Unit tests cover all permission workflows
- [ ] Integration tests verify approval flows
- [ ] Security testing confirms proper access controls
- [ ] Documentation includes all permission types and workflows

---

## Related PRD Sections

- [Functions of the Authentication and Authorization Module](./functions-of-the-authentication-and-authorization-module.md)
- [Actors - External Users](./actors.md#external-users)
- [Actors - Internal Users](./actors.md#internal-users)
- [B. Specification of Non-Functional Requirements - Security](./b-specification-of-non-functional-requirements.md#2-security)

---

## Notes

- This epic is complex and may require 6-8 stories depending on implementation approach
- Consider implementing request approval as a state machine to handle all status transitions
- Entity Administrator blocking has special rules - ensure proper testing of cascade effects
- "My Entities" assignment for UKNF Employees should be configurable (may be part of Epic 3)
- Integration with UKNF Directory of Entities is critical - coordinate with external team

