# OrderFlow — Project Checkpoint

**Checkpoint date:** November 5, 2026

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
- Application and API currently use in-memory persistence; SQL Server is not yet integrated with the .NET application.
- Order creation now references users through `UserId` instead of accepting a customer name.
- Application validation checks positive user IDs, nonnegative order totals, and user existence.
- Git branch: `main`
- Latest pushed commit: `dd42c6e` — Validate configurable SQL Server deployment scripts.
- Local branch is up to date with `origin/main`.
- Application changes committed as `6fba866` (`Implement user-based order validation`). Five database/documentation files remain staged for the second commit.
- Database deployment scripts support a configurable SQLCMD `DatabaseName` variable.

### Test Status

Last verified application test results, November 5, 2026:

- Build succeeded.
- 14 tests passed.
- 0 failed.
- 0 skipped.

Coverage includes application-service validation, repository behavior, API responses, invalid user IDs, nonexistent users, negative totals, and successful order creation.

### Current Uncommitted Changes

**Backend:**
- Updated API tests.
- Updated application tests and fake repository.
- Updated `CreateOrderRequest` to use `UserId`.
- Expanded `IOrderRepository` with `UserExistsAsync`.
- Updated the `Order` model to include `UserId`.
- Updated `OrderService` validation and creation logic.
- Updated `InMemoryOrderRepository` with separate user and order collections.

**Database:**
- Added soft-delete columns to the schema creation script.
- Updated the user-orders view to exclude deleted orders.
- Expanded database verification checks.
- Added `05-create-stored-procedures.sql` containing seven stored procedures.

These changes have been reviewed. Whitespace validation with `git diff --check` is clean. Changes have not yet been staged, committed, or pushed.

## SQL Server Environment

- SQL Server: 2022 Express
- Instance: `SQLEXPRESS`
- Windows VM hostname: `TheDevenant`
- PowerShell: 7.6.9
- SQLCMD connection method: Shared Memory (`lpc:`)
- Authentication: Windows Integrated Authentication.
- SQLCMD executable uses ODBC Driver 17.

### Development Database

- Database: `FullStackLearning`
- State: ONLINE
- Recovery model: SIMPLE
- Existing schema and synthetic data verified.
- Full database backup created at `C:\OrderFlow-Backups\FullStackLearning.bak`.
- `RESTORE VERIFYONLY WITH CHECKSUM` completed successfully.
- A full restore test has not yet been performed.

This remains the original development database and was used for stored-procedure testing.

### Historical Fresh Validation Database

- Database: `OrderFlowFreshTest`
- Previously used for fresh schema deployment and validation.
- Retained as a historical validation environment.

### Deployment Validation Databases

**V1: `OrderFlowDeploymentTest`**
- Fresh database creation succeeded.
- Initial deployment exposed a mismatch between the Windows schema script and the updated soft-delete schema.
- The view failed because `IsDeleted` did not exist.
- The issue was traced to a stale script copy.
- The corrected script was transferred and inspected.
- V1 was retained for troubleshooting history.

**V2: `OrderFlowDeploymentTestV2`**
- Fresh database creation succeeded.
- Corrected schema deployed successfully.
- Seed data deployed successfully.
- View created and verified.
- Seven stored procedures deployed.
- Verification script completed successfully.
- Create, read, and soft-delete behavior smoke-tested.
- Normal retrieval excludes deleted orders.
- Administrative retrieval includes deleted orders and deletion metadata.

V2 is the latest successful fresh-deployment validation environment.

## Database Schema

### dbo.Users

- `UserId` — INT IDENTITY, primary key
- `FirstName` — NVARCHAR(50)
- `LastName` — NVARCHAR(50)
- `Email` — NVARCHAR(255)
- `CreatedAt` — DATETIME2(0), defaults to UTC system time

### dbo.Orders

- `OrderId` — INT IDENTITY, primary key
- `UserId` — INT, foreign key
- `OrderDate` — DATETIME2(0), defaults to UTC system time
- `Status` — NVARCHAR(30), defaults to `Pending`
- `Total` — DECIMAL(18,2)
- `IsDeleted` — BIT, defaults to 0
- `DeletedAt` — DATETIME2(0), nullable

