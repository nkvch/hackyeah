# End-to-End Tests: Report Submission Flow (Story 4.1)

## Overview
This document outlines the E2E test scenarios for the Report Submission feature (Story 4.1). These tests verify the complete user journey from authentication to successful report submission and validation queue placement.

---

## Test Environment Setup

### Prerequisites
1. **Backend API**: Running on `https://localhost:5001` (or configured URL)
2. **Frontend UI**: Running on `http://localhost:4200` (or configured URL)
3. **Database**: PostgreSQL/SQL Server with migrations applied
4. **File Storage**: Local storage directory created at `./storage/files`
5. **Test User**: 
   - Email: `test.user@example.com`
   - Password: `Test@Password123`
   - Assigned Entity: `Test Bank S.A.` (ID: 1001)
   - Permissions: `communication.reports.submit`

### Test Data
- **Valid XLSX File**: `test-data/valid_report_Q1_2025.xlsx` (size: 5 MB)
- **Large XLSX File**: `test-data/large_report_99MB.xlsx` (size: 99 MB, valid)
- **Oversized File**: `test-data/oversized_report_101MB.xlsx` (size: 101 MB, invalid)
- **Invalid File**: `test-data/invalid_report.pdf` (PDF instead of XLSX)
- **Malicious File**: `test-data/malicious.xlsx.exe` (wrong magic bytes)

---

## E2E Test Scenarios

### **Scenario 1: Successful Report Submission (Happy Path)**

**Objective**: Verify that an authenticated user with proper permissions can successfully submit a valid report.

**Steps**:
1. **Navigate** to the application homepage (`http://localhost:4200`)
2. **Login** with test user credentials
3. **Navigate** to "Submit Report" page via menu or direct URL (`/reports/submit`)
4. **Verify** that the entity selection dropdown is visible (if user has multiple entities)
5. **Select** entity: `Test Bank S.A.`
6. **Fill form**:
   - Report Type: `Quarterly`
   - Reporting Period: `Q1_2025`
7. **Upload file**: Select `valid_report_Q1_2025.xlsx` (5 MB)
8. **Submit** the form
9. **Observe** upload progress bar reaching 100%
10. **Verify** success message displays:
    - "Report submitted successfully!"
    - Report ID is shown
    - Status: "Working"
    - Message: "Your report is being validated."
11. **Verify backend**:
    - Check database: Report record created with status `Working`
    - Check file storage: File saved at `./storage/files/{year}/{guid}/valid_report_Q1_2025.xlsx`
    - Check logs: `ReportSubmittedEvent` published
12. **Verify form reset**: Form clears after 3 seconds

**Expected Result**: ✅ Report submitted successfully, status `Working`, event published.

---

### **Scenario 2: Report Submission with Maximum Allowed File Size**

**Objective**: Verify that the system accepts files at the maximum allowed size (100 MB).

**Steps**:
1. Login and navigate to Submit Report page
2. Fill form with valid data
3. Upload file: `large_report_99MB.xlsx` (99 MB)
4. Submit the form
5. **Observe** extended upload time with progress bar updates
6. Verify success message

**Expected Result**: ✅ Report submitted successfully despite large file size.

---

### **Scenario 3: Report Submission with Oversized File**

**Objective**: Verify that files exceeding 100 MB are rejected with a clear error message.

**Steps**:
1. Login and navigate to Submit Report page
2. Fill form with valid data
3. Attempt to upload file: `oversized_report_101MB.xlsx` (101 MB)
4. **Client-side validation** should trigger immediately:
   - Error message: "File size exceeds 100 MB limit."
   - Submit button disabled
5. If bypassed, submit the form
6. **Server-side validation** should reject with HTTP 413 or 400

**Expected Result**: ❌ Submission blocked, error message displayed.

---

### **Scenario 4: Report Submission with Invalid File Type**

**Objective**: Verify that non-XLSX files are rejected.

