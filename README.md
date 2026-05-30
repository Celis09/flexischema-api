# Contacts API (CQRS & Clean Architecture)

A robust, production-ready ASP.NET Core 8 Web API for managing contacts and users. Built to demonstrate advanced software engineering patterns, this project utilizes **CQRS (Command Query Responsibility Segregation)**, **MediatR**, and a **Folder-Based Clean Architecture** to ensure high scalability, testability, and separation of concerns.

## 🚀 Key Features & Architecture

* **CQRS Pattern via MediatR**: Strict separation of read operations (Queries) and write operations (Commands). Controllers are kept incredibly thin.
* **MediatR Pipeline Behaviors**: Cross-cutting concerns are intercepted automatically in the request pipeline:
  * **ValidationBehavior**: Executes FluentValidation rules concurrently.
  * **AuditLoggingBehavior**: Automatically records mutations to the database using an isolated DbContext.
  * **RoleAuthorizationBehavior**: Validates user roles via custom attributes before a command is executed.
  * **CorrelationIdBehavior**: Injects correlation IDs into logs for end-to-end request tracing.
  * **ExceptionHandlingBehavior**: Centralized error logging.
* **Flexible / Dynamic Data Schema**: Contacts support a custom field architecture (similar to an Entity-Attribute-Value pattern). Administrators can define `ExtraFieldDefinitions` (like Dropdowns, Text, Dates) which are dynamically attached to Contacts via `ContactExtraField`, allowing the system's data model to evolve without database schema migrations.
* **Vertical Slice Architecture**: Application logic is grouped by feature (e.g., Contacts, Users, Admins) rather than technical layer, vastly improving developer velocity.
* **Robust Testing**: Comprehensive test suite containing **187 passing Unit and Integration Tests** utilizing `xUnit`, `Moq`, and `FluentAssertions`.
* **Structured Logging**: Integrated `Serilog` with Elasticsearch & Console sinks, featuring Correlation IDs to trace requests seamlessly.

## 🛠️ Tech Stack

* **Framework**: .NET 8 / C# 12
* **Architecture**: CQRS, Vertical Slice, MediatR
* **ORM**: Entity Framework Core (SQL Server)
* **Validation**: FluentValidation
* **Logging**: Serilog (Structured Logging)
* **Testing**: xUnit, Moq, FluentAssertions
* **Authentication**: JWT (JSON Web Tokens) Bearer Auth

## 📂 Folder Structure (Vertical Slices)

The project leverages a folder-based Clean Architecture, separating the Domain, Data/Infrastructure, and Application layers inside a unified project structure:

```text
ContactsAPI/
├── API/             # Controllers, keeping HTTP concerns isolated
├── Application/     # Business logic separated by feature slices (CQRS)
│   ├── Contacts/    # Everything for Contacts (Commands, Queries, Dtos)
│   ├── Users/       # Everything for Users
│   └── Behaviors/   # MediatR Pipeline interceptors
├── Data/            # EF Core DbContext and Migrations
├── Entities/        # Core Domain Models
└── Infrastructure/  # External integrations (e.g., HTTP Handlers)
```

## ⚙️ Getting Started

### Prerequisites
* [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
* SQL Server (or LocalDB)

### Setup & Run
1. Clone the repository.
2. Update the `DefaultConnection` string in `appsettings.json` to point to your SQL Server instance.
3. Review the `Jwt` settings in `appsettings.json`. A dummy key is provided for local development, but this should be injected via environment variables in production.
4. Open a terminal in the `ContactsAPI` directory and apply the database migrations:
   ```bash
   dotnet ef database update
   ```
5. Run the application:
   ```bash
   dotnet run
   ```
6. Navigate to `https://localhost:<port>/swagger` to explore the API endpoints!

### 🔑 Authentication (Initial Login)
The database migration automatically seeds a default Admin and Editor user so you can generate your first JWT token:

* **Admin Username**: `admin`
* **Editor Username**: `editor`
* **Password**: `Password@123` *(Same for both. Be sure to change this or delete the seed in a real production environment!)*

Use the `POST /api/v1/auth/login` endpoint to get your token, and paste it into Swagger's "Authorize" button using the format `Bearer <your_token>`.

## 🧪 Testing

The project includes an extensive test suite that isolates handlers and validators. To run the tests:

```bash
dotnet test
```

Currently passing **187 out of 187 tests**, ensuring high reliability across domain rules and authorization checks.
