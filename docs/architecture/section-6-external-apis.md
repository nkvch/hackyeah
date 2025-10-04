# **Section 6: External APIs**

For the hackathon demo, all "external" services will be **mocked, simulated, or self-hosted** within the Docker Compose environment.

---

## **1. Report Validation Service (Mock Implementation)**

- **Purpose:** Validate submitted financial reports - **we'll build a simple mock validator**
- **Implementation:** Internal service in the monolith that simulates validation
- **Base URL(s):** Internal service call (no external API)
- **Authentication:** Not applicable (internal)

**Mock Validation Logic:**
- Parse XLSX file structure (validate it's a proper Excel file)
- Check for required worksheets/columns based on report type
- Use provided test files (G. RIP100000_Q1_2025.xlsx = pass, G. RIP100000_Q2_2025.xlsx = fail)
- Generate PDF result with UKNF format (QuestPDF library)
- Simulate processing delay (5-10 seconds) to show async workflow
- Return validation statuses as per PRD requirements

**Integration Notes:**
- No external API needed - just async processing via RabbitMQ
- Simplified for demo but follows all PRD status requirements
- Can easily be replaced with real external API later by implementing Refit interface

---

## **2. Email Service (Mock/Dev Container)**

- **Purpose:** Email notifications - **use MailDev for demo**
- **Implementation:** MailDev Docker container (catches all emails, provides web UI)
- **Base URL(s):** `smtp://maildev:1025` (SMTP), `http://maildev:1080` (Web UI to view emails)
- **Authentication:** None required for MailDev

**Integration Notes:**
- MailDev catches all outgoing emails and displays them in a web interface
- No emails actually sent outside the system (perfect for demo)
- Shows all notification flows work correctly
- Zero external dependencies

---

## **3. Authentication (Self-Hosted, Simplified)**

- **Purpose:** User authentication with JWT tokens
- **Implementation:** **Simplified custom JWT implementation** (no Duende IdentityServer licensing needed)
- **Base URL(s):** Internal service (`/api/auth/*`)
- **Authentication:** JWT Bearer tokens

**Integration Notes:**
- Use ASP.NET Core Identity + custom JWT generation (no external IdP)
- Simpler than Duende IdentityServer (which requires license for commercial use)
- OAuth2/OIDC mentioned in PRD but simple JWT is sufficient for demo
- All authentication logic in Authentication Service

---

## **4. Virus Scanning (Optional for Demo)**

- **Purpose:** Scan uploaded files for malware
- **Implementation Options:**
  1. **Skip for demo** - just validate file extensions and sizes
  2. **ClamAV container** - if you want to show the feature (self-hosted, free)

**Recommendation:** **Skip actual virus scanning for hackathon** - just implement file type/size validation
- Validates allowed extensions (PDF, DOC, XLSX, CSV, TXT, MP3, ZIP)
- Checks file size (max 100MB for attachments)
- Simulates "scanning" with instant pass
- Can add ClamAV later if time permits

---

## **5. Object Storage (Self-Hosted MinIO)**

- **Purpose:** Store files (reports, attachments, library files)
- **Implementation:** MinIO Docker container (S3-compatible, fully open-source)
- **Base URL(s):** `http://minio:9000` (API), `http://localhost:9001` (Web Console)
- **Authentication:** Access Key + Secret Key (configured in docker-compose)

**Integration Notes:**
- Fully self-contained in Docker Compose
- No cloud provider account needed
- Works identically to AWS S3 (same API)
- Web console available for browsing uploaded files

---

## **Revised External Dependencies Summary**

| Service | Implementation | Why |
|---------|---------------|-----|
| Report Validator | **Mock service (internal)** | No access to actual UKNF validator; build simple validator logic |
| Email/SMTP | **MailDev container** | Catch emails in web UI for demo; no external SMTP needed |
| Authentication | **Custom JWT (ASP.NET Identity)** | Avoid Duende licensing; simpler for demo |
| Virus Scanner | **Skip or stub** | Not critical for demo; validate file types only |
| Object Storage | **MinIO container** | Self-hosted S3-compatible storage; fully contained |
| Database | **PostgreSQL container** | Self-hosted; no cloud needed |
| Cache/Session | **Redis container** | Self-hosted; standard Docker image |
| Message Queue | **RabbitMQ container** | Self-hosted; standard Docker image |

**Result: 100% self-contained demo with zero external API dependencies**
