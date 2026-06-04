# FlexiSchema API (CQRS & Clean Architecture)

A robust, production-ready ASP.NET Core 8 Web API for managing contacts and users. Built to demonstrate advanced software engineering patterns, this project utilizes **CQRS (Command Query Responsibility Segregation)**, **MediatR**, and a **Folder-Based Clean Architecture** to ensure high scalability, testability, and separation of concerns.

## 🌐 Live Demo

| Layer    | URL                                                              |
|----------|------------------------------------------------------------------|
| Backend  | `http://flexischemacrm.runasp.net`                               |
| Frontend | `https://flexischema-crm-tawny.vercel.app`                 |
| Swagger  | `http://flexischemacrm.runasp.net/swagger`                       |
| Health   | `http://flexischemacrm.runasp.net/health`                        |

> **Note:** The backend is hosted on a free-tier provider without HTTPS. The Vercel frontend uses a reverse-proxy (`vercel.json` rewrites) to securely bridge the connection.

## 🚀 Key Features & Architecture

* **CQRS Pattern via MediatR**: Strict separation of read operations (Queries) and write operations (Commands). Controllers are kept incredibly thin.
* **MediatR Pipeline Behaviors**: Cross-cutting concerns are intercepted automatically in the request pipeline:
  * **ValidationBehavior**: Executes FluentValidation rules concurrently.
  * **AuditLoggingBehavior**: Automatically records mutations to the database using an isolated DbContext.
  * **RoleAuthorizationBehavior**: Validates user roles via custom attributes before a command is executed.
  * **CorrelationIdBehavior**: Injects correlation IDs into logs for end-to-end request tracing.
  * **MetricsBehavior**: Tracks handler execution time for performance monitoring.
  * **LoggingBehavior**: Structured request/response logging with Serilog.
* **AI-Powered Features (Groq + LLaMA 3.1 8B)**:
  * **Contact Insights**: AI-generated profile summaries and relationship tags (Lead / Active / At Risk) per contact. Results are cached in the database to minimize token usage; force-regenerate on demand via `GET /api/v1/contacts/{id}/insights?forceRegenerate=true`.
  * **AI Search Bar**: Natural language search for contacts (e.g. "contacts added last week" or "manager is Marco"). The AI parses the prompt into a structured filter with status, search terms, date ranges, and extra field conditions. Role-based guardrails prevent unauthorized data access. Gracefully falls back to standard search if the AI call fails.
  * **Smart CSV Header Mapper**: AI-powered auto-mapping of messy CSV headers to system fields (standard + dynamic extra field definitions) during import. Accepts sample data rows for context-aware mapping.
* **Flexible / Dynamic Data Schema**: Contacts support a custom field architecture (similar to an Entity-Attribute-Value pattern). Administrators can define `ExtraFieldDefinitions` (like Dropdowns, Text, Dates, Email, Phone, URL, Number) which are dynamically attached to Contacts via `ContactExtraField`, allowing the system's data model to evolve without database schema migrations.
* **Smart Caching with Auto-Invalidation**: Extra field definitions are cached in-memory for performance, with automatic cache invalidation whenever an admin modifies, adds, or toggles any field definition.
* **Vertical Slice Architecture**: Application logic is grouped by feature (e.g., Contacts, Users, Admins) rather than technical layer, vastly improving developer velocity.
* **Robust Testing**: Comprehensive test suite containing **201 passing Unit and Integration Tests** utilizing `xUnit`, `Moq`, and `FluentAssertions`.
* **Structured Logging**: Integrated `Serilog` with Elasticsearch & Console sinks, featuring Correlation IDs to trace requests seamlessly.
* **Production-Ready Security**: 
  * Locked-down CORS policy with explicit origin whitelisting.
  * **HTTP-Only Cookies** for secure, XSS-immune JWT and Refresh Token storage (with a fallback to `Authorization` header for backward compatibility with Swagger/Postman).
  * Strict role-based authorization restricting `Inactive` and `Archived` contact access to Admins only.