### Constraints

Ten named constraints are present in the current schema:

**Users**
- `PK_Users`
- `UQ_Users_Email`
- `DF_Users_CreatedAt`

**Orders**
- `PK_Orders`
- `FK_Orders_Users`
- `CK_Orders_Total`
- `CK_Orders_Status`
- `DF_Orders_OrderDate`
- `DF_Orders_Status`
- `DF_Orders_IsDeleted`

## Database Validation Results

### Synthetic Test Data

Successfully deployed and verified:

**Users**
1. Ryan Richardson — `ryan@example.test`
2. Alex Morgan — `alex@example.test`
3. Jordan Blake — `jordan@example.test`

**Orders**
1. User 1 — Processing — 249.99
2. User 1 — Pending — 89.95
3. User 2 — Shipped — 1249.50
4. User 3 — Pending — 499.00

- Three synthetic users.
- Four synthetic orders.
- Correct user/order relationships.
- User-orders JOIN query returned expected records.
- `dbo.vw_UserOrders` created and verified.

### Deployment and Idempotency

- Configurable SQLCMD database variable implemented.
- Fresh database creation verified.
- Both tables created successfully.
- Ten named constraints created successfully.
- Seed script executed successfully.
- Seed script rerun without duplicating the existing sample records.
- Final baseline record counts: three users and four orders.

**Deployment caveat:** `01-create-schema.sql` creates missing tables but is not an incremental migration script. It does not add new columns to an existing table that was created using an older schema.

### Constraint Failure Testing

All three controlled invalid operations were rejected as expected:

| Test | Constraint | SQL Server error |
|---|---|---|
| Duplicate user email | `UQ_Users_Email` | 2627 |
| Order referencing nonexistent UserId | `FK_Orders_Users` | 547 |
| Negative order total | `CK_Orders_Total` | 547 |

Rejected inserts left the original records unchanged.

### Soft-Delete Validation

- `IsDeleted` defaults to 0.
- `DeletedAt` records deletion time in UTC.
- Normal order retrieval excludes deleted records.
- Administrative retrieval includes deleted records.
- Restore resets `IsDeleted` to 0 and clears `DeletedAt`.
- `dbo.vw_UserOrders` excludes deleted orders.
- Fresh V2 deployment confirmed the expected columns and behavior.

## Stored Procedures

Located in `database/scripts/05-create-stored-procedures.sql`.

Seven procedures use `CREATE OR ALTER` and are designed for repeatable deployment.

1. `dbo.usp_GetAllOrders`
   - Retrieves all active orders with associated user information.

2. `dbo.usp_GetOrderById`
   - Retrieves one active order by ID.

3. `dbo.usp_CreateOrder`
   - Validates user existence and order total.
   - Inserts an order and returns the generated order record.

4. `dbo.usp_UpdateOrderStatus`
   - Validates the requested status.
   - Updates active orders only.
   - Distinguishes nonexistent orders from deleted orders.

5. `dbo.usp_DeleteOrder`
   - Performs a soft delete.
   - Sets `IsDeleted` and records `DeletedAt`.

6. `dbo.usp_RestoreOrder`
   - Restores a soft-deleted order.
   - Clears the deletion timestamp.

7. `dbo.usp_GetAllOrdersIncludingDeleted`
   - Administrative retrieval of all orders, including deletion metadata.

### Stored-Procedure Error Testing

Nine custom error scenarios were tested successfully:

| Error | Scenario |
|---|---|
| 50001 | User does not exist |
| 50002 | Negative order total (procedure also rejects NULL) |
| 50003 | Order does not exist during status update |
| 50004 | Invalid order status |
| 50005 | Order does not exist during deletion |
| 50006 | Order is already deleted |
| 50007 | Order does not exist during restoration |
| 50008 | Order is not deleted |
| 50009 | Attempt to update a deleted order |

### Stored-Procedure Smoke Testing

Tested against `FullStackLearning` and fresh deployment V2.

- Created an order for an existing user.
- Retrieved the created order.
- Soft-deleted the order.
- Verified that normal retrieval excluded it.
- Verified that administrative retrieval included it.
- Restored an order and confirmed its active state.
- Confirmed that deleted-order metadata was recorded correctly.

