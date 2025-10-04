# **Section 8: REST API Spec**

Now let's define the REST API specification using OpenAPI 3.0. This will serve as the contract between frontend and backend.

---

```yaml
openapi: 3.0.0
info:
  title: UKNF Communication Platform API
  version: 1.0.0
  description: |
    REST API for the UKNF Communication Platform - a secure system for communication 
    between UKNF (Polish Financial Supervision Authority) and supervised financial entities.
    
    ## Modules
    - **Communication Module**: Reports, messages, cases, library, FAQ, bulletin board
    - **Authentication & Authorization Module**: User registration, access requests, entity context
    - **Administrative Module**: User management, roles, password policies
    
    ## Authentication
    All endpoints (except registration and login) require JWT Bearer token authentication.
    Include the token in the Authorization header: `Authorization: Bearer <token>`
    
  contact:
    name: UKNF Development Team
    email: support@uknf.gov.pl

servers:
  - url: http://localhost:5000/api
    description: Local development server
  - url: https://api.uknf-demo.example.com/api
    description: Demo/staging server

security:
  - BearerAuth: []

tags:
  - name: Authentication
    description: User authentication and session management
  - name: Authorization
    description: Authorization and entity context management
  - name: Access Requests
    description: External user registration and access requests
  - name: Reports
    description: Financial report submission and validation
  - name: Messages
    description: Two-way communication between users
  - name: Cases
    description: Administrative case management
  - name: Library
    description: File repository and template management
  - name: FAQ
    description: Questions and answers knowledge base
  - name: Bulletin Board
    description: Announcements and notifications
  - name: Entities
    description: Supervised entity management
  - name: Users (Admin)
    description: User account management (Admin only)
  - name: Roles (Admin)
    description: Role and permission management (Admin only)

paths:
  # Authentication Endpoints
  /auth/register:
    post:
      tags: [Authentication]
      summary: Register new external user
      security: []
      requestBody:
        required: true
        content:
          application/json:
            schema:
              type: object
              required: [firstName, lastName, pesel, email, phone]
              properties:
                firstName:
                  type: string
                  example: Jan
                lastName:
                  type: string
                  example: Kowalski
                pesel:
                  type: string
                  description: National ID (will be masked, last 4 digits visible)
                  example: "12345678901"
                email:
                  type: string
                  format: email
                  example: jan.kowalski@example.com
                phone:
                  type: string
                  pattern: '^\+(?:[0-9] ?){6,14}[0-9]$'
                  example: "+48123456789"
      responses:
        '201':
          description: User registered successfully, activation email sent
          content:
            application/json:
              schema:
                type: object
                properties:
                  userId:
                    type: string
                    format: uuid
                  message:
                    type: string
                    example: Registration successful. Please check your email to activate your account.
        '400':
          $ref: '#/components/responses/BadRequest'
        '409':
          description: Email or PESEL already exists

  /auth/login:
    post:
      tags: [Authentication]
      summary: User login
      security: []
      requestBody:
        required: true
        content:
          application/json:
            schema:
              type: object
              required: [email, password]
              properties:
                email:
                  type: string
                  format: email
                password:
                  type: string
                  format: password
      responses:
        '200':
          description: Login successful
          content:
            application/json:
              schema:
                type: object
                properties:
                  accessToken:
                    type: string
                    description: JWT access token
                  refreshToken:
                    type: string
                    description: Refresh token for obtaining new access tokens
                  expiresIn:
                    type: integer
                    description: Token expiration time in seconds
                  user:
                    $ref: '#/components/schemas/UserInfo'
                  entities:
                    type: array
                    description: List of entities the user can represent
                    items:
                      $ref: '#/components/schemas/EntitySummary'
        '401':
          $ref: '#/components/responses/Unauthorized'

  /auth/logout:
    post:
      tags: [Authentication]
      summary: User logout (invalidate token)
      responses:
        '204':
          description: Logout successful

  /auth/me:
    get:
      tags: [Authentication]
      summary: Get current authenticated user info
      responses:
        '200':
          description: User information
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/UserInfo'

  # Authorization Endpoints
  /authorization/select-entity:
    post:
      tags: [Authorization]
      summary: Select entity context for session
      requestBody:
        required: true
        content:
          application/json:
            schema:
              type: object
              required: [entityId]
              properties:
                entityId:
                  type: integer
                  format: int64
      responses:
        '200':
          description: Entity context selected
          content:
            application/json:
              schema:
                type: object
                properties:
                  entityId:
                    type: integer
                    format: int64
                  entityName:
                    type: string
                  permissions:
                    type: array
                    items:
                      type: string
        '403':
          $ref: '#/components/responses/Forbidden'

  /authorization/permissions:
    get:
      tags: [Authorization]
      summary: Get current user's permissions in selected entity context
      responses:
        '200':
          description: User permissions
          content:
            application/json:
              schema:
                type: object
                properties:
                  entityId:
                    type: integer
                    format: int64
                  permissions:
                    type: array
                    items:
                      type: string
                      example: reporting.submit

  # Access Requests
  /access-requests:
    post:
      tags: [Access Requests]
      summary: Create access request (draft)
      requestBody:
        required: true
        content:
          application/json:
            schema:
              $ref: '#/components/schemas/AccessRequestCreate'
      responses:
        '201':
          description: Access request created
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/AccessRequest'
        '400':
          $ref: '#/components/responses/BadRequest'
    
    get:
      tags: [Access Requests]
      summary: List access requests
      parameters:
        - name: status
          in: query
          schema:
            type: string
            enum: [Working, New, Accepted, Blocked, Updated]
        - name: entityId
          in: query
          schema:
            type: integer
            format: int64
        - name: myEntities
          in: query
          description: Filter by entities assigned to current UKNF employee
          schema:
            type: boolean
        - $ref: '#/components/parameters/PageNumber'
        - $ref: '#/components/parameters/PageSize'
      responses:
        '200':
          description: List of access requests
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/AccessRequestList'

  /access-requests/{id}:
    get:
      tags: [Access Requests]
      summary: Get access request details
      parameters:
        - $ref: '#/components/parameters/Id'
      responses:
        '200':
          description: Access request details
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/AccessRequest'
        '404':
          $ref: '#/components/responses/NotFound'
    
    put:
      tags: [Access Requests]
      summary: Update access request (draft only)
      parameters:
        - $ref: '#/components/parameters/Id'
      requestBody:
        required: true
        content:
          application/json:
            schema:
              $ref: '#/components/schemas/AccessRequestUpdate'
      responses:
        '200':
          description: Access request updated
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/AccessRequest'

  /access-requests/{id}/submit:
    post:
      tags: [Access Requests]
      summary: Submit access request for approval
      parameters:
        - $ref: '#/components/parameters/Id'
      responses:
        '200':
          description: Access request submitted
        '400':
          description: Request incomplete or already submitted

  /access-requests/{id}/approve:
    post:
      tags: [Access Requests]
      summary: Approve access request (UKNF or Entity Admin)
      parameters:
        - $ref: '#/components/parameters/Id'
      responses:
        '200':
          description: Access request approved
        '403':
          $ref: '#/components/responses/Forbidden'

  # Reports
  /reports/upload:
    post:
      tags: [Reports]
      summary: Upload report file (supports chunked upload)
      requestBody:
        required: true
        content:
          multipart/form-data:
            schema:
              type: object
              properties:
                file:
                  type: string
                  format: binary
                entityId:
                  type: integer
                  format: int64
                reportType:
                  type: string
                  example: Quarterly
                reportingPeriod:
                  type: string
                  example: Q1_2025
                chunkNumber:
                  type: integer
                  description: For chunked uploads
                totalChunks:
                  type: integer
                  description: For chunked uploads
      responses:
        '201':
          description: Report uploaded
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/Report'
        '400':
          $ref: '#/components/responses/BadRequest'

  /reports:
    get:
      tags: [Reports]
      summary: List reports
      parameters:
        - name: entityId
          in: query
          schema:
            type: integer
            format: int64
        - name: status
          in: query
          schema:
            type: string
            enum: [Working, Transmitted, Ongoing, Successful, ValidationErrors, TechnicalError, TimeoutError, ContestedByUKNF]
        - name: reportType
          in: query
          schema:
            type: string
        - name: reportingPeriod
          in: query
          schema:
            type: string
        - name: isArchived
          in: query
          schema:
            type: boolean
        - name: myEntities
          in: query
          schema:
            type: boolean
        - $ref: '#/components/parameters/PageNumber'
        - $ref: '#/components/parameters/PageSize'
      responses:
        '200':
          description: List of reports
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/ReportList'

  /reports/{id}:
    get:
      tags: [Reports]
      summary: Get report details
      parameters:
        - $ref: '#/components/parameters/Id'
      responses:
        '200':
          description: Report details
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/Report'
        '404':
          $ref: '#/components/responses/NotFound'

  /reports/{id}/submit:
    post:
      tags: [Reports]
      summary: Submit report for validation
      parameters:
        - $ref: '#/components/parameters/Id'
      responses:
        '200':
          description: Report submitted for validation
          content:
            application/json:
              schema:
                type: object
                properties:
                  uniqueValidationId:
                    type: string
                  status:
                    type: string
                    example: Transmitted

  /reports/{id}/validation-result:
    get:
      tags: [Reports]
      summary: Download validation result PDF
      parameters:
        - $ref: '#/components/parameters/Id'
      responses:
        '200':
          description: Validation result file
          content:
            application/pdf:
              schema:
                type: string
                format: binary
        '404':
          description: Validation result not available yet

  /reports/{id}/contest:
    post:
      tags: [Reports]
      summary: UKNF contests report (UKNF employees only)
      parameters:
        - $ref: '#/components/parameters/Id'
      requestBody:
        required: true
        content:
          application/json:
            schema:
              type: object
              required: [contestDescription]
              properties:
                contestDescription:
                  type: string
      responses:
        '200':
          description: Report contested
        '403':
          $ref: '#/components/responses/Forbidden'

  /reports/{id}/archive:
    post:
      tags: [Reports]
      summary: Archive report (UKNF employees only)
      parameters:
        - $ref: '#/components/parameters/Id'
      responses:
        '200':
          description: Report archived

  # Messages
  /messages:
    post:
      tags: [Messages]
      summary: Send new message
      requestBody:
        required: true
        content:
          multipart/form-data:
            schema:
              type: object
              required: [subject, body, recipientIds]
              properties:
                subject:
                  type: string
                body:
                  type: string
                recipientIds:
                  type: array
                  items:
                    type: string
                    format: uuid
                contextType:
                  type: string
                  enum: [AccessRequest, Case, Report, Standalone]
                contextId:
                  type: string
                  format: uuid
                attachments:
                  type: array
                  items:
                    type: string
                    format: binary
      responses:
        '201':
          description: Message sent
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/Message'
    
    get:
      tags: [Messages]
      summary: List messages
      parameters:
        - name: contextType
          in: query
          schema:
            type: string
        - name: contextId
          in: query
          schema:
            type: string
            format: uuid
        - name: status
          in: query
          schema:
            type: string
            enum: [AwaitingUknfResponse, AwaitingUserResponse, Closed]
        - name: myEntities
          in: query
          schema:
            type: boolean
        - $ref: '#/components/parameters/PageNumber'
        - $ref: '#/components/parameters/PageSize'
      responses:
        '200':
          description: List of messages
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/MessageList'

  /messages/{id}:
    get:
      tags: [Messages]
      summary: Get message thread
      parameters:
        - $ref: '#/components/parameters/Id'
      responses:
        '200':
          description: Message with replies
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/MessageThread'

  /messages/{id}/reply:
    post:
      tags: [Messages]
      summary: Reply to message
      parameters:
        - $ref: '#/components/parameters/Id'
      requestBody:
        required: true
        content:
          multipart/form-data:
            schema:
              type: object
              required: [body]
              properties:
                body:
                  type: string
                attachments:
                  type: array
                  items:
                    type: string
                    format: binary
      responses:
        '201':
          description: Reply sent

  /messages/{id}/mark-read:
    post:
      tags: [Messages]
      summary: Mark message as read
      parameters:
        - $ref: '#/components/parameters/Id'
      responses:
        '204':
          description: Message marked as read

  /messages/{id}/close:
    post:
      tags: [Messages]
      summary: Close message thread
      parameters:
        - $ref: '#/components/parameters/Id'
      responses:
        '200':
          description: Message closed

  # Cases
  /cases:
    post:
      tags: [Cases]
      summary: Create case
      requestBody:
        required: true
        content:
          application/json:
            schema:
              $ref: '#/components/schemas/CaseCreate'
      responses:
        '201':
          description: Case created
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/Case'
    
    get:
      tags: [Cases]
      summary: List cases
      parameters:
        - name: entityId
          in: query
          schema:
            type: integer
            format: int64
        - name: status
          in: query
          schema:
            type: string
            enum: [Draft, New, Ongoing, ToBeCompleted, Cancelled, Completed]
        - name: category
          in: query
          schema:
            type: string
            enum: [RegistrationDataChange, StaffChange, EntityCall, SystemEntitlements, Reporting, Other]
        - name: priority
          in: query
          schema:
            type: string
            enum: [Low, Medium, High]
        - $ref: '#/components/parameters/PageNumber'
        - $ref: '#/components/parameters/PageSize'
      responses:
        '200':
          description: List of cases
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/CaseList'

  /cases/{id}:
    get:
      tags: [Cases]
      summary: Get case details
      parameters:
        - $ref: '#/components/parameters/Id'
      responses:
        '200':
          description: Case details
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/Case'
    
    put:
      tags: [Cases]
      summary: Update case
      parameters:
        - $ref: '#/components/parameters/Id'
      requestBody:
        required: true
        content:
          application/json:
            schema:
              $ref: '#/components/schemas/CaseUpdate'
      responses:
        '200':
          description: Case updated

  /cases/{id}/submit:
    post:
      tags: [Cases]
      summary: Submit draft case
      parameters:
        - $ref: '#/components/parameters/Id'
      responses:
        '200':
          description: Case submitted

  /cases/{id}/change-status:
    post:
      tags: [Cases]
      summary: Change case status
      parameters:
        - $ref: '#/components/parameters/Id'
      requestBody:
        required: true
        content:
          application/json:
            schema:
              type: object
              required: [status]
              properties:
                status:
                  type: string
                  enum: [Ongoing, ToBeCompleted, Completed]
                notes:
                  type: string
      responses:
        '200':
          description: Status changed

  /cases/{id}/cancel:
    post:
      tags: [Cases]
      summary: Cancel case (only if not opened by entity yet)
      parameters:
        - $ref: '#/components/parameters/Id'
      requestBody:
        required: true
        content:
          application/json:
            schema:
              type: object
              required: [cancellationReason]
              properties:
                cancellationReason:
                  type: string
      responses:
        '200':
          description: Case cancelled
        '400':
          description: Case already opened, cannot cancel

  # Library
  /library/files:
    post:
      tags: [Library]
      summary: Upload file to library (UKNF employees only)
      requestBody:
        required: true
        content:
          multipart/form-data:
            schema:
              type: object
              required: [file, fileName]
              properties:
                file:
                  type: string
                  format: binary
                fileName:
                  type: string
                description:
                  type: string
                reportingPeriod:
                  type: string
                category:
                  type: string
                version:
                  type: string
      responses:
        '201':
          description: File uploaded
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/LibraryFile'
    
    get:
      tags: [Library]
      summary: List library files
      parameters:
        - name: category
          in: query
          schema:
            type: string
        - name: isCurrentVersion
          in: query
          schema:
            type: boolean
        - name: isArchived
          in: query
          schema:
            type: boolean
        - $ref: '#/components/parameters/PageNumber'
        - $ref: '#/components/parameters/PageSize'
      responses:
        '200':
          description: List of library files
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/LibraryFileList'

  /library/files/{id}/download:
    get:
      tags: [Library]
      summary: Download library file
      parameters:
        - $ref: '#/components/parameters/Id'
      responses:
        '200':
          description: File download
          content:
            application/octet-stream:
              schema:
                type: string
                format: binary

  # FAQ
  /faq/questions:
    post:
      tags: [FAQ]
      summary: Submit question (anonymous)
      requestBody:
        required: true
        content:
          application/json:
            schema:
              type: object
              required: [title, questionContent, category]
              properties:
                title:
                  type: string
                questionContent:
                  type: string
                category:
                  type: string
                tags:
                  type: string
                  description: Comma-separated tags
      responses:
        '201':
          description: Question submitted
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/FAQ'
    
    get:
      tags: [FAQ]
      summary: Search/list questions
      parameters:
        - name: search
          in: query
          schema:
            type: string
        - name: category
          in: query
          schema:
            type: string
        - name: tags
          in: query
          schema:
            type: string
        - name: status
          in: query
          schema:
            type: string
            enum: [Submitted, Answered, Published, Archived]
        - name: sortBy
          in: query
          schema:
            type: string
            enum: [popularity, date, rating]
        - $ref: '#/components/parameters/PageNumber'
        - $ref: '#/components/parameters/PageSize'
      responses:
        '200':
          description: List of questions
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/FAQList'

  /faq/questions/{id}/answer:
    post:
      tags: [FAQ]
      summary: Add answer to question (UKNF employees only)
      parameters:
        - $ref: '#/components/parameters/Id'
      requestBody:
        required: true
        content:
          application/json:
            schema:
              type: object
              required: [answerContent]
              properties:
                answerContent:
                  type: string
      responses:
        '200':
          description: Answer added

  /faq/questions/{id}/rate:
    post:
      tags: [FAQ]
      summary: Rate answer (1-5 stars)
      parameters:
        - $ref: '#/components/parameters/Id'
      requestBody:
        required: true
        content:
          application/json:
            schema:
              type: object
              required: [rating]
              properties:
                rating:
                  type: integer
                  minimum: 1
                  maximum: 5
      responses:
        '200':
          description: Rating submitted

  # Bulletin Board
  /bulletin-board:
    post:
      tags: [Bulletin Board]
      summary: Create announcement (UKNF employees only)
      requestBody:
        required: true
        content:
          application/json:
            schema:
              $ref: '#/components/schemas/BulletinBoardCreate'
      responses:
        '201':
          description: Announcement created
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/BulletinBoard'
    
    get:
      tags: [Bulletin Board]
      summary: List announcements
      parameters:
        - name: category
          in: query
          schema:
            type: string
        - name: priority
          in: query
          schema:
            type: string
            enum: [Low, Medium, High]
        - name: unreadOnly
          in: query
          schema:
            type: boolean
        - $ref: '#/components/parameters/PageNumber'
        - $ref: '#/components/parameters/PageSize'
      responses:
        '200':
          description: List of announcements
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/BulletinBoardList'

  /bulletin-board/{id}/publish:
    post:
      tags: [Bulletin Board]
      summary: Publish announcement
      parameters:
        - $ref: '#/components/parameters/Id'
      responses:
        '200':
          description: Announcement published

  /bulletin-board/{id}/confirm-read:
    post:
      tags: [Bulletin Board]
      summary: Confirm reading announcement (required for high priority)
      parameters:
        - $ref: '#/components/parameters/Id'
      responses:
        '200':
          description: Read confirmation recorded

  /bulletin-board/{id}/statistics:
    get:
      tags: [Bulletin Board]
      summary: Get read statistics for announcement
      parameters:
        - $ref: '#/components/parameters/Id'
      responses:
        '200':
          description: Read statistics
          content:
            application/json:
              schema:
                type: object
                properties:
                  totalRecipients:
                    type: integer
                  readCount:
                    type: integer
                  readPercentage:
                    type: number
                  readByEntities:
                    type: integer
                  totalEntities:
                    type: integer

  # Entities
  /entities:
    get:
      tags: [Entities]
      summary: List entities
      parameters:
        - name: entityType
          in: query
          schema:
            type: string
        - name: status
          in: query
          schema:
            type: string
        - name: sector
          in: query
          schema:
            type: string
        - $ref: '#/components/parameters/PageNumber'
        - $ref: '#/components/parameters/PageSize'
      responses:
        '200':
          description: List of entities
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/EntityList'

  /entities/{id}:
    get:
      tags: [Entities]
      summary: Get entity details
      parameters:
        - name: id
          in: path
          required: true
          schema:
            type: integer
            format: int64
      responses:
        '200':
          description: Entity details
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/Entity'

components:
  securitySchemes:
    BearerAuth:
      type: http
      scheme: bearer
      bearerFormat: JWT

  parameters:
    Id:
      name: id
      in: path
      required: true
      schema:
        type: string
        format: uuid
    PageNumber:
      name: page
      in: query
      schema:
        type: integer
        minimum: 1
        default: 1
    PageSize:
      name: pageSize
      in: query
      schema:
        type: integer
        minimum: 1
        maximum: 100
        default: 20

  responses:
    BadRequest:
      description: Bad request - validation errors
      content:
        application/json:
          schema:
            $ref: '#/components/schemas/ErrorResponse'
    Unauthorized:
      description: Unauthorized - invalid or missing token
      content:
        application/json:
          schema:
            $ref: '#/components/schemas/ErrorResponse'
    Forbidden:
      description: Forbidden - insufficient permissions
      content:
        application/json:
          schema:
            $ref: '#/components/schemas/ErrorResponse'
    NotFound:
      description: Resource not found
      content:
        application/json:
          schema:
            $ref: '#/components/schemas/ErrorResponse'

  schemas:
    # Common schemas
    ErrorResponse:
      type: object
      properties:
        message:
          type: string
        errors:
          type: object
          additionalProperties:
            type: array
            items:
              type: string
        correlationId:
          type: string

    UserInfo:
      type: object
      properties:
        id:
          type: string
          format: uuid
        firstName:
          type: string
        lastName:
          type: string
        email:
          type: string
        userType:
          type: string
          enum: [Internal, External]
        isActive:
          type: boolean

    EntitySummary:
      type: object
      properties:
        id:
          type: integer
          format: int64
        uknfCode:
          type: string
        name:
          type: string
        entityType:
          type: string

    Entity:
      allOf:
        - $ref: '#/components/schemas/EntitySummary'
        - type: object
          properties:
            lei:
              type: string
            nip:
              type: string
            krs:
              type: string
            street:
              type: string
            buildingNumber:
              type: string
            postalCode:
              type: string
            city:
              type: string
            phone:
              type: string
            email:
              type: string
            entityStatus:
              type: string
            category:
              type: string
            sector:
              type: string
            createdDate:
              type: string
              format: date-time

    # Access Request schemas
    AccessRequestCreate:
      type: object
      properties:
        permissionLines:
          type: array
          items:
            $ref: '#/components/schemas/PermissionLineCreate'

    PermissionLineCreate:
      type: object
      required: [entityId]
      properties:
        entityId:
          type: integer
          format: int64
        hasReportingAccess:
          type: boolean
        hasCasesAccess:
          type: boolean
        isEntityAdministrator:
          type: boolean
        entityEmailForNotifications:
          type: string

    AccessRequestUpdate:
      type: object
      properties:
        permissionLines:
          type: array
          items:
            $ref: '#/components/schemas/PermissionLineCreate'

    AccessRequest:
      type: object
      properties:
        id:
          type: string
          format: uuid
        userId:
          type: string
          format: uuid
        status:
          type: string
          enum: [Working, New, Accepted, Blocked, Updated]
        permissionLines:
          type: array
          items:
            $ref: '#/components/schemas/PermissionLine'
        submittedDate:
          type: string
          format: date-time
        createdDate:
          type: string
          format: date-time

    PermissionLine:
      type: object
      properties:
        id:
          type: string
          format: uuid
        entityId:
          type: integer
          format: int64
        entityName:
          type: string
        hasReportingAccess:
          type: boolean
        hasCasesAccess:
          type: boolean
        isEntityAdministrator:
          type: boolean
        isBlocked:
          type: boolean
        grantedDate:
          type: string
          format: date-time

    AccessRequestList:
      type: object
      properties:
        items:
          type: array
          items:
            $ref: '#/components/schemas/AccessRequest'
        totalCount:
          type: integer
        page:
          type: integer
        pageSize:
          type: integer

    # Report schemas
    Report:
      type: object
      properties:
        id:
          type: string
          format: uuid
        entityId:
          type: integer
          format: int64
        entityName:
          type: string
        fileName:
          type: string
        fileSize:
          type: integer
          format: int64
        reportType:
          type: string
        reportingPeriod:
          type: string
        validationStatus:
          type: string
          enum: [Working, Transmitted, Ongoing, Successful, ValidationErrors, TechnicalError, TimeoutError, ContestedByUKNF]
        uniqueValidationId:
          type: string
        isArchived:
          type: boolean
        isCorrectionOf:
          type: string
          format: uuid
        submittedDate:
          type: string
          format: date-time
        validationCompletedDate:
          type: string
          format: date-time
        errorDescription:
          type: string

    ReportList:
      type: object
      properties:
        items:
          type: array
          items:
            $ref: '#/components/schemas/Report'
        totalCount:
          type: integer
        page:
          type: integer
        pageSize:
          type: integer

    # Message schemas
    Message:
      type: object
      properties:
        id:
          type: string
          format: uuid
        subject:
          type: string
        body:
          type: string
        senderId:
          type: string
          format: uuid
        senderName:
          type: string
        messageStatus:
          type: string
          enum: [AwaitingUknfResponse, AwaitingUserResponse, Closed]
        contextType:
          type: string
        contextId:
          type: string
          format: uuid
        sentDate:
          type: string
          format: date-time
        readDate:
          type: string
          format: date-time
        attachments:
          type: array
          items:
            $ref: '#/components/schemas/Attachment'

    MessageThread:
      type: object
      properties:
        parentMessage:
          $ref: '#/components/schemas/Message'
        replies:
          type: array
          items:
            $ref: '#/components/schemas/Message'

    MessageList:
      type: object
      properties:
        items:
          type: array
          items:
            $ref: '#/components/schemas/Message'
        totalCount:
          type: integer
        page:
          type: integer
        pageSize:
          type: integer

    Attachment:
      type: object
      properties:
        id:
          type: string
          format: uuid
        fileName:
          type: string
        fileSize:
          type: integer
          format: int64
        contentType:
          type: string
        uploadedDate:
          type: string
          format: date-time

    # Case schemas
    CaseCreate:
      type: object
      required: [title, entityId, category]
      properties:
        title:
          type: string
        description:
          type: string
        entityId:
          type: integer
          format: int64
        category:
          type: string
          enum: [RegistrationDataChange, StaffChange, EntityCall, SystemEntitlements, Reporting, Other]
        priority:
          type: string
          enum: [Low, Medium, High]
        isDraft:
          type: boolean

    CaseUpdate:
      type: object
      properties:
        title:
          type: string
        description:
          type: string
        priority:
          type: string

    Case:
      type: object
      properties:
        id:
          type: string
          format: uuid
        caseNumber:
          type: string
        title:
          type: string
        description:
          type: string
        entityId:
          type: integer
          format: int64
        entityName:
          type: string
        category:
          type: string
        priority:
          type: string
        status:
          type: string
          enum: [Draft, New, Ongoing, ToBeCompleted, Cancelled, Completed]
        createdDate:
          type: string
          format: date-time
        completedDate:
          type: string
          format: date-time

    CaseList:
      type: object
      properties:
        items:
          type: array
          items:
            $ref: '#/components/schemas/Case'
        totalCount:
          type: integer
        page:
          type: integer
        pageSize:
          type: integer

    # Library schemas
    LibraryFile:
      type: object
      properties:
        id:
          type: string
          format: uuid
        fileName:
          type: string
        description:
          type: string
        reportingPeriod:
          type: string
        category:
          type: string
        version:
          type: string
        isCurrentVersion:
          type: boolean
        isArchived:
          type: boolean
        fileSize:
          type: integer
          format: int64
        uploadedDate:
          type: string
          format: date-time

    LibraryFileList:
      type: object
      properties:
        items:
          type: array
          items:
            $ref: '#/components/schemas/LibraryFile'
        totalCount:
          type: integer
        page:
          type: integer
        pageSize:
          type: integer

    # FAQ schemas
    FAQ:
      type: object
      properties:
        id:
          type: string
          format: uuid
        title:
          type: string
        questionContent:
          type: string
        answerContent:
          type: string
        category:
          type: string
        tags:
          type: string
        status:
          type: string
          enum: [Submitted, Answered, Published, Archived]
        averageRating:
          type: number
        viewCount:
          type: integer
        createdDate:
          type: string
          format: date-time
        answeredDate:
          type: string
          format: date-time

    FAQList:
      type: object
      properties:
        items:
          type: array
          items:
            $ref: '#/components/schemas/FAQ'
        totalCount:
          type: integer
        page:
          type: integer
        pageSize:
          type: integer

    # Bulletin Board schemas
    BulletinBoardCreate:
      type: object
      required: [title, content, category, priority]
      properties:
        title:
          type: string
        content:
          type: string
          description: HTML content from WYSIWYG editor
        category:
          type: string
        priority:
          type: string
          enum: [Low, Medium, High]
        expiryDate:
          type: string
          format: date-time
        requiresReadConfirmation:
          type: boolean
        recipientCriteria:
          type: object
          description: Defines who receives the announcement

    BulletinBoard:
      type: object
      properties:
        id:
          type: string
          format: uuid
        title:
          type: string
        content:
          type: string
        category:
          type: string
        priority:
          type: string
        publishedDate:
          type: string
          format: date-time
        expiryDate:
          type: string
          format: date-time
        requiresReadConfirmation:
          type: boolean
        isPublished:
          type: boolean
        hasRead:
          type: boolean
          description: Whether current user has read this announcement

    BulletinBoardList:
      type: object
      properties:
        items:
          type: array
          items:
            $ref: '#/components/schemas/BulletinBoard'
        totalCount:
          type: integer
        page:
          type: integer
        pageSize:
          type: integer

    EntityList:
      type: object
      properties:
        items:
          type: array
          items:
            $ref: '#/components/schemas/Entity'
        totalCount:
          type: integer
        page:
          type: integer
        pageSize:
          type: integer
```

---

## **Rationale:**

1. **OpenAPI 3.0 Standard**: Industry-standard API documentation format, auto-generates client SDKs and Swagger UI for testing

2. **Complete Coverage**: All modules from PRD covered - Communication (Reports, Messages, Cases, Library, FAQ, Bulletin Board), Auth/Authorization, and Admin

3. **JWT Authentication**: Simple bearer token authentication appropriate for hackathon (Bearer Auth security scheme)

4. **Pagination**: All list endpoints support pagination (page, pageSize parameters) as required by PRD for large datasets

5. **Filtering**: List endpoints include relevant filters (status, entity, dates) for the PRD's advanced table filtering requirements

6. **File Uploads**: Proper multipart/form-data for file uploads with chunked upload support for large files

7. **Error Responses**: Standardized error response schema with validation details and correlation IDs for debugging

8. **Hackathon-Ready**: Focused on demonstrating PRD requirements without over-engineering; can be extended later
