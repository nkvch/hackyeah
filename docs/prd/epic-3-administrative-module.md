# Epic 3: Administrative Module - User & Role Management

**Epic Number:** 3  
**Phase:** Foundation & Access (Additional Functionalities - Prerequisites)  
**Priority:** High (Required for system configuration and user management)  
**Status:** Draft

---

## Epic Goal

Implement comprehensive administrative capabilities for UKNF System Administrators to manage internal and external user accounts, configure system-wide password policies, and establish role-based access control (RBAC) for the entire platform.

---

## Epic Description

### Context

While Epics 1 and 2 established user authentication and authorization workflows, the system requires administrative tools for UKNF System Administrators to:
- Manage all user accounts (both internal UKNF employees and external supervised entity users)
- Configure and enforce password policies system-wide
- Create and manage roles with fine-grained permissions
- Assign users to roles for scalable access control

[Source: docs/prd/functions-of-the-administrative-module.md]

### What This Epic Delivers

This epic implements the Administrative Module providing:
- **User Account Management**: Full CRUD operations for internal and external users by System Administrators
- **Password Policy Configuration**: System-wide password security settings
- **Role-Based Access Control (RBAC)**: Role creation, permission configuration, and user assignments
- **Forced Password Changes**: Ability to require users to reset passwords
- **Administrative Audit Trail**: Comprehensive logging of all administrative actions

### Key Business Rules

1. **System Administrator Role**: Only UKNF Employees with System Administrator role can access these functions
2. **Password Policy Enforcement**: All users (internal and external) must comply with configured policies
3. **Role Hierarchy**: Permissions are assigned to roles, roles are assigned to users (standard RBAC)
4. **Manual Password Reset**: System Administrators can set or force reset of any user password
5. **User Type Distinction**: System must manage both internal (UKNF) and external (supervised entity) users

[Source: docs/prd/functions-of-the-administrative-module.md]

---

## User Stories

### Story 3.1: View and Search User Accounts

**As a** System Administrator  
**I want to** view and search all user accounts in the system  
**So that** I can manage users efficiently

**Acceptance Criteria:**
- System Administrator can view list of all users (internal and external)
- List displays: Name, Email, User Type (Internal/External), Status (Active/Inactive/Blocked), Roles, Last Login
- Administrator can filter by: User type, Status, Role, Entity (for external users)
- Administrator can search by: Name, Email, PESEL (last 4 digits)
- Administrator can sort by: Name, Email, Last Login, Registration Date
- List supports pagination for large datasets
- Administrator can export user list (CSV, Excel)

### Story 3.2: Create Internal User Account

**As a** System Administrator  
**I want to** create accounts for UKNF Employees  
**So that** internal staff can access the system

**Acceptance Criteria:**
- System Administrator can access "Create Internal User" form
- Form includes fields: First name, Last name, Email, Phone number, Employee ID (optional)
- System validates email uniqueness
- System validates email format
- System validates phone number format
- Administrator can set initial password OR trigger automatic password setup email
- Administrator can assign roles during creation
- Upon creation, user account is set to Active status
- User receives welcome email with login instructions
- System logs user creation with timestamp and administrator ID

### Story 3.3: Create External User Account (Manual)

**As a** System Administrator  
**I want to** manually create accounts for external users  
**So that** I can onboard users who cannot self-register

**Acceptance Criteria:**
- System Administrator can access "Create External User" form
- Form includes mandatory fields: First name, Last name, PESEL (full, will be masked in display), Email, Phone
- System validates all field formats (email, PESEL, phone)
- System validates email and PESEL uniqueness
- Administrator can set initial password OR trigger automatic password setup email
- Administrator cannot directly assign entity permissions (must go through access request workflow from Epic 2)
- Upon creation, user account is set to Active status
- User receives account creation email with login instructions
- System logs user creation with timestamp and administrator ID

### Story 3.4: Edit User Account

**As a** System Administrator  
**I want to** edit user account information  
**So that** I can keep user data current and accurate

**Acceptance Criteria:**
- System Administrator can select any user account to edit
- Administrator can modify: First name, Last name, Email, Phone number
- Administrator cannot modify: PESEL (immutable), User ID
- Email changes require uniqueness validation
- System tracks all changes in audit log
- Changes take effect immediately
- If email is changed, user receives notification to both old and new addresses
- System displays last modified timestamp and administrator

### Story 3.5: Delete/Deactivate User Account

**As a** System Administrator  
**I want to** deactivate or delete user accounts  
**So that** I can remove access for users who should no longer have it

