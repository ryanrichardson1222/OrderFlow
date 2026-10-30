# OrderFlow — Project Checkpoint

**Checkpoint date:** October 30, 2026

## Project Goal

Build a full-stack Order Management System using React, C#/.NET Web API,
SQL Server, and Dapper. Use the project to demonstrate engineering,
database design, integration, and systematic troubleshooting.

Guiding principle: Build it. Break it. Diagnose it. Fix it. Prove it's fixed.

## Application Status

- Solution: OrderFlow.slnx
- Backend: .NET 10
- Architecture: API, Application, and Infrastructure projects
- Repository pattern implemented with asynchronous operations
- Current repository implementation: InMemoryOrderRepository
- Git branch: main
- Latest pushed commit: cb318bd
- Latest commit message: Refactor order operations to async repository pattern

### Test Status

Last verified:
- Build succeeded
- 15 tests passed
- 0 failed
- 0 skipped

## SQL Server Status

- SQL Server: 2022 Express
- Instance: SQLEXPRESS
- Database: FullStackLearning
- Database state: ONLINE
- Recovery model: SIMPLE

### Schema

dbo.Users:
- UserId
- FirstName
- LastName
- Email
- CreatedAt

dbo.Orders:
- OrderId
- UserId
- OrderDate
- Status
- Total

Constraints verified:
- PK_Users
- UQ_Users_Email
- DF_Users_CreatedAt
- PK_Orders
- FK_Orders_Users
- CK_Orders_Total
- CK_Orders_Status
- DF_Orders_OrderDate
- DF_Orders_Status

### Synthetic Test Data

- 3 users
- 4 orders
- Joined user/order query verified

### Constraint Failure Tests

The following invalid operations were rejected as expected:
- Duplicate user email
- Order referencing nonexistent UserId
- Negative order total

## Database Scripts

Located in database/scripts:

1. 01-create-schema.sql
2. 02-seed-data.sql
3. 03-create-view.sql
4. 04-verify-database.sql

These scripts have been saved locally but have not yet been replayed
against a fresh database to validate reproducibility.

A full SQL Server backup was created at
C:\OrderFlow-Backups\FullStackLearning.bak.
RESTORE VERIFYONLY WITH CHECKSUM completed successfully.
A full restore test has not yet been performed.

## Important Architecture Decision

The current application Order model uses CustomerName, while the SQL
schema uses normalized Users and Orders tables.

Before implementing Dapper integration, reconcile the domain model,
repository interface, API contracts, and database relationships.

Do not force the existing in-memory model onto the SQL schema.

## Next Steps

## Next Steps

1. Replay the SQL scripts against a fresh test database and verify reproducibility.
2. Confirm the view and all database objects using the verification script.
3. Add stored procedures for CRUD operations.
4. Configure SQL Server TCP/IP access securely from macOS.
5. Create a least-privilege application SQL login.
6. Add Dapper and Microsoft.Data.SqlClient to Infrastructure.
7. Refactor the domain model and repository contract for Users and Orders.
8. Implement the SQL-backed repository.
9. Add integration tests and controlled failure scenarios.
10. Perform a full database restore test.

## Security Notes

- Use synthetic data only.
- Never use the SQL Server sa account from the application.
- Never commit passwords, connection strings containing credentials,
  database backups, or production data.
- Store development secrets outside source control.