The fresh V2 database was also tested with a new order (OrderId 5), which was successfully created, deleted, excluded from normal queries, and retained in administrative results. Restoration behavior was separately tested against FullStackLearning.

**Important:** These procedures have been manually validated in SQL Server. They have not yet been integrated into the .NET application through Dapper.

## Database Scripts

Located in `database/scripts/`:

1. `01-create-schema.sql`
2. `02-seed-data.sql`
3. `03-create-view.sql`
4. `04-verify-database.sql`
5. `05-create-stored-procedures.sql`

All five scripts support a configurable SQLCMD `DatabaseName` variable.

Example execution:

```powershell
sqlcmd -b -S lpc:.\SQLEXPRESS -E -v DatabaseName="OrderFlowDeploymentTestV2" -i "C:\OrderFlow-SQL-Validation\scripts\01-create-schema.sql"
```

**Validation status:** All five scripts have been deployed and validated in the SQL Server environment. The verification script confirms database state, tables, constraints, row counts, soft-delete columns, view existence, and stored-procedure existence.

## Architecture and Integration Decisions

### User and Order Relationship

The application previously accepted a customer name directly when creating an order.

The current request model uses:

```csharp
public record CreateOrderRequest(
    int UserId,
    decimal Total);
```

The application validates the user ID and checks that the referenced user exists before creating an order.

The in-memory repository now maintains separate user and order collections, reflecting the normalized SQL schema.

### SQL-to-Application Mapping

The SQL procedures return database-oriented column names:

- `OrderId`
- `UserId`
- `FirstName`
- `LastName`
- `OrderDate`
- `Status`
- `Total`

The current application model uses:

- `Id`
- `UserId`
- `CustomerName`
- `Status`
- `Total`

Before Dapper integration, define the mapping between these contracts. A persistence DTO is one option; SQL aliases are another.

Consider whether `OrderDate` should become part of the application order model.

Do not force database-specific naming or persistence details into the application layer without considering the architectural tradeoff.

## Next Steps

### Immediate Engineering Milestones

1. Finalize and commit the database scripts and project documentation.
2. Update and verify this project checkpoint.
3. Stage and commit the reviewed application and SQL changes after explicit approval.
4. Add Dapper and `Microsoft.Data.SqlClient` to Infrastructure.
5. Define the SQL result DTO and mapping strategy.
6. Configure secure SQL Server connectivity.
7. Create a least-privilege application SQL login.
8. Implement the SQL-backed repository using the stored procedures.
9. Register the SQL repository through dependency injection.
10. Add integration tests against SQL Server.

### Additional Validation and Engineering Work

- Test API-to-SQL end-to-end order creation and retrieval.
- Test stored-procedure error handling through the .NET application.
- Add structured logging and consistent API error responses.
- Perform a full database restore test using the existing backup.
- Add CI automation with GitHub Actions.
- Build the React frontend.
- Document architecture, deployment, and local development setup.
- Evaluate authentication and authorization requirements.

## Security Notes

- Use synthetic data only.
- Never use the SQL Server `sa` account from the application.
- Never commit passwords, credentials, connection strings containing secrets, database backups, or production data.
- Store development secrets outside source control.
- Use appropriately scoped database permissions for application access.
- Keep validation and development databases separate when testing deployment or recovery procedures.

## Current Project Position

**Application:** Functional .NET API and application-layer foundation, using asynchronous repository operations and in-memory persistence. Fourteen automated tests currently pass.

**Database:** Normalized Users and Orders schema, ten named constraints, synthetic data, view, soft-delete lifecycle, and seven stored procedures successfully deployed and validated.

**Deployment:** Fresh V2 deployment completed and verified. V1 deployment failure was diagnosed and retained as a troubleshooting example.

**Integration:** The application is not yet connected to SQL Server. Dapper implementation and end-to-end API/database integration remain outstanding.

**Next engineering milestone:** Implement the Dapper-backed SQL repository and connect the existing application/API architecture to the validated database.

**Project standard:** We must be able to explain not only what we built, but why it works, how it fails, and how we proved our fixes were effective.