**Steps**:
1. Login and navigate to Submit Report page
2. Fill form with valid data
3. Attempt to upload file: `invalid_report.pdf`
4. **Client-side validation** should trigger:
   - Error message: "Only .xlsx files are allowed."
   - Submit button disabled
5. If bypassed, server-side should reject with HTTP 400

**Expected Result**: ❌ Submission blocked, file type error displayed.

---

### **Scenario 5: Report Submission with Malicious File (Security Test)**

**Objective**: Verify that files with invalid magic bytes are rejected (e.g., renamed executable).

**Steps**:
1. Login and navigate to Submit Report page
2. Fill form with valid data
3. Attempt to upload file: `malicious.xlsx.exe` (renamed file with wrong magic bytes)
4. Submit the form
5. **Backend validation** should reject:
   - HTTP 400 response
   - Error: "Invalid file. Only XLSX files up to 100 MB are allowed."

**Expected Result**: ❌ Submission blocked by magic bytes validation, security error logged.

---

### **Scenario 6: Report Submission with Missing Required Fields**

**Objective**: Verify form validation for missing fields.

**Steps**:
1. Login and navigate to Submit Report page
2. Leave `Entity ID` empty (if applicable)
3. Attempt to submit
4. Verify validation error: "Entity is required."
5. Fill entity, leave `Report Type` empty
6. Attempt to submit
7. Verify validation error: "Report type is required."
8. Repeat for `Reporting Period` and `File`

**Expected Result**: ❌ Submission blocked for each missing field with specific error messages.

---

### **Scenario 7: Report Submission with Invalid Reporting Period Format**

**Objective**: Verify reporting period format validation.

**Steps**:
1. Login and navigate to Submit Report page
2. Fill form with valid data except:
   - Reporting Period: `InvalidFormat`
3. Attempt to submit
4. Verify validation error: "Reporting period must be in format like Q1_2025, Annual_2025, or Monthly_2025."

**Test Variations**:
- `Q5_2025` (invalid quarter)
- `Q1-2025` (wrong separator)
- `2025_Q1` (wrong order)
- `January_2025` (invalid format)

**Expected Result**: ❌ Submission blocked with format error.

---

### **Scenario 8: Report Submission for Annual Report**

**Objective**: Verify submission works for Annual reports.

**Steps**:
1. Login and navigate to Submit Report page
2. Fill form:
   - Entity: `Test Bank S.A.`
   - Report Type: `Annual`
   - Reporting Period: `Annual_2025`
   - File: Valid XLSX
3. Submit the form
4. Verify success

**Expected Result**: ✅ Annual report submitted successfully.

---

### **Scenario 9: Report Submission for Monthly Report**

**Objective**: Verify submission works for Monthly reports.

**Steps**:
1. Login and navigate to Submit Report page
2. Fill form:
   - Entity: `Test Bank S.A.`
   - Report Type: `Monthly`
   - Reporting Period: `Monthly_2025`
   - File: Valid XLSX
3. Submit the form
4. Verify success

**Expected Result**: ✅ Monthly report submitted successfully.

---

### **Scenario 10: Duplicate Report Submission Prevention**

**Objective**: Verify that duplicate reports for the same entity/period are rejected.

**Steps**:
1. Login and submit a report:
   - Entity: `1001`
   - Report Type: `Quarterly`
   - Reporting Period: `Q1_2025`
2. Verify success (first submission)
3. **Immediately** submit another report with the same details
4. Verify backend rejects with HTTP 400:
   - Error: "A report for this entity, type, and period already exists."

**Expected Result**: ❌ Duplicate submission blocked.

---

### **Scenario 11: Unauthenticated User Access**

**Objective**: Verify that unauthenticated users cannot access the submit report page.

**Steps**:
1. **Do NOT log in**
2. Attempt to navigate to `/reports/submit`
3. Verify redirect to login page (or HTTP 401/403 if accessing API directly)

**Expected Result**: 🚫 Access denied, redirected to login.

---

### **Scenario 12: User Without Permission Access**

**Objective**: Verify that users without `communication.reports.submit` permission are blocked.

