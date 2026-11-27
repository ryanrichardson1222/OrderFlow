# OrderFlow

A full-stack Order Management System built to demonstrate software engineering, API development, relational database design, infrastructure integration, automated testing, and systematic troubleshooting.

OrderFlow is a hands-on engineering portfolio project designed to demonstrate how a modern application is structured, how its components communicate, how data moves through the system, and how failures can be diagnosed and resolved.

## Project Overview

OrderFlow allows users to manage orders through a React-based frontend backed by a C# ASP.NET Core Web API and SQL Server database.

The application follows a layered architecture that separates API responsibilities, application logic, and infrastructure concerns. Dapper provides database access while repository abstractions help maintain separation between application behavior and persistence implementation.

The project emphasizes not just building functional software, but understanding the engineering decisions behind it.

## Technology Stack

| Layer | Technology |
|---|---|
| Frontend | React 19, TypeScript |
| Backend | C#, .NET 10, ASP.NET Core Web API |
| Application Architecture | Layered Architecture, Repository Pattern, Dependency Injection |
| Data Access | Dapper |
| Database | Microsoft SQL Server 2022 Express |
| Database Operations | T-SQL, Stored Procedures, Relational Constraints |
| Testing | xUnit, API Tests, Application Tests, Infrastructure Tests |
| Frontend Tooling | Vite, ESLint |
| Version Control | Git, GitHub |

## Architecture

OrderFlow uses a layered architecture to separate responsibilities and reduce dependencies between application logic and infrastructure implementation.

```text
React + TypeScript
       |
       | HTTP / JSON
       v
ASP.NET Core Web API
       |
       v
Application Layer
(OrderService and application rules)
       |
       | Repository Interface
       v
Infrastructure Layer
(SqlOrderRepository + Dapper)
       |
       v
SQL Server
(Stored Procedures and Relational Schema)
```

### Component Responsibilities

**Frontend**
- Provides the Order Management dashboard.
- Displays order information retrieved from the API.
- Supports order search, filtering, and status-oriented presentation.

**API Layer**
- Exposes HTTP endpoints.
- Handles incoming requests and outgoing responses.
- Delegates application behavior to the application layer.

**Application Layer**
- Contains order-related application logic and validation.
- Uses repository abstractions rather than depending directly on database implementation details.

**Infrastructure Layer**
- Implements repository interfaces.
- Uses Dapper and Microsoft.Data.SqlClient for SQL Server communication.
- Handles database-specific operations and result mapping.

**Database Layer**
- Maintains normalized Users and Orders tables.
- Enforces relational constraints.
- Uses stored procedures for order operations.
- Implements soft-delete and restoration behavior.

## Current Capabilities

- Retrieve orders from SQL Server through the API.
- Create orders associated with existing users.
- Update order statuses.
- Soft-delete orders without permanently removing their records.
- Restore previously deleted orders.
- Exclude deleted orders from normal retrieval.
- Retrieve deleted orders through administrative database operations.
- Display order information through the React dashboard.
- Search and filter order information.
- Validate application requests and enforce database constraints.

All project data is synthetic.

## Database Design

The database contains two primary relational tables:

### dbo.Users

Stores user information and provides the parent relationship for orders.

### dbo.Orders

Stores order information and references users through `UserId`.

The schema includes primary keys, foreign keys, unique constraints, check constraints, and default constraints.

### Stored Procedures

The database includes seven stored procedures:

1. `dbo.usp_GetAllOrders`
2. `dbo.usp_GetOrderById`
3. `dbo.usp_CreateOrder`
4. `dbo.usp_UpdateOrderStatus`
5. `dbo.usp_DeleteOrder`
6. `dbo.usp_RestoreOrder`
7. `dbo.usp_GetAllOrdersIncludingDeleted`

These procedures support order retrieval, creation, status updates, soft deletion, restoration, and administrative retrieval.

### Soft-Delete Design

Orders are not physically removed when deleted.

Instead:
- `IsDeleted` identifies whether an order is deleted.
- `DeletedAt` records the deletion timestamp.
- Normal retrieval excludes deleted orders.
- Administrative retrieval retains access to deleted records.
- Restoration clears the deletion state and timestamp.

This provides an opportunity to demonstrate data lifecycle management and controlled recovery.

## Testing and Validation

The solution contains separate automated test projects:

- `OrderFlow.Api.Tests`
- `OrderFlow.Application.Tests`
- `OrderFlow.Infrastructure.Tests`

Testing covers API behavior, application validation and business logic, and infrastructure-level database operations.

The full solution test command is:

```bash
dotnet test OrderFlow.slnx
```

The latest recorded verification included:
- 17 passing API tests.
- 39 passing Application tests.
- Infrastructure tests added and passing.

Test results should be revalidated against the current working tree before relying on them as a release or demonstration baseline.

Additional database validation has included:
- Fresh database deployment.
- Schema and constraint verification.
- Stored-procedure execution.
- Invalid-operation testing.
- Order creation and retrieval.
- Soft-delete and restoration behavior.
- Verification that normal and administrative retrieval return the expected records.

## Development and Troubleshooting Philosophy

**Build it. Break it. Diagnose it. Fix it. Prove it's fixed.**

OrderFlow is intentionally treated as an engineering lab.

The objective is to understand the entire application lifecycle, including what happens when individual components fail.

Troubleshooting exercises include:
- SQL Server connectivity failures.
- Database permission issues.
- Incorrect or missing configuration.
- Stored-procedure errors.
- Invalid API requests.
- Repository and data-mapping behavior.
- Frontend-to-API communication failures.
- Deployment inconsistencies.

Each failure should be investigated systematically, documented, corrected, and followed by verification that demonstrates the fix was effective.

## Security Principles

Security is considered throughout development rather than added as an afterthought.

Current practices and design considerations include:
- Synthetic data only.
- Development secrets stored outside source control.
- Dedicated application SQL login rather than the SQL Server `sa` account.
- Appropriately scoped database permissions.
- Application-level request validation.
- SQL parameterization through Dapper and stored-procedure parameters.
- Separation of development and validation databases.

Authentication, authorization, and additional security hardening remain part of the project's continuing engineering review.

## Repository Structure

The solution is organized into separate backend projects for API, application, infrastructure, and their corresponding tests.

The database deployment scripts are maintained separately under:

```text
database/
└── scripts/
    ├── 01-create-schema.sql
    ├── 02-seed-data.sql
    ├── 03-create-view.sql
    ├── 04-verify-database.sql
    └── 05-create-stored-procedures.sql
```

## Project Status

The project has progressed from an in-memory API foundation to a SQL-backed full-stack application.

Completed milestones include:
- Layered .NET application architecture.
- Application and API validation.
- SQL Server schema and relational constraints.
- Stored-procedure development and validation.
- Dapper-backed SQL repository.
- Dependency injection and SQL connection configuration.
- Automated API, application, and infrastructure testing.
- React and TypeScript dashboard.
- Frontend/API integration.
- Live SQL-backed order retrieval and mutation validation.

Remaining engineering work focuses on comprehensive end-to-end verification, security review, deliberate failure and recovery exercises, documentation, and final architectural review.

## Project Objective

OrderFlow is more than an order management application.

It is an opportunity to demonstrate the ability to design, develop, integrate, test, troubleshoot, secure, and maintain a full-stack system.

The ultimate goal is to understand not only what was built, but why it was built that way, how it behaves under failure, and how to prove that corrective actions work.