**Acceptance Criteria:**
- System Administrator can select user to deactivate or delete
- **Deactivation**: User account status changes to "Inactive", user cannot log in, data is retained
- **Deletion**: User account is marked as deleted (soft delete recommended), user cannot log in
- System prompts confirmation before deactivation/deletion
- Deactivated users can be reactivated
- Deleted users cannot be reactivated (must create new account)
- All user actions/data remain in audit logs even after deletion
- User receives email notification of account deactivation
- System logs action with timestamp and administrator ID

### Story 3.6: Set User Password Manually

**As a** System Administrator  
**I want to** manually set a user's password  
**So that** I can help users who are locked out or need immediate access

**Acceptance Criteria:**
- System Administrator can select any user and choose "Set Password"
- Administrator enters new password meeting system policy requirements
- Password strength indicator is displayed
- System validates password meets all policy requirements
- Upon setting password, system can optionally require user to change on next login
- User receives email notification that password was changed by administrator
- User's existing sessions are invalidated (force re-login)
- Action is logged in audit trail with timestamp and administrator ID

### Story 3.7: Force Password Change

**As a** System Administrator  
**I want to** force specific users to change their passwords  
**So that** I can enforce security policies when needed

**Acceptance Criteria:**
- System Administrator can select one or multiple users
- Administrator triggers "Force Password Change" action
- Selected users are flagged in system as requiring password change
- Upon next login, flagged users are redirected to password change page
- Users cannot access system functions until password is changed
- New password must meet all policy requirements
- Users receive email notification of forced password change requirement
- Action is logged with timestamp, affected users, and administrator ID

### Story 3.8: Configure Password Policy

**As a** System Administrator  
**I want to** configure system-wide password policy settings  
**So that** I can enforce appropriate security standards

**Acceptance Criteria:**
- System Administrator can access "Password Policy Configuration" page
- Administrator can configure:
  - Minimum password length (e.g., 8-64 characters)
  - Password complexity requirements (uppercase, lowercase, numbers, special characters)
  - Password expiration period (e.g., 90 days, never)
  - Password history length (prevent reuse of last N passwords, e.g., 5-10)
  - Lockout policy after failed login attempts (optional)
  - Minimum password age (prevent rapid changes)
- System validates configuration values are within acceptable ranges
- Changes to password policy apply to all future password creations/changes
- Existing passwords are not invalidated but must comply on next change
- System displays current active policy settings
- Policy changes are logged with timestamp and administrator ID
- Administrator can view history of policy changes

### Story 3.9: Create and Manage Roles

**As a** System Administrator  
**I want to** create and manage roles with specific permissions  
**So that** I can implement role-based access control

**Acceptance Criteria:**
- System Administrator can create new roles
- Role creation includes: Role name, Description, Active status
- Role names must be unique
- Administrator can edit role details (name, description, status)
- Administrator can deactivate roles (users with deactivated roles lose those permissions)
- Administrator can delete roles (only if no users are assigned)
- System displays list of all roles with user count
- Changes to roles are logged with timestamp and administrator ID

### Story 3.10: Configure Role Permissions

**As a** System Administrator  
**I want to** configure which permissions are assigned to each role  
**So that** I can control what users with that role can do

**Acceptance Criteria:**
- System Administrator can select a role and access "Manage Permissions"
- System displays all available permissions organized by module/category:
  - **Communication Module**: View Reports, Submit Reports, Validate Reports, View Cases, Manage Cases, View Messages, Send Messages, Access Library, etc.
  - **Authentication Module**: View Access Requests, Approve Access Requests, Manage User Permissions, etc.
  - **Administrative Module**: Manage Users, Manage Roles, Configure System Settings, etc.
  - **Entity Management**: View Entities, Edit Entities, etc.
- Administrator can select/deselect permissions using checkboxes
- Administrator can select entire module permissions at once (bulk selection)
- Changes take effect immediately for all users with that role
- System shows which permissions are currently assigned
- Permission changes are logged with timestamp and administrator ID

### Story 3.11: Assign Roles to Users

**As a** System Administrator  
**I want to** assign roles to internal and external users  
**So that** they receive appropriate permissions

