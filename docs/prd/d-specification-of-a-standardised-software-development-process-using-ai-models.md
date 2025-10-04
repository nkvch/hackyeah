# D. Specification of a standardised software development process using AI models

## Purpose of the document:

The aim of the document is to standardize the process of software development using AI models, ensuring:

- Efficiency (accelerating development),
- Repeatability (maintaining consistent practices),
- Quality (minimizing errors and technological debts),
- Security (conscious use of generated code).

## 1. Prerequisites

### 1.1. Environment

- List of supported IDEs (e.g. Cursor AI, IntelliJ AI Assistant, VSCode with Copilot etc.)
- Required plugins / extensions
- Versions of tools
- Configuration of LLM (e.g. LLM) OpenAI GPT-4o, Claude, Local LLM)

## 2. Good practices of prompting

### 2.1. Structure of prompt

- Context (e.g. language, framework, architecture)
- Task (to be performed)
- Expectations (output, format, style, tests)

### 2.2. Examples of prompts

- 'Write a function in Python to parse CSV, with unit tests in pytest'
- 'Propose folder structure for Spring Boot application with REST API layer and PostgreSQL integration'

### 2.3. Advanced Prompts

- Refactoring
- Generic code production
- Creating documentation from code
- Diagnosis of errors

## Stages of development with AI

### 3.1. Planning

- Generating functional specification based on prompt
- Automatic creation of backlog / user stories

### 3.2. Design

- Creating class/component schemas
- Architecture: microservices, monolith, DDD
- UML/diagrams (converted to code)

### 3.3. Coding

- Generating the project skeleton
- Extending functionality step by step
- Integration with existing code (contextual AI hints)

### 3.4. Testing

- Generating unit tests, integration tests
- Coverage by tests – Coverage analysis prompts

### 3.5. Documentation

- Generate README, diagrams, changelogs
- Creation of inline documentation (e.g. Javadoc, docstrings)

### 3.6. Refactoring and maintenance

- AI proposals for refactoring
- Search code smell and architectural errors
- Comparing versions and suggesting changes