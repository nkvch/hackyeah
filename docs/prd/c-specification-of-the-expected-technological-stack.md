# C. Specification of the expected technological stack

## 1. Introduction

This document specifies the technology stack of the user-to-user communication and file transfer portal. The portal will be built on the basis of Open Source technologies in accordance with modern programming standards as a SPA (Single Page Application) type application. with backend architecture based on REST API.

## 2. System architecture

### 2.1 General architecture

The system will consist of the following layers:

- Frontend: SPA application responsible for the user interface
- Backend: REST API supporting business logic and data access
- Database: Relational database storing system data
- The queuing system: Processing of asynchronous operations

## 3. Technologies

### 3.1 Frontend

#### 3.1.1 Framework

- Angular 20.x

#### 3.1.2 Component libraries

- PrimeNG using one of the default themes
- Tailwind CSS

#### 3.1.3 UI/UX requirements

- Compatibility: Support for the latest versions of browsers (Chrome, Firefox, Safari, Edge)
- Responsiveness: Adaptation to different devices (computer, tablet, smartphone)
- Availability: Compliant with WCAG 2.2 standard, the ability to switch the interface to high contrast mode
- Typography: Clear fonts and appropriate text sizes

### 3.2 Backend - Java version

#### 3.2.1 Platform

- OpenJDK 21

#### 3.2.2 Framework

- Spring Boot 3.5 or later

#### 3.2.3 ORM

- Hibernate 6.x or higher

#### 3.2.4 Architecture

- REST API
- CQRS (Command Query Responsibility Segregation)
- Dependency Injection

#### 3.2.5 Validation

- Bean Validation (JSR 380)
- Spring Validation

### 3.3 Backend - .NET version

#### 3.3.1 Platform

- .NET 8 or .NET 9 (LTS or latest)

#### 3.3.2 Framework

- ASP.NET Core Web API

#### 3.3.3 ORM

- Entity Framework Core (EF Core)

### 3.3.4 Architecture

- REST API
- CQRS (Command Query Responsibility Segregation)
- Dependency Injection (built-in DI container in .NET Core)

#### 3.3.5 Validation

- FluentValidation (suggested)
- System.ComponentModel.DataAnnotations (alternative)

### 3.4 Database

- MS SQL Server or other relational database

### 3.5 The queuing system

- Apache Kafka or RabbitMQ (if used)

### 3.6 Containerisation

- Docker with docker-compose.yml/compose.yml

### 3.7 Operating system (production environment)

- Linux
- Windows

## 4. Security

### 4.1 Authentication and authorization

- OAuth 2.0 / OpenID Connect
- JWT (JSON Web Token)
