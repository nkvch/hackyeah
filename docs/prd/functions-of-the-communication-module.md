# Functions of the Communication Module

The module is designed to enable:

➢ handling the acquisition of reports submitted by supervised entities with feedback from validation of the status of reports,
➢ handling messages with the possibility of attaching attachments ensuring two-way communication between internal and external users,
➢ maintaining a local file repository that performs the library function,
➢ handling and handling cases concerning supervised entities,
➢ handling messages in the form of a bulletin board, with the option to all or to selected groups of entities, with confirmation of reading by representatives of entities,
➢ service of addressees, contact groups and contacts,
➢ maintaining a database of questions and answers giving the opportunity to ask questions and view answers in the FAQ convention,
➢ handling the file of entities and updating information on supervised entities.

## Description of the function

Report acquisition services are provided by:

- handling the acquisition of reports submitted to the UKNF by Supervised Entities with feedback from validation of the status of reports, in particular:
- uploading a file containing the report by the entity using the file attachment option. An employee of the Entity submits reports to the Supervised Entity in the form of MS Excel files in XLSX format. The templates of reporting files to be filled in each time downloads from the local repository of files - Libraries. Once submitted, the reports are automatically verified and subject to validation for compliance with the rules defined by the UKNF. The report's will be updated automatically. At the end of the validation, a report with the result of the analysis of the report and the current status will be attached to the submitted report. The report shall be validated by an external tool that will provide the result of the validation. The validation result file will contain the UKNF markings, the date of receipt of the report, the date of validation and the name of the entity. In addition, in the case of negative (technical errors, validation errors, etc.), the result file will contain the relevant information about the processing error. Attached are two regular reports with correct (G. RIP100000_Q1_2025.xlsx) and incorrect (G. RIP100000_Q2_2025.xlsx) data,
- display status, errors and technical and substantive validation report.

## Examples of validation statuses:

| Importance | Validation status |
|---|---|
| Working | transitional, set after adding a report file |
| Transmitted | transitional, set after the start of the validation process of the report. Confirmed with a unique ID. |
| Ongoing | transitional, set at the moment when the processing of the report is ongoing |
| Successful validation process | the processing of the report was successful and no validation errors were found as a result of the validation of the report. The report has been recorded and the data from the report will be analysed. |
| Errors from validation rules | the processing of the report has been completed, but errors in the validation rules have been detected as a result of the validation of the report |
| Technical error in the validation process | the processing of the report resulted in a validation process error |
| Error - Exceeded time | automatically set when the report processing process is not completed within 24 hours of adding the report file |
| Contested by UKNF | set only on demand in a situation where the UKNF Employee uses the action "Challenge" with the addition of the content of the field "Description of irregularities" |

- the possibility of reviewing the results of validation of reporting files submitted to the UKNF and monitoring changes in the status of reports submitted,
- the possibility for an employee of the UKNF to categorise the reports (organising in the so-called Report Registers) on the basis of metadata provided in files as registers, e.g. 'Quarterly reports', 'Annual reports' including 'current' and 'archival' – the possibility for a UKNF employee to transfer (mark) the report to the archive through the action 'Archiving the report',
- possibility to handle corrections of reports. An external user may submit a correction to the submitted report. The correction should be linked in the system to the adjusted report,
- the possibility of displaying, by an employee of the UKNF, in the form of summaries of the so-called Report logs with basic information about the report, including data of the user who added the report, i.e. Name, Surname, E-mail, Telephone, whether a correction has been made and filtering of these registers based on the quick filters "My entities" (reports of entities assigned to "My entities" at the UKNF Employee) and the filter of the validation status of the reporting period, etc.,
- the possibility for an employee of the UKNF to view information about the report in the details view, including the file, name, number, person submitting, entity submitting, reporting period, validation status, complex corrections, etc. metadata,
- maintaining the so-called reporting calendar (timetable), which, among others: informs entities about upcoming dates of sending reports, handles reminders, reports the progress and completeness of data flows within a specific reporting action,
- use of the Message Handling function in reporting - support for two-way communication between internal users (UKNF Employees) and external users (Administrators of the Supervised Entity and Employees of the Supervised Entity,
- the possibility for an employee of the UKNF to display in the form of a list of Entities that did not submit the selected report (there is no report submitted in the system with the status 'Validation process successfully completed' for the selected period) and to generate a new message.

## Message handling provides:

- two-way communication between internal users (UKNF Employees) and external users (Administrators of the Supervised Entity and Employees of the Supervised Entity), in particular:
- support for messages with the possibility of attaching attachments in allowed formats, i.e. PDF, DOC/DOCX, XLS/XLSX, CSV/TXT, MP3 and zip packed files provided that the total size of the pre-packaged files does not exceed 100 MB,
- rejection of files with unexpected formats or exceeding the file size,
- rejecting messages from infected and containing SPAM,
- use of communication in system components - in access requests (conversation within the framework of external users' requests), cases, acquisition of reports,
- the ability to mark message statuses. Message statuses:

| Status | Importance |
|---|---|
| Awaits UKNF's response | Message Added by an External User |
| Awaiting the User's Response | message added by an employee of the UKNF |
| Closed | message for which there is a response from the UKNF Employee/External User |

- the possibility of displaying in the form of a list of messages addressed to entities by UKNF Employees and filtering the list based on quick filters: 'My entities' (requests of entities assigned to 'My entities' by a UKNF Employee), 'Requires UKNF responses' (messages with the status 'Pending UKNF response') and the filter of the component in which the message was used,
- Ensuring that supervised entities and external users can be grouped together (categorisation of recipients) in order to inform them simultaneously. Transmission of mass messages by the UKNF to designated Supervised Entities (selected from the list or by market types or by indicating groups).

## Maintaining the local file repository ensures:

- maintaining a repository of files in the form of a Library for the purpose of making them available to system users (e.g. report templates in Excel file format, instructions, descriptions, etc.), the repository of files is a common resource of all external Users. External users decide which files are accessed by the UKNF Employee. The repository shall ensure in particular:
- adding a file and modifying information about the file (metadata) by UKNF Employees – e.g. fields 'Filename', 'Reporting period', 'Model update date' and 'Annex' (template file). The UKNF employee has access to all files in the repository and can add, modify and delete files and their metadata,
- sharing files with all or individual users and selected groups of users,
- possibility of categorizing, filtering and sorting files,
- the ability to view the history of the file and view the history of changes,
- marking files available in the current and archival versions with the date of the last update.

## Handling and handling cases in administrative mode provides:

- handling and conducting administrative cases concerning supervised entities, in particular:
- creating cases (registration), assigning and monitoring progress on individual cases (case statuses). Cases are conducted between the Employees of the Supervised Entity and the Employees of the UKNF in the context of only one selected Supervised Entity and concerning one of the categories, e.g.: 'Change of registration data', 'Change of staff composition', 'Call to the Supervised Entity', 'Entitlements to the System', 'Reporting', 'Other' with prioritisation: 'Low', 'Medium' or 'High'.

## Examples of case statuses:

| Status of the case | Importance |
|---|---|
| Draft | set after saving the draft case. Such matters are not visible to the UKNF Employee |
| New case | set when the case is started/transferred. Such matters are visible to the UKNF Employee |
| Ongoing | set automatically after the case is opened by an Employee of the Supervised Entity or an Employee of the UKNF |
| To be completed | set by the Employee of the UKNF, indicates the need for the Employee of the Supervised Entity to supplement the information or annex on the |
| Cancelled | set by the UKNF Employee, possible to set if the Employee of the Supervised Entity has not yet become acquainted with it |
| Completed | status of the case set by the UKNF Employee, means the end of the case |

- storing and managing electronic documents related to individual cases – adding attachments to the case,
- cancellation of a case that has not yet been taken by the Employee of the Supervised Entity. A mechanism that allows, in the event of the creation of a message about the assignment of a new case that has been sent to an Employee of the Supervised Entity, to cancel that message (case) unless the employee has already become acquainted with it (i.e. has the status: The New Case). The content of the message will no longer be available to the Employee of the Supervised Entity and will instead see the message "cancelled message". In addition, a notification will be sent to the entity about the cancellation of the message. By the UKNF, such a case will be visible with Cancelled status. In such a case, the editing possibilities will be blocked,
- handling of messages - two-way communication between internal users (UKNF Employees) and external users (Administrators of the Supervised Entity and Employees of the Supervised Entity. Transmission of electronic messages as part of the case or in connection with the report, with annexes, by Supervised Entities to the UKNF and by the UKNF to Supervised Entities,
- the ability to view the Cases list with basic information about the cases, including the data of the user who added the case, i.e. Name, Surname, E-mail, Telephone,
- the possibility of reviewing information about the case in the view of details, including the name, number, entity concerned, case handler, category, messages transmitted as part of the case, etc. metadata,
- a preview of the history of changes in the case by the UKNF Employee and an external user.

## Handling messages in the form of a bulletin board ensures:

- handling messages made available by employees of the UKNF in the form of a bulletin board with the possibility of "read-only" with the registration of reading by representatives of entities, in particular:
- adding and publishing advertisements by authorized internal users,
- editing the content of the message using the WYSWIG editor,
- editing, removing from publications and removing existing redundant notices,
- attaching files (attachments) to messages,
- viewing a message with a history of changes,
- setting the expiry date of announcements,
- setting the priority for the message, i.e. 'Low', 'Medium', 'High',
- assigning notices to different categories (e.g. general information, events),
- defining groups of recipients, i.e. to individual external users, groups of external users or all external users,
- displaying announcements on the start page in the application to specific groups of recipients,
- an indication, e.g. a distinction on the part of the entity when it receives a new message,
- the ability to display a list of messages with filtering options,
- monitoring the activity of users in the field of message reading (statistics of message reading by entities, e.g. 71/100 entities) and popularity of announcements,
- the ability to set the maturity of the confirmation of a message read by an external User depending on the defined priority of the message. For 'High' priority messages, the user must click that they have read it. The confirmation will show the date and time of reading, the name of the user and the name of the entity he represented.

## Support for addressees, contact groups and contacts provides:

- Add and modify recipients, contact groups, and contacts. Recipients should be understood as external Users who are notified about events in the system (e.g. about adding a new Message) or receive permissions to selected files in the Library. There are four types of adding Addressees:

| Type | Description |
|---|---|
| Selected entities | the addressee is any external User who is associated with any of the selected Entities from the list |
| Selected contact groups | so-called contact database - the addressee is each external User assigned to the selected Contact Group. |
| Selected types of entities | the addressee is any external User whose Entity is associated with the type of entity, e.g. 'Loan institution'. |
| Selected users | the addressee is any external user selected from the list of users |

The Contact Group is a list of selected Representatives of external entities and Users who may receive e-mail notifications from UKNF Employees. A person who is a member of such a Contact Group does not need to have access to the System, i.e. it does not need to be an external User. Adding such a person is possible by adding a new so-called. It's a contact. Contacts may include recipients who do not have to be users of the System. Employees of the UKNF may notify such persons by e-mail-about various events, e.g. assigning them to a selected Contact Group.

## Maintaining the Q&A database ensures:

- Possibility to ask and receive answers to questions and view anonymized questions with answers (so-called. Knowledge base FAQ), in particular:
- Adding new questions anonymously. Questions should have fields: title, content, category, labels, date added, status.
- adding answers to questions by the UKNF Employee,
- management of questions and answers - the possibility of modifying or removing questions/answers, changing the content, status and other fields by the UKNF Employee or the System Administrator,
- categorisation of questions and answers – grouping into categories,
- support for labels (tags) for better filtering,
- assessment of responses (e.g. asterisks 1-5),
- search by keyword, title, content, category or label.
- sorting the results by: popularity, date added, ratings.
- filtering by category, tags, response status.

## Handling the file of entities and updating information on supervised entities ensures:

- the possibility of maintaining information on supervised entities in the form of an Entity File in the system, in particular:
- adding and modifying data on the entity supervised by the System Administrator and the UKNF Employee, in accordance with the structure of:

| Name | fields type | Description |
|---|---|---|
| ID | bigint | |
| Type of entity | nvarchar(250) | The role of the entity in the Entity Database, e.g. a loan institution |
| UKNF code | nvarchar(250) | code generated in the Entity Database, uneditable, |
| Name of the entity | nvarchar(500) | |
| lei | nvarchar(20) | |
| NIP | nvarchar(10) | |
| KRS | nvarchar(10) | |
| Street | nvarchar(250) | |
| Building number | nvarchar(250) | |
| Number of the premises | nvarchar(250) | |
| Postcode | nvarchar(250) | |
| City | nvarchar(250) | |
| Telephone | nvarchar(250) | A string of numbers and characters defining the phone number. Verification of the correctness of the phone number taking into account the international format (eg. /^\+(?:[0-9] ?){6,14}[0-9]$/) |
| E-mail | nvarchar(500) | E-mail address containing the @ character |
| UKNF registration number | nvarchar(100) | Identification number assigned in the UKNF register |
| Entity status | nvarchar(250) | The status of the entity in the Entity Database, e.g. Entered, Deleted |
| Category of entity | nvarchar(500) | Name of the entity category in the Entity Database |
| Sector of the operator | nvarchar(500) | Name of the Entity Sector in the Entity Database |
| Entity sub-sector | nvarchar(500) | Name of the entity sub-sector in the Operators Database |
| Cross-border entity | bit | checkbox checked/unchecked |
| Date of creation | datetime2(7) | |
| Date of update | datetime2(7) | |

- reviewing the history of changes in data about the entity,
- a preview of detailed data of the entity supervised by the System Administrator and the UKNF Employee,
- preview of the list of all Users of the Entity assigned in the system to a given Entity Supervised by the Entity Administrator, the System Administrator and the UKNF Employee,
- a preview of the details of the entity by external Users assigned to the entity in order to be able to verify the compliance of the data with the facts and report a change in the registration data (name of the entity, address data, telephone, e-mail) in the event of non-compliance or change in their change, through a case with the category "Change of registration data" (the report generates a new case).

## Updating information on supervised entities (entity data updater service) provides:

- the possibility of updating data on supervised entities (registration data) cooperating with the UKNF contained in the Entities Database, in order to store reliable information about entities and their relationships and history of changes, in particular:
- updating the data on the entity supervised by an authenticated Administrator of the Supervised Entity using the change form made available in the system (Name of the entity, Address: street, postcode and city Telephone, E-mail).
- versioning of changes in data about the entity - stored historical data, available for inspection by the PFSA Employee,
- verification of updated data by the UKNF Employee and after correct verification recording them in the Entity Database,
- scheduling, for alerts cyclically appearing during external user access sessions with confirmation of current data of the supervised entity.