* **Smart CSV Import Engine**: Automatically parses and normalizes dozens of inconsistent date formats into strict ISO standards during data ingestion.

## 🛠️ Tech Stack

* **Framework**: .NET 8 / C# 12
* **Architecture**: CQRS, Vertical Slice, MediatR
* **ORM**: Entity Framework Core (SQL Server)
* **AI**: Microsoft.Extensions.AI + Groq API (LLaMA 3.1 8B)
* **Validation**: FluentValidation
* **Logging**: Serilog (Elasticsearch + Console sinks)
* **Testing**: xUnit, Moq, FluentAssertions
* **Authentication**: JWT (JSON Web Tokens) Bearer Auth
* **Hosting**: RunASP (Backend) + Vercel (Frontend)

## 📂 Folder Structure (Vertical Slices)

The project leverages a folder-based Clean Architecture, separating the Domain, Data/Infrastructure, and Application layers inside a unified project structure:

```text
ContactsAPI/
├── API/             # Controllers, keeping HTTP concerns isolated
│   └── Controllers/
│       ├── Auth/        # Login, Refresh, Logout
│       ├── Admin/       # User & ExtraField management (Admin-only)
│       └── Contacts/    # CRUD + Search + Export + AI features
├── Application/     # Business logic separated by feature slices (CQRS)
│   ├── Contacts/    # Commands, Queries, Dtos, Validators
│   │   ├── Commands/
│   │   │   └── MapCsvHeaders/       # AI CSV header mapper
│   │   └── Queries/
│   │       ├── GetContactInsights/   # AI contact profiler
│   │       └── SearchContactsByAi/   # AI natural language search
│   ├── Users/       # Commands, Queries, Dtos
│   ├── Admins/      # Admin-specific commands
│   └── Behaviors/   # MediatR Pipeline interceptors
├── Data/            # EF Core DbContext and Migrations
├── Entities/        # Core Domain Models (includes ContactInsight cache)
├── Middleware/      # Exception Handling & Correlation ID middleware
├── Services/        # Auth, Config, Export services
├── Infrastructure/  # External integrations (e.g., HTTP Handlers)
└── Test/            # Unit & Integration tests organized by feature
```

## ⚙️ Getting Started

### Prerequisites
* [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
* SQL Server (or LocalDB)
* Groq API Key (for AI features)

### Setup & Run
1. Clone the repository.
2. Copy `appsettings.example.json` to `appsettings.Development.json` and fill in your secrets:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=ContactsDB;Trusted_Connection=True;TrustServerCertificate=True;"
     },
     "Jwt": {
       "Key": "YourSuperSecretKeyHere_AtLeast32Characters!"
     },
     "AI_Providers": {
       "Groq_ApiKey": "gsk_your_groq_api_key_here"
     },
     "Elasticsearch": {
       "Uri": "http://localhost:9200"
     }
   }
   ```
3. Open a terminal in the `ContactsAPI` directory and apply the database migrations:
   ```bash
   dotnet ef database update
   ```
4. Run the application:
   ```bash
   dotnet run --launch-profile https
   ```
5. Navigate to `https://localhost:<port>/swagger` to explore the API endpoints!

> **For local development with the frontend:** The API uses HTTPS (`https://localhost:7148`). The frontend Vite dev server proxies `/api/*` requests to this URL, so no cookie issues arise.

### Environment Variables (Production)
For production deployments, configure these via your hosting provider's environment variable settings:

| Variable                            | Description                          |
|-------------------------------------|--------------------------------------|
| `ConnectionStrings__DefaultConnection` | SQL Server connection string       |
| `Jwt__Key`                          | JWT signing secret (min 32 chars)    |
| `Jwt__Issuer`                       | JWT issuer (default: `ContactsAPI`)  |
| `Jwt__Audience`                     | JWT audience (default: `ContactsAPIUsers`) |
| `AI_Providers__Groq_ApiKey`         | Groq API key for AI features         |
| `Elasticsearch__Uri`                | Elasticsearch endpoint URL           |
| `AllowedOrigins__0`                 | First allowed CORS origin            |
| `AllowedOrigins__1`                 | Second allowed CORS origin           |

