# A. Specification of functional requirements

## Category: Logical architecture

As regards the exchange of information related to the performance of supervisory duties, the KNF needs an application for secure and fast communication with supervised entities.

The system should consist of three main modules:

[preferred functionalities]
- **the Communication Module associated** with the **Entity Data Updater** service

[additional functionalities]
- **the Authentication and Authorization Module,**
- **the Administrative Module.**

```
@startuml
!define COMPONENT_STYLE rectangle

skinparam rectangle {
    BackgroundColor White
    BorderColor Black
}

skinparam actor {
    BackgroundColor White
    BorderColor Black
}

skinparam database {
    BackgroundColor White
    BorderColor Black
}

title Business Architecture - Communication Platform

actor "Użytkownik\nzewnętrzny" as external_user
actor "Użytkownik\nwewnętrzny" as internal_user

database "Baza Podmiotów" as subject_db

package "PLATFORMA KOMUNIKACYJJNA" {
    
    package "Moduł Komunikacyjny" as comm_module {
        COMPONENT_STYLE "Sprawozdania" as reports
        COMPONENT_STYLE "Komunikaty" as messages
        COMPONENT_STYLE "Sprawy" as cases
        COMPONENT_STYLE "Biblioteka" as library
    }
    
    COMPONENT_STYLE "Aktualizator danych podmiotu" as data_updater
    
    package "Moduł Uwierzytelniania i Autoryzacji" as auth_module {
        COMPONENT_STYLE "Uwierzytelnienie" as authentication
        COMPONENT_STYLE "Wnioski" as requests
        COMPONENT_STYLE "Autoryzacja" as authorization
    }
    
    package "Moduł Adminstracyjny" as admin_module {
        COMPONENT_STYLE "Zarządzanie\nużytkownikami" as user_mgmt
        COMPONENT_STYLE "Polityka haseł" as password_policy
        COMPONENT_STYLE "Role i uprawnienia" as roles
    }
}

COMPONENT_STYLE "Formularz kontaktowy" as contact_form

' Connections
external_user <--> comm_module
comm_module <--> subject_db
comm_module --> data_updater
data_updater ..> subject_db
contact_form --> auth_module
internal_user <--> admin_module

@enduml
```
