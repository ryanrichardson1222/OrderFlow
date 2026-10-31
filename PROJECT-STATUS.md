# OrderFlow — Project Checkpoint

**Checkpoint date:** October 31, 2026

## Project Goal

Build a full-stack Order Management System using React, C#/.NET Web API, SQL Server, and Dapper.

Use the project to demonstrate full-stack engineering, relational database design, API integration, and systematic software troubleshooting.

**Guiding principle:** Build it. Break it. Diagnose it. Fix it. Prove it's fixed.

## Application Status

- Solution: `OrderFlow.slnx`
- Backend: .NET 10
- Architecture: API, Application, and Infrastructure projects.
- Repository pattern implemented with asynchronous operations.
- Current repository implementation: `InMemoryOrderRepository`.
- Git branch: `main`
- Latest pushed commit: `cb318bd`
- Latest pushed commit message: `Refactor order operations to async repository pattern`
- Latest local commit: `802db67` — Add SQL Server schema and database checkpoint.
- Local branch is one commit ahead of `origin/main`.
- Database deployment scripts now support a configurable SQLCMD `DatabaseName` variable.

### Test Status

Last verified application test results:

- Build succeeded.
- 15 tests passed.
- 0 failed.
- 0 skipped.

## SQL Server Environment

- SQL Server: 2022 Express
- Instance: `SQLEXPRESS`
- Windows VM hostname: `TheDevenant`
- PowerShell: 7.6.9
- SQLCMD connection method: Shared Memory (`lpc:`)
- Authentication: Windows Integrated Authentication.

### Development Database

- Database: `FullStackLearning`
- State: ONLINE
- Recovery model: SIMPLE
- Existing schema and synthetic data verified.
- Full database backup created at `C:\OrderFlow-Backups\FullStackLearning.bak`.
- `RESTORE VERIFYONLY WITH CHECKSUM` completed successfully.
- A full restore test has not yet been performed.

### Fresh Validation Database

- Database: `OrderFlowFreshTest`
- State: ONLINE
- Recovery model: SIMPLE
- Fresh schema deployment completed successfully.
- All four database scripts executed and validated against this database.

**Important:** `FullStackLearning` was retained as the original development database. Fresh-deployment validation was performed against the separate `OrderFlowFreshTest` database.

## Database Schema

### dbo.Users

- `UserId` — INT IDENTITY, primary key
- `FirstName` — NVARCHAR(50)
- `LastName` — NVARCHAR(50)
- `Email` — NVARCHAR(255)
- `CreatedAt` — DATETIME2(0)

### dbo.Orders

- `OrderId` — INT IDENTITY, primary key
- `UserId` — INT, foreign key
- `OrderDate` — DATETIME2(0)
- `Status` — NVARCHAR(30)
- `Total` — DECIMAL(18,2)

### Constraints

All nine constraints were verified:

- `PK_Users`
- `UQ_Users_Email`
- `DF_Users_CreatedAt`
- `PK_Orders`
- `FK_Orders_Users`
- `CK_Orders_Total`
- `CK_Orders_Status`
- `DF_Orders_OrderDate`
- `DF_Orders_Status`

## Database Validation Results

### Synthetic Test Data

Successfully deployed and verified:

- 3 synthetic users.
- 4 synthetic orders.
- Correct user/order relationships.
- User-orders JOIN query returned all four expected records.
- `dbo.vw_UserOrders` created and verified.

### Deployment and Idempotency

- Fresh database creation verified against `OrderFlowFreshTest`.
- Both tables created successfully.
- All nine constraints created successfully.
- Seed script executed successfully.
- Seed script rerun without duplicating records.
- Final record counts remained unchanged: 3 users and 4 orders.

### Constraint Failure Testing

All three controlled invalid operations were rejected as expected:

| Test | Constraint | SQL Server error |
|---|---|---|
| Duplicate user email | `UQ_Users_Email` | 2627 |
| Order referencing nonexistent UserId | `FK_Orders_Users` | 547 |
| Negative order total | `CK_Orders_Total` | 547 |

Final verification confirmed that all rejected inserts left the original records unchanged.

### Troubleshooting Incident: Database Context

During fresh deployment testing, the database was successfully created, but the expected tables were missing.

Investigation identified a missing `USE [$(DatabaseName)]` statement after the database-creation batch.

The script was corrected, rerun, and successfully validated.

**Lesson:** Successful database creation does not guarantee subsequent statements execute in the intended database context.

## Database Scripts

Located in `database/scripts/`:

1. `01-create-schema.sql`
2. `02-seed-data.sql`
3. `03-create-view.sql`
4. `04-verify-database.sql`

All four scripts support a configurable SQLCMD `DatabaseName` variable.

Example execution:

```powershell
sqlcmd -b -S lpc:.\SQLEXPRESS -E -v DatabaseName="OrderFlowFreshTest" -i "C:\OrderFlow-SQL-Validation\scripts\01-create-schema.sql"
```

**Validation status:** All four scripts have been successfully executed against a fresh database, and their results independently verified.

## Important Architecture Decision

The current application `Order` model uses `CustomerName`, while the SQL schema uses normalized `Users` and `Orders` tables.

Before implementing Dapper integration, reconcile:

- Domain models
- Repository interfaces
- API request and response contracts
- Database entities and relationships
- Data mapping responsibilities

Do not force the existing in-memory model onto the SQL schema.

## Next Steps

1. Add stored procedures for CRUD operations.
2. Configure secure SQL Server connectivity from macOS.
3. Create a least-privilege application SQL login.
4. Reconcile the domain model and repository contract with the normalized schema.
5. Add Dapper and `Microsoft.Data.SqlClient` to Infrastructure.
6. Implement the SQL-backed repository.
7. Add integration tests and controlled failure scenarios.
8. Perform a full database restore test.

## Security Notes

- Use synthetic data only.
- Never use the SQL Server `sa` account from the application.
- Never commit passwords, credentials, connection strings containing secrets, database backups, or production data.
- Store development secrets outside source control.
- Use appropriately scoped database permissions for application access.
- Keep validation and development databases separate when testing deployment or recovery procedures.

## Current Project Position

**Application:** Functional API and application-layer foundation with automated tests.

**Database:** Fresh deployment, schema, seed data, view, idempotency, and constraint enforcement successfully validated.

**Next engineering milestone:** Reconcile the application architecture with the normalized SQL schema and begin Dapper integration.

**Project standard:** We must be able to explain not only what we built, but why it works, how it fails, and how we proved our fixes were effective.
