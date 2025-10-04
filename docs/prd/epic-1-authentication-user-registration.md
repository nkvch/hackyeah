# Epic 1: Authentication & User Registration

**Epic Number:** 1  
**Phase:** Foundation & Access (Additional Functionalities - Prerequisites)  
**Priority:** High (Prerequisite for all other features)  
**Status:** Draft

---

## Epic Goal

Establish secure user authentication and registration capabilities for external users (Supervised Entity Administrators and Employees) to access the Communication Platform, ensuring compliance with security requirements and password policies.

---

## Epic Description

### Context

The Communication Platform requires a secure foundation for external user access. Before supervised entities can interact with UKNF through the platform, external users must be able to register, activate their accounts, and authenticate securely.

[Source: docs/prd/functions-of-the-authentication-and-authorization-module.md]

### What This Epic Delivers

This epic implements the foundational authentication system that enables:
- **External user registration** through an online form with mandatory fields
- **Account activation** via email confirmation links
- **Secure password management** following system password policies
- **User authentication** for session establishment
- **Basic user profile management**

### Target Users

- **External Users:**
  - Supervised Entity Administrators (broadest permissions within their entity)
  - Employees of Supervised Entity (permissions granted per configuration)

[Source: docs/prd/actors.md]

---

## User Stories

### Story 1.1: External User Registration Form

**As an** external user (representative of a supervised entity)  
**I want to** register for a system account via an online form  
**So that** I can gain access to communicate with UKNF through the platform

**Acceptance Criteria:**
- Online registration form is publicly accessible (no authentication required)
- Form includes mandatory fields: First name, Last name, PESEL (masked, last 4 digits visible), phone number, email
- Form validates all required fields before submission
- PESEL field is properly masked showing only last 4 digits
- Email format is validated
- Phone number format is validated (international format: `/^\+(?:[0-9] ?){6,14}[0-9]$/`)
- Successful registration creates an inactive user account in the system
- User receives confirmation message after successful registration submission

### Story 1.2: Account Activation via Email

**As a** newly registered external user  
**I want to** activate my account via an email link  
**So that** I can set my password and access the system

**Acceptance Criteria:**
- System sends activation link to the email address provided during registration
- Activation email is sent immediately after successful registration
- Activation link is secure (time-limited, single-use token)
- Clicking activation link opens password creation page
- User cannot log in before activating their account
- System displays appropriate error messages for expired or invalid activation links

### Story 1.3: Initial Password Creation

**As an** external user activating my account  
**I want to** set my initial password according to system policy  
**So that** I can secure my account and log in

**Acceptance Criteria:**
- Password creation form enforces system password policy (complexity, length, uniqueness requirements)
- Password strength indicator is displayed to the user
- Password confirmation field requires exact match
- System validates password meets all policy requirements before accepting
- Upon successful password creation, account status changes to "Active"
- System displays success message and redirects to login page
- Password is securely hashed before storage

### Story 1.4: User Login (Authentication)

**As an** external user with an active account  
**I want to** log in using my email and password  
**So that** I can access the communication platform

**Acceptance Criteria:**
- Login form accepts email address and password
- System validates credentials against stored user data
- Successful authentication creates a user session
- Failed login attempts display generic error message (security best practice)
- System implements rate limiting to prevent brute force attacks
- System logs all authentication attempts (for audit purposes)
- Successful login redirects to appropriate landing page based on user permissions

### Story 1.5: Basic User Profile Management

**As an** authenticated external user  
**I want to** view and update my basic profile information  
**So that** my contact details remain current in the system

**Acceptance Criteria:**
- User can view their profile information (First name, Last name, PESEL (masked), phone, email)
- User can update phone number and email address
- Email changes require confirmation via new email address
- PESEL cannot be modified (immutable after registration)
- System validates phone and email format on update
- Changes are logged with timestamp and user ID (audit trail)
- User receives confirmation after successful profile update

### Story 1.6: Password Change