**Acceptance Criteria:**
- System Administrator can select any user account
- Administrator can view current roles assigned to that user
- Administrator can add one or multiple roles to the user
- Administrator can remove roles from the user
- Changes take effect immediately (user's permissions update)
- If user is currently logged in, changes apply on next page request or next login (per system design)
- User receives email notification of role changes
- Role assignments are logged with timestamp and administrator ID
- Administrator can bulk-assign roles to multiple users

### Story 3.12: View Role Assignments and Permissions

**As a** System Administrator  
**I want to** view which users have which roles and what permissions those roles provide  
**So that** I can audit and understand access control configuration

**Acceptance Criteria:**
- System Administrator can view role assignments by role (see all users with Role X)
- System Administrator can view role assignments by user (see all roles for User Y)
- System Administrator can view effective permissions for a user (all permissions from all roles)
- System provides "Role Usage Report" showing user counts per role
- System provides "Permission Report" showing which permissions are granted through which roles
- Reports can be exported (PDF, CSV)
- Administrator can search/filter reports by user, role, permission

---

## Technical Considerations

### Role-Based Access Control (RBAC) Architecture

- **Roles**: Named collections of permissions (e.g., "Report Reviewer", "Case Manager", "System Administrator")
- **Permissions**: Granular access rights to specific functions (e.g., "reports.view", "reports.submit", "users.manage")
- **User-Role Assignment**: Many-to-many relationship (users can have multiple roles, roles can have multiple users)
- **Role-Permission Assignment**: Many-to-many relationship (roles can have multiple permissions, permissions can be in multiple roles)

### Permission Naming Convention

Recommended structure: `{module}.{entity}.{action}`

Examples:
- `communication.reports.view`
- `communication.reports.submit`
- `communication.cases.create`
- `communication.cases.manage`
- `admin.users.create`
- `admin.users.delete`
- `admin.roles.manage`

### Password Policy Storage

- Store as system configuration in database
- Apply validation on all password creation/change operations
- Consider making policies versioned for audit purposes

### Security Requirements

[Source: docs/prd/b-specification-of-non-functional-requirements.md#2-security]

- Only users with System Administrator role can access administrative functions
- All administrative actions must be audited
- Password changes by administrators should notify users
- Bulk operations should require confirmation
- Deactivating/deleting users should verify no active sessions

### Audit Requirements

[Source: docs/prd/b-specification-of-non-functional-requirements.md#22-audit-and-login]

- Log all user account changes (create, edit, delete, deactivate, reactivate)
- Log all password operations (set, force change)
- Log all password policy changes
- Log all role operations (create, edit, delete, activate, deactivate)
- Log all permission configuration changes
- Log all role assignments and removals
- Include timestamp, administrator ID, action performed, affected entities

---

## Dependencies

### Prerequisites
- **Epic 1**: Authentication & User Registration (extends user management)
- **Epic 2**: Authorization & Access Requests (works alongside for external users)
- **Database**: User table, Roles table, Permissions table, Role-User junction, Role-Permission junction

### Blocks
- **Epic 4-8**: Communication features will check permissions configured through this epic

---

## Definition of Done

- [ ] System Administrators can view, search, and filter all user accounts
- [ ] System Administrators can create internal and external user accounts
- [ ] System Administrators can edit user account information
- [ ] System Administrators can deactivate and delete user accounts
- [ ] System Administrators can manually set user passwords
- [ ] System Administrators can force users to change passwords
- [ ] System Administrators can configure system-wide password policy
- [ ] Password policy is enforced on all password operations
- [ ] System Administrators can create and manage roles
- [ ] System Administrators can configure permissions for roles
- [ ] System Administrators can assign/remove roles to/from users
- [ ] System Administrators can view role assignments and permission reports
- [ ] All administrative actions are logged in audit trail
- [ ] Email notifications are sent for relevant actions
- [ ] Unit tests cover all administrative functions
- [ ] Integration tests verify RBAC enforcement
- [ ] Security testing confirms only System Administrators can access admin functions
- [ ] Documentation includes all permissions and recommended role configurations

---

## Related PRD Sections

- [Functions of the Administrative Module](./functions-of-the-administrative-module.md)
- [Actors - Internal Users](./actors.md#internal-users)
- [B. Specification of Non-Functional Requirements - Security](./b-specification-of-non-functional-requirements.md#2-security)
- [B. Specification of Non-Functional Requirements - Audit](./b-specification-of-non-functional-requirements.md#22-audit-and-login)

---

## Notes

- This epic provides administrative capabilities; actual permission enforcement happens in Epic 4-8 implementations
- Consider creating default roles during system initialization: "System Administrator", "UKNF Employee", "Entity Administrator", "Entity Employee"
- RBAC should be flexible enough to support future permission additions without schema changes
- Integration with Epic 2's Entity Administrator and Employee concepts should be seamless
- Consider UI/UX carefully for permission management - checkboxes grouped by module work well
- Bulk operations (assigning roles to multiple users) should be carefully designed to prevent errors
- Password policy should be retrievable via API for frontend validation