**Steps**:
1. Login with a user that **lacks** the permission
2. Attempt to navigate to `/reports/submit`
3. Frontend should hide the menu option (if permission-aware)
4. If directly accessed, verify:
   - HTTP 403 Forbidden
   - Error message: "You do not have permission to submit reports."

**Expected Result**: 🚫 Access denied with permission error.

---

### **Scenario 13: Cancel Report Submission**

**Objective**: Verify that users can cancel the submission and navigate away.

**Steps**:
1. Login and navigate to Submit Report page
2. Fill form partially
3. Click **Cancel** button
4. Verify:
   - Form resets
   - Navigates to home or reports list page

**Expected Result**: ✅ Form cancelled, no data submitted.

---

### **Scenario 14: Upload Progress Indicator**

**Objective**: Verify that the upload progress bar works for large files.

**Steps**:
1. Login and navigate to Submit Report page
2. Fill form and upload a large file (50 MB+)
3. Submit the form
4. **Observe** progress bar:
   - Starts at 0%
   - Updates incrementally (e.g., 25%, 50%, 75%)
   - Reaches 100% before success message
5. Verify "X% uploaded" text updates

**Expected Result**: ✅ Progress bar updates smoothly.

---

### **Scenario 15: Network Failure During Upload**

**Objective**: Verify graceful error handling for network failures.

**Steps**:
1. Login and navigate to Submit Report page
2. Fill form and start file upload
3. **Simulate network failure** (disconnect Wi-Fi or use dev tools)
4. Verify error message:
   - "An error occurred. Please try again."
5. Verify form remains editable (not reset)
6. Reconnect network and retry

**Expected Result**: ❌ Error message displayed, user can retry.

---

### **Scenario 16: Backend Service Unavailable**

**Objective**: Verify error handling when backend is down.

**Steps**:
1. **Stop backend API**
2. Login (cached auth) and navigate to Submit Report page
3. Fill form and attempt to submit
4. Verify error message:
   - "Internal Server Error: Something went wrong on the server."
   - Or appropriate network error

**Expected Result**: ❌ Error message displayed, submission fails gracefully.

---

### **Scenario 17: Multiple Entity Selection**

**Objective**: Verify entity selection for users with multiple entities.

**Steps**:
1. Login with a user assigned to multiple entities (e.g., `User A` with entities `1001`, `1002`)
2. Navigate to Submit Report page
3. Verify entity dropdown is visible and populated with all user entities
4. Select one entity
5. Submit a report
6. Verify report is associated with the correct entity in the database

**Expected Result**: ✅ Entity selection works, report linked to selected entity.

---

### **Scenario 18: Single Entity Auto-Selection**

**Objective**: Verify that users with a single entity have it auto-selected.

**Steps**:
1. Login with a user assigned to **only one entity**
2. Navigate to Submit Report page
3. Verify:
   - Entity dropdown is **hidden** or **pre-selected and disabled**
   - No manual selection needed
4. Submit a report
5. Verify report is associated with the single entity

**Expected Result**: ✅ Entity auto-selected, seamless UX.

---

### **Scenario 19: Accessibility (Screen Reader)**

**Objective**: Verify that the form is accessible to screen readers.

**Steps**:
1. Enable screen reader (NVDA, JAWS, VoiceOver)
2. Navigate to Submit Report page using keyboard only
3. Verify:
   - All form fields are announced correctly
   - File input is accessible
   - Error messages are read aloud
   - Submit/Cancel buttons are keyboard accessible

**Expected Result**: ✅ Form is fully accessible.

---

### **Scenario 20: Cross-Browser Compatibility**

**Objective**: Verify report submission works across major browsers.

**Browsers to Test**:
- Chrome (latest)
- Firefox (latest)
- Safari (latest)
- Edge (latest)

**Steps**:
1. Repeat **Scenario 1** (Happy Path) in each browser
2. Verify consistent behavior and styling

**Expected Result**: ✅ Works identically in all browsers.

---

## Automated E2E Test Implementation (Playwright/Cypress)