**As an** authenticated external user  
**I want to** change my password  
**So that** I can maintain account security

**Acceptance Criteria:**
- Password change form requires current password verification
- New password must meet system password policy requirements
- New password cannot match current password
- New password cannot match passwords in history (based on password policy configuration)
- Password strength indicator is displayed
- Successful password change invalidates current session and requires re-login
- User receives email notification of password change
- System logs password change event with timestamp

### Story 1.7: Password Reset (Forgot Password)

**As an** external user who forgot their password  
**I want to** reset my password via email  
**So that** I can regain access to my account

**Acceptance Criteria:**
- "Forgot Password" link is available on login page
- User enters their registered email address
- System sends password reset link to email (if email exists in system)
- System does not reveal whether email exists (security best practice)
- Reset link is secure (time-limited, single-use token)
- Reset link opens password creation page
- New password must meet system password policy
- Successful reset invalidates all existing sessions
- User receives confirmation email after successful reset

---

## Technical Considerations

### Security Requirements

[Source: docs/prd/b-specification-of-non-functional-requirements.md#2-security]

- All passwords must be hashed using industry-standard algorithms (bcrypt, Argon2, or similar)
- Session tokens must be secure, random, and time-limited
- All authentication endpoints must be protected against brute force attacks
- Implement CSRF protection for all forms
- Use HTTPS for all communications
- Email activation and password reset tokens must expire (recommended: 24 hours)

### Password Policy Requirements

[Source: docs/prd/functions-of-the-administrative-module.md]

- Password complexity: configurable length, uniqueness requirements
- Password history: prevent reuse of recent passwords (configurable)
- Password expiration: configurable frequency of mandatory change
- Manual password reset capability for administrators

### Audit Requirements

[Source: docs/prd/b-specification-of-non-functional-requirements.md#22-audit-and-login]

- Log all authentication events (successful and failed)
- Log all registration events
- Log all password changes and resets
- Log all profile updates
- Include timestamp, user ID, IP address, and action performed

### Data Validation

- PESEL: masked display, last 4 digits visible
- Email: standard email format validation
- Phone: international format validation `/^\+(?:[0-9] ?){6,14}[0-9]$/`
- All user inputs must be sanitized to prevent injection attacks

---

## Dependencies

### Prerequisites
- Database schema for user accounts
- Email service configuration (SMTP)
- Password hashing library
- Session management infrastructure

### Blocks
- Epic 2: Authorization & Access Requests (requires authenticated users)
- Epic 3: Administrative Module (requires user accounts to manage)
- All communication module epics (require authenticated users)

---

## Definition of Done

- [ ] External users can register via online form with all mandatory fields
- [ ] Account activation via email works reliably
- [ ] Users can set secure passwords meeting system policy
- [ ] Users can authenticate with email and password
- [ ] Users can view and update their profile information
- [ ] Users can change their password
- [ ] Users can reset forgotten passwords
- [ ] All authentication events are logged for audit
- [ ] All security requirements are implemented
- [ ] Password policy is configurable and enforced
- [ ] Unit tests cover all authentication flows
- [ ] Integration tests verify email delivery
- [ ] Security testing confirms protection against common attacks
- [ ] Documentation includes API specifications for all endpoints

---

## Related PRD Sections

- [Functions of the Authentication and Authorization Module](./functions-of-the-authentication-and-authorization-module.md#description-of-the-function)
- [Actors - External Users](./actors.md#external-users)
- [B. Specification of Non-Functional Requirements - Security](./b-specification-of-non-functional-requirements.md#2-security)
- [B. Specification of Non-Functional Requirements - Audit](./b-specification-of-non-functional-requirements.md#22-audit-and-login)

---

## Notes

- This epic focuses ONLY on authentication; authorization (permissions, access requests) is handled in Epic 2
- After completing this epic, users can register and log in but will have no specific permissions until Epic 2 is completed
- Consider implementing rate limiting and CAPTCHA for registration form to prevent abuse
- Email service must be thoroughly tested in development environment before production deployment