## 🔑 Authentication (Initial Login)
The database migration automatically seeds a default Admin and Editor user so you can generate your first JWT token:

* **Admin Username**: `admin`
* **Editor Username**: `editor`
* **Password**: `Password@123` *(Same for both. Be sure to change this in a real production environment!)*

Use the `POST /api/v1/auth/login` endpoint to get your token (returned in an HTTP-Only cookie and response body). Subsequent API calls automatically include the cookie via `credentials: "include"`.

### API Endpoints Overview

| Method | Endpoint                              | Auth       | Description                    |
|--------|---------------------------------------|------------|--------------------------------|
| POST   | `/api/v1/auth/login`                  | Public     | Get JWT + refresh token        |
| POST   | `/api/v1/auth/refresh`                | Public     | Refresh expired JWT            |
| POST   | `/api/v1/auth/logout`                 | Public     | Revoke refresh token           |
| GET    | `/api/v1/contacts`                    | Bearer     | List contacts (paginated)      |
| POST   | `/api/v1/contacts`                    | Bearer     | Create a new contact           |
| GET    | `/api/v1/contacts/{id}`               | Bearer     | Get contact by ID              |
| PUT    | `/api/v1/contacts/{id}`               | Bearer     | Update a contact               |
| GET    | `/api/v1/contacts/{id}/insights`      | Bearer     | AI contact summary & tag       |
| GET    | `/api/v1/contacts/ai-search`          | Bearer     | AI natural language search     |
| POST   | `/api/v1/contacts/import/map-headers` | Bearer     | AI CSV header auto-mapper      |
| POST   | `/api/v1/contacts/import/preview`     | Admin      | Preview CSV import             |
| POST   | `/api/v1/contacts/import`             | Admin      | Execute CSV import             |
| GET    | `/api/v1/contacts/export`             | Bearer     | Export contacts to CSV          |
| GET    | `/api/v1/admin/users`                 | Admin      | List all users                 |
| POST   | `/api/v1/admin/users`                 | Admin      | Create a new user              |
| PUT    | `/api/v1/admin/users/{id}`            | Admin      | Update a user                  |
| GET    | `/api/v1/admin/extra-fields`          | Admin      | List extra field definitions   |
| POST   | `/api/v1/admin/extra-fields`          | Admin      | Create extra field definition  |
| GET    | `/health`                             | Public     | Health check                   |
| GET    | `/health/details`                     | Public     | Detailed health check (JSON)   |

## 🧠 AI Features

### Contact Insights (`GET /api/v1/contacts/{id}/insights`)
Analyzes a contact's profile (name, email, status, extra fields) using Groq's LLaMA 3.1 8B model and returns:
- **Summary**: A 2-sentence AI-generated profile summary
- **Tag**: A relationship tag — **Lead**, **Active**, or **At Risk**

Results are **cached in the database** to minimize API token usage. Pass `?forceRegenerate=true` to re-run the AI analysis.

### AI Search (`GET /api/v1/contacts/ai-search?prompt=...`)
Parses natural language queries into structured database filters. Examples:
- `"contacts added last week"` → filters by `AddedAfter` date
- `"inactive contacts"` → filters by `Status`
- `"manager is Marco"` → filters by extra field value

Includes role-based guardrails: non-admin users are forced to view only `Active` contacts regardless of the AI's output.

### Smart CSV Mapper (`POST /api/v1/contacts/import/map-headers`)
Accepts CSV headers and sample data rows, then uses AI to map messy headers to system fields (standard + all active extra field definitions). Returns a dictionary of `{ "Messy CSV Header": "System Field Name" }`.

## 🧪 Testing

The project includes an extensive test suite that isolates handlers and validators. To run the tests:

```bash
dotnet test
```

Currently passing **201 out of 201 tests**, ensuring high reliability across domain rules, authorization checks, AI features, and admin operations.

## 📝 License

This project is intended as a portfolio demonstration piece.