### Example Playwright Test (TypeScript)

```typescript
import { test, expect } from '@playwright/test';

test.describe('Report Submission E2E', () => {
  test.beforeEach(async ({ page }) => {
    // Login
    await page.goto('http://localhost:4200/login');
    await page.fill('input[name="email"]', 'test.user@example.com');
    await page.fill('input[name="password"]', 'Test@Password123');
    await page.click('button[type="submit"]');
    await expect(page).toHaveURL('http://localhost:4200/');
  });

  test('Scenario 1: Successful report submission', async ({ page }) => {
    // Navigate to submit report
    await page.goto('http://localhost:4200/reports/submit');

    // Fill form
    await page.selectOption('select[name="entityId"]', '1001');
    await page.selectOption('select[name="reportType"]', 'Quarterly');
    await page.fill('input[name="reportingPeriod"]', 'Q1_2025');
    
    // Upload file
    await page.setInputFiles('input[type="file"]', 'test-data/valid_report_Q1_2025.xlsx');

    // Submit
    await page.click('button[type="submit"]');

    // Wait for success message
    await expect(page.locator('.bg-green-100')).toContainText('Report submitted successfully');
    await expect(page.locator('.bg-green-100')).toContainText('Report ID:');
    await expect(page.locator('.bg-green-100')).toContainText('Working');
  });

  test('Scenario 3: Oversized file rejection', async ({ page }) => {
    await page.goto('http://localhost:4200/reports/submit');

    await page.selectOption('select[name="reportType"]', 'Quarterly');
    await page.fill('input[name="reportingPeriod"]', 'Q1_2025');
    await page.setInputFiles('input[type="file"]', 'test-data/oversized_report_101MB.xlsx');

    // Verify error message
    await expect(page.locator('.text-red-500')).toContainText('100 MB limit');
    
    // Verify submit button is disabled
    await expect(page.locator('button[type="submit"]')).toBeDisabled();
  });

  test('Scenario 11: Unauthenticated access blocked', async ({ page, context }) => {
    // Clear session
    await context.clearCookies();
    
    // Attempt to access submit report page
    await page.goto('http://localhost:4200/reports/submit');

    // Should redirect to login
    await expect(page).toHaveURL(/.*login.*/);
  });
});
```

---

## Test Execution Checklist

- [ ] **Scenario 1**: Happy path ✅
- [ ] **Scenario 2**: Max file size ✅
- [ ] **Scenario 3**: Oversized file ❌
- [ ] **Scenario 4**: Invalid file type ❌
- [ ] **Scenario 5**: Malicious file ❌
- [ ] **Scenario 6**: Missing fields ❌
- [ ] **Scenario 7**: Invalid period format ❌
- [ ] **Scenario 8**: Annual report ✅
- [ ] **Scenario 9**: Monthly report ✅
- [ ] **Scenario 10**: Duplicate prevention ❌
- [ ] **Scenario 11**: Unauthenticated access 🚫
- [ ] **Scenario 12**: Permission check 🚫
- [ ] **Scenario 13**: Cancel action ✅
- [ ] **Scenario 14**: Upload progress ✅
- [ ] **Scenario 15**: Network failure ❌
- [ ] **Scenario 16**: Backend unavailable ❌
- [ ] **Scenario 17**: Multiple entities ✅
- [ ] **Scenario 18**: Single entity auto-select ✅
- [ ] **Scenario 19**: Accessibility ♿
- [ ] **Scenario 20**: Cross-browser 🌐

---

## Notes

1. **Test Data Management**: Create test files using scripts in `test-data/generate_test_files.sh`
2. **Database Cleanup**: Reset test database between runs to avoid duplicate report conflicts
3. **File Storage Cleanup**: Clear `./storage/files` directory after tests
4. **Logging**: Enable verbose logging during E2E tests for debugging
5. **CI/CD Integration**: Run E2E tests in a separate pipeline stage after unit and integration tests

---

## Contact

For questions about these E2E tests, contact the QA team or Story 4.1 developer.

