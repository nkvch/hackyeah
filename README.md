# UKNF Communication Platform

This repository contains the full-stack UKNF Communication Platform, including a .NET 8.0 backend (ASP.NET Core Web API) and an Angular 20 frontend. The application is containerized using Docker and Docker Compose for easy setup and deployment.

## Prerequisites

Before you begin, ensure you have the following installed on your system:

*   **Docker Desktop:** Essential for running the containerized application. Includes Docker Engine and Docker Compose.
    *   [Download Docker Desktop](https://www.docker.com/products/docker-desktop/)

## Getting Started

Follow these steps to get the application up and running:

1.  **Clone the repository:**
    ```bash
    git clone <repository-url>
    cd uknf-communication-platform
    ```

2.  **Build and Run the Containers:**
    This command will build the Docker images for both frontend and backend, create the necessary services (PostgreSQL, RabbitMQ, MailDev), and start them in detached mode.

    ```bash
    docker-compose up -d --build
    ```
    _This process might take a few minutes on the first run as it downloads images and builds the application._

3.  **Seed Internal Users (Optional but Recommended):**
    To populate the database with a set of internal test users, run the seeding script:

    ```bash
    ./scripts/seed-internal-users.sh
    ```
    _This script will create 5 internal users with a common password `UknfAdmin123!` and save their credentials to `test-data/internal-users-credentials.txt`._

## Accessing Services

Once all containers are running, you can access the application and its services at the following URLs:

*   **Frontend (Angular SPA):** [http://localhost:4200](http://localhost:4200)
*   **Backend API (ASP.NET Core):** [http://localhost:8080](http://localhost:8080)
*   **Swagger API Documentation:** [http://localhost:8080/swagger](http://localhost:8080/swagger)
*   **MailDev (Email Testing Interface):** [http://localhost:1080](http://localhost:1080)
*   **RabbitMQ Management Interface:** [http://localhost:15672](http://localhost:15672) (Default credentials: `guest`/`guest`)

## Test User Credentials (Internal)

If you ran the `seed-internal-users.sh` script, you can log in with these credentials:

*   **Common Password:** `UknfAdmin123!`
*   **User Emails:**
    *   `jan.kowalski@uknf.gov.pl`
    *   `anna.nowak@uknf.gov.pl`
    *   `piotr.wisniewski@uknf.gov.pl`
    *   `maria.wojcik@uknf.gov.pl`
    *   `tomasz.kaminski@uknf.gov.pl`

These details are also available in `test-data/internal-users-credentials.txt`.

## Stopping the Application

To stop all running services and remove their containers, volumes, and networks:

```bash
docker-compose down -v
```

To stop only the services (keeping volumes):

```bash
docker-compose down
```

## Troubleshooting

*   **Port Conflicts:** If you encounter errors about ports already being in use, ensure no other applications are running on `5432` (PostgreSQL), `8080` (Backend), `4200` (Frontend), `1025` (MailDev SMTP), `1080` (MailDev Web), or `15672` (RabbitMQ Management).
    *   You can check port usage with `lsof -i :<PORT>` (macOS/Linux) or `netstat -ano | findstr :<PORT>` (Windows).
*   **Database Migration Issues:** If you're starting fresh and encounter database errors, try a full clean-up:
    ```bash
    docker-compose down -v
    # Then rebuild and restart
    docker-compose up -d --build
    ```
    _This will remove all Docker data volumes, including the database, allowing for a fresh migration._
