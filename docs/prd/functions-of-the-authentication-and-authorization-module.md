# Functions of the Authentication and Authorization Module

The module is designed to enable:

➢ registration of external users via an online form,
➢ handling access requests with assigning permissions to external users,
➢ selection of the entity represented in the session by an authenticated external user.

## Description of the function

The registration of external users shall ensure:

- registration of external users via the online form, in particular:
- registration of external users (Administrators of the Supervised Entity, Employee of the Supervised Entity),
- creation of an account by an external user via an online form (mandatory fields: First name, Last name, PESEL [masked, last 4 digits visible], phone, e-mail),
- sending a link to activate the account to the user's e-mail address and setting the user's password to the system in accordance with the adopted policy of creating passwords in the system.

The handling of access requests shall ensure:

- handling access requests with granting/removing the right to individual system functions for the selected entity with the possibility of updating them and adding annexes, in particular:
- after the correct activation of the user account in the system (registration), automatic generation of the request for access with the status "Working",
- editing of the access request by the entity's representative, mandatory fields: First name, Last name, PESEL [masked, visible last 4 digits], phone number, e-mail downloaded from the form when registering the user,
- adding powers within a given Supervised Entity by selecting in the application, by the Internal User, from the list of entities (list of entities made available by the UKNF - Directory of Entities) supervised entities with which it will be associated and which will represent the so-called line of powers,
- assigning the permission requested by the user by marking the permission in checkboxes, e.g. permissions: Reporting, Cases, Entity Administrator,
- supplementing the e-mail address of the Supervised Entity, which will result in assigning the given e-mail address to the selected Entity and sending automatic e-mail notifications to this address, e.g. in the case of sending a report by the Employee of the Supervised Entity,
- after the access request has been validated by the external user, the message confirming the submission of the request and the change of the status of the request from 'Rollover' to 'New' is displayed,
- sending an automatic confirmation of submitting an access request to the e-mail address of the external user indicated during registration,
- acceptance of the application for access by an employee of the UKNF (for the Administrator of the Supervised Entity) or by the Administrator of the Supervised Entity (for the Employee of the Supervised Entity),
- after accepting the access request, display a message confirming the acceptance and change of the status of the request from "New" to "Accepted",
- possibility to mark statuses of applications. Application statuses:

| Status | Importance |
|---|---|
| Working | a request that has not yet been submitted for acceptance |
| New | an application that has been completed and submitted for approval |
| Accepted | request where all lines of allowances have been accepted |
| Blocked | request where all permission lines have been blocked |
| Updated | a proposal that has been modified and is awaiting re-acceptance |

- the ability of the UKNF Employee to communicate with an external user via electronic messages available at the level of the access request. For example, when verifying an access request in the event of an irregularity, the UKNF Employee may create and send a message to an external user asking for clarifications, with the possibility of adding attachments,
- the possibility of displaying in the form of a list of all applications registered in the system by the UKNF Employees and filtering them based on quick filters: 'My Entities' (applications of entities assigned to 'My Entities' with a UKNF Employee), 'Requires
- the possibility of displaying in the form of a list of registered applications of the Supervised Entity by the Administrator of this Supervised Entity,
- preview the history of the access request, the possibility to view the history of changes,
- preview by the UKNF Employee of the permission line of external users,
- blocking the rights of the Administrator of the Entity Supervised by UKNF Employees. Blocking the rights of the Entity Administrator will result in the Administrator losing access to the system. If the entity for which the Administrator has been blocked is assigned a second / different Administrator of the entity, it will be able to continue to accept and manage access requests of other users assigned to this entity. Blocking the Administrator of the Supervised Entity by the UKNF will not modify or block the rights of other users of the entity to whom the blocked Administrator accepted the rights. If for the entity in which the Administrator has been blocked, the second/other Administrators of the entity will not be accepted, then the modification and acceptance of the rights for the other users of the entity will require the acceptance of the UKNF,
- management by the Administrator of the Supervised Entity of the rights of the Employees of the Supervised Entity, including the possibility of modifying the rights of the Employees of the Supervised Entity in the scope, access to the Reporting modules (access / lack of access), Cases (access / lack of access), assigned Entities and access to the system (the possibility of blocking access to the system) only in the scope of the entity to which the Administrator of the Supervised Entity has rights.

## The selection of the entity represented in the session shall ensure:

- Selection, by an authenticated external user, of the entity represented in the session, i.e. in the selection of the entity to which the external user has assigned permissions,
- displaying on further navigation screens information in which context and role the user works.
