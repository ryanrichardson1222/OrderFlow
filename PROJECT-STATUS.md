OrderFlow — Project Checkpoint
Checkpoint date: November 27, 2026
Project Goal
Build and demonstrate a full-stack Order Management System using React, TypeScript, C#/.NET Web API, SQL Server, and Dapper.

Use the project as a practical engineering lab for:
- Full-stack application design and implementation.
- Layered architecture, dependency inversion, and SOLID principles.
- Relational database design and stored-procedure development.
- API-to-database integration and data mapping.
- Automated testing at application, API, and infrastructure levels.
- Systematic troubleshooting, failure analysis, and recovery.
- Secure configuration and appropriately scoped database access.
Guiding principle: Build it. Break it. Diagnose it. Fix it. Prove it's fixed.

Expanded engineering standard: Build it. Secure it. Break it. Diagnose it. Fix it. Prove it's fixed.
Current Project Position
OrderFlow has progressed from an in-memory .NET API and manually validated SQL scripts into a SQL-backed full-stack application with a React/TypeScript dashboard.
Current status at a glance
Area	Current position
Backend API	Implemented with .NET 10 minimal APIs
Application layer	Service and validation logic implemented
Infrastructure	Dapper-based SQL Server repository implemented
Database	Schema, seed data, view, and stored procedures deployed and validated
Persistence	API is connected to SQL Server through the infrastructure layer
Automated tests	API, Application, and Infrastructure test projects present and passing at the latest recorded verification
Frontend	React + TypeScript order-management dashboard implemented
Git	Backend and frontend work committed and pushed to main
Remaining work	End-to-end review, hardening, documentation, failure exercises, and final project verification


Important: This checkpoint reflects the latest project information available as of November 27, 2026. Re-run the build, tests, Git status, and live application checks before treating any status as a fresh verification.
Solution and Repository
- Repository: github.com/ryanrichardson1222/OrderFlow
- Local working directory: ~/Projects/OrderFlow
- Git branch: main
- Solution: OrderFlow.slnx
- Backend framework: .NET 10
- Frontend: React, TypeScript, and Vite
- Database: SQL Server 2022 Express
- Data access: Dapper
Backend projects
- OrderFlow.Api
- OrderFlow.Api.Tests
- OrderFlow.Application
- OrderFlow.Application.Tests
- OrderFlow.Infrastructure
- OrderFlow.Infrastructure.Tests
Frontend
- React 19
- TypeScript
- Vite
- ESLint
Recent Git Milestones
Backend
- Commit: d9fdfd8
- Message: Implement SQL-backed order mutations and infrastructure tests
- Result: SQL-backed order mutation behavior and infrastructure-level tests were implemented.
Frontend
- Commit: a2e301b
- Message: Add React TypeScript order management dashboard
- Result: React/TypeScript order-management dashboard added.
Both milestones were pushed to origin/main according to the latest recorded project checkpoint.
Architecture
The application follows a layered design:
React + TypeScript frontend
          |
          | HTTP / JSON
          v
ASP.NET Core Web API
          |
          v
Application layer
(OrderService and application rules)
          |
          | Repository abstraction
          v
Infrastructure layer
(SqlOrderRepository + Dapper)
          |
          v
SQL Server
(Stored procedures and relational schema)
Responsibilities
Frontend
- Presents the order-management dashboard.
- Retrieves and displays orders through the API.
- Supports order-management interactions and dashboard filtering/search functionality.
API
- Exposes HTTP endpoints.
- Receives requests and returns HTTP responses.
- Delegates application behavior to the application layer.
Application
- Contains order-related application logic and validation.
- Uses repository abstractions rather than depending directly on SQL Server or Dapper.
Infrastructure
- Implements the repository abstraction.
- Uses Dapper and Microsoft.Data.SqlClient to communicate with SQL Server.
- Keeps database-specific implementation details outside the application layer.
Database
- Stores users and orders.
- Enforces relational constraints.
- Exposes stored procedures for order operations.
- Implements soft-delete behavior.
Dependency injection
The API registers the SQL connection factory as a singleton and the SQL repository and order service as scoped services.
- ISqlConnectionFactory → SqlConnectionFactory (singleton)
- IOrderRepository → SqlOrderRepository (scoped)
- OrderService (scoped)
The connection factory reads ConnectionStrings:OrderFlow from configuration and throws an explicit configuration error if the connection string is missing or empty.

The application layer depends on the repository interface; it does not need to know that Dapper or SQL Server is used underneath.
Backend Implementation
Domain and application behavior
The order model includes:
- Id
- UserId
- CustomerName
- Status
- Total
Order creation references a user through UserId, rather than trusting a customer name supplied by the caller.

The application validates the request and uses the repository to confirm the referenced user exists before creating the order.

Known status values include:
- Pending
- Processing
- Shipped
- Cancelled
Status normalization is handled through the application's status logic.
SQL-backed persistence
The SQL repository is implemented in the Infrastructure project using:
- Dapper
- Microsoft.Data.SqlClient
- ISqlConnectionFactory
- Stored procedures in the dbo schema
The SQL connection factory centralizes connection creation and configuration.

The repository maps database results into application-facing models. This boundary is important because database column names and application model names do not need to be identical.
Relevant packages
- Dapper 2.1.89
- Microsoft.Data.SqlClient 7.1.1
- Microsoft.Extensions.Configuration.Abstractions 10.0.0
Automated Testing
The solution contains three test projects:
- OrderFlow.Api.Tests
- OrderFlow.Application.Tests
- OrderFlow.Infrastructure.Tests
Latest recorded test status
- API tests: 17 passed.
- Application tests: 39 passed.
- Infrastructure tests: added and passing in the latest recorded verification.
- Full solution test command: dotnet test OrderFlow.slnx
The exact Infrastructure test count is not recorded in this checkpoint.

The test suite has evolved beyond the original 14-test in-memory baseline. The test projects exercise API behavior, application validation and business rules, and infrastructure behavior.

Verification reminder: Run dotnet test OrderFlow.slnx again from the current working tree and record the exact results before a final release or interview demonstration.
Frontend Implementation
The React/TypeScript frontend now provides an Order Management dashboard.

Known implementation details:
- React 19 and TypeScript.
- Vite development server.
- API proxy configured for /api to the local .NET API at http://localhost:5137.
- Order listing and dashboard presentation.
- Search and filtering functionality.
- Status-oriented dashboard cards.
- Frontend lint and production build were previously verified successfully.
The frontend was committed in a2e301b.
Local development endpoints
- API HTTP: http://localhost:5137
- API HTTPS profile: https://localhost:7144
- Vite proxies /api requests to the API HTTP endpoint.
Start the API and frontend using the repository's current launch instructions and configuration. Confirm the configured ports before a demonstration.
SQL Server Environment
- SQL Server: 2022 Express
- Instance: SQLEXPRESS
- Windows VM hostname: TheDevenant
- SQL Server address used by the application: 192.168.236.135,1433
- Database used for deployment validation: OrderFlowDeploymentTestV2
- Application connection-string key: ConnectionStrings:OrderFlow
- Application SQL login: orderflow_app
The SQL Server environment is hosted on the Windows VM, while the API and frontend development workflow runs on the Mac.
Connectivity and configuration
The API connects to SQL Server over the network using the configured connection string.

The application connection is configured to use encryption and trust the development SQL Server certificate. This is a development-lab configuration; certificate trust and certificate validation should be revisited before any production-style deployment.

The API uses User Secrets for local development configuration. Connection strings containing credentials must not be committed to source control.
Database Design
dbo.Users
- UserId — INT IDENTITY, primary key
- FirstName — NVARCHAR(50)
- LastName — NVARCHAR(50)
- Email — NVARCHAR(255)
- CreatedAt — DATETIME2(0), defaults to UTC system time
dbo.Orders
- OrderId — INT IDENTITY, primary key
- UserId — INT, foreign key
- OrderDate — DATETIME2(0), defaults to UTC system time
- Status — NVARCHAR(30), defaults to Pending
- Total — DECIMAL(18,2)
- IsDeleted — BIT, defaults to 0
- DeletedAt — DATETIME2(0), nullable
Constraints
The validated schema includes named constraints for:

Users
- PK_Users
- UQ_Users_Email
- DF_Users_CreatedAt
Orders
- PK_Orders
- FK_Orders_Users
- CK_Orders_Total
- CK_Orders_Status
- DF_Orders_OrderDate
- DF_Orders_Status
- DF_Orders_IsDeleted
The original checkpoint described these as ten named constraints, but the listed names total eleven. The schema's current definition should be treated as authoritative and recounted during the next database review.
Database Scripts and Stored Procedures
Database scripts are located in database/scripts/:
1. 01-create-schema.sql
2. 02-seed-data.sql
3. 03-create-view.sql
4. 04-verify-database.sql
5. 05-create-stored-procedures.sql
The scripts use a configurable SQLCMD DatabaseName variable.
Stored procedures
Seven procedures were deployed and manually validated:
1. dbo.usp_GetAllOrders
2. dbo.usp_GetOrderById
3. dbo.usp_CreateOrder
4. dbo.usp_UpdateOrderStatus
5. dbo.usp_DeleteOrder
6. dbo.usp_RestoreOrder
7. dbo.usp_GetAllOrdersIncludingDeleted
The procedures cover active-order retrieval, single-order retrieval, creation, status updates, soft deletion, restoration, and administrative retrieval including deleted orders.
Soft-delete behavior
- IsDeleted defaults to 0.
- Deletion sets IsDeleted and records DeletedAt.
- Normal retrieval excludes deleted orders.
- Administrative retrieval includes deleted orders and deletion metadata.
- Restore resets IsDeleted to 0 and clears DeletedAt.
- The user-orders view excludes deleted orders.
Deployment and validation history
The earlier database work remains important engineering evidence:
- FullStackLearning was used as the original development database.
- OrderFlowFreshTest was used for fresh schema validation.
- OrderFlowDeploymentTest (V1) exposed a deployment mismatch caused by a stale schema-script copy. The view failed because IsDeleted was absent from the deployed table.
- OrderFlowDeploymentTestV2 was created as a fresh validation database. The corrected schema, seed data, view, stored procedures, and verification script deployed successfully.
- Constraint-failure tests confirmed that duplicate email, nonexistent user references, and negative order totals were rejected.
- Stored-procedure error scenarios were exercised.
- Create, retrieve, soft-delete, administrative retrieval, and restore behavior were smoke-tested.
Deployment caveat: 01-create-schema.sql creates missing tables but is not an incremental migration system. It does not automatically add new columns to an existing table created from an older schema.
Runtime Validation
The latest recorded live API/database validation showed:
- The API successfully connected to SQL Server.
- GET /api/orders returned six active orders.
- The dashboard displayed the returned orders.
- The displayed total was $2,488.42.
- The dashboard showed four Pending and two Shipped orders.
- A new order was successfully created through the API with a total of $199.99.
These are historical observed results, not a guarantee of the current database state. Re-run the API and UI checks before relying on these values in a demonstration.
Security and Configuration Standards
- Use synthetic data only.
- Never use the SQL Server sa account from the application.
- Use the dedicated orderflow_app login with appropriately scoped permissions.
- Do not commit passwords, credentials, secret-bearing connection strings, database backups, or production data.
- Store development secrets outside source control.
- Keep development and validation databases separate when testing deployment or recovery.
- Validate request data at the application boundary.
- Keep SQL access parameterized through Dapper and stored-procedure parameters.
- Review stored-procedure permissions and grant only what the application requires.
- Revisit development certificate-trust settings before production-style deployment.
- Consider authentication and authorization before exposing the API beyond the local lab.
Troubleshooting and Failure-Engineering Evidence
The project has already produced useful real-world troubleshooting exercises:
1. Stale schema deployment: A fresh deployment failed because the Windows-side schema script was outdated. The mismatch was isolated, the corrected script was transferred and inspected, and a new V2 deployment was validated.
2. Database permissions: A SQL permission issue prevented an operation from completing. The database permissions were investigated and corrected.
3. Cross-machine SQL connectivity: SQL Server service state, TCP configuration, VM networking, and Mac-to-Windows connectivity were part of the physical-to-application troubleshooting path.
4. Frontend/API integration: The dashboard's ability to display orders depends on the complete chain: React → API → application service → repository → SQL Server.
Continue turning failures into documented exercises. For each exercise, record the symptom, initial hypothesis, diagnostic evidence, root cause, corrective action, and verification that proves the fix.
Remaining Work and Next Milestones
OrderFlow is not considered complete merely because the dashboard renders or the API returns HTTP 200. The remaining work is to verify the full system, understand its behavior, harden it, and document it to a standard suitable for technical interviews.
1. Final end-to-end validation
- Start SQL Server and verify the database is online.
- Verify Mac-to-Windows SQL connectivity.
- Start the API and confirm its health.
- Start the React frontend.
- Verify order retrieval, creation, status updates, deletion, and restoration through the UI/API/database chain.
- Confirm expected behavior for empty results, invalid input, missing records, deleted records, and database failures.
- Capture fresh test and build results.
2. Backend and architecture review
- Review the API, Application, and Infrastructure boundaries.
- Explain dependency inversion and why the application depends on IOrderRepository.
- Explain dependency-injection lifetimes.
- Review Dapper mapping and SQL parameterization.
- Review asynchronous connection and command handling.
- Confirm error translation from SQL/database failures to application and HTTP responses.
- Identify any remaining duplicated logic or unnecessary abstractions.
3. Security review
- Verify the application's SQL login has only required permissions.
- Review User Secrets and repository hygiene.
- Review input validation and database constraints together.
- Evaluate authentication and authorization.
- Review CORS and development server configuration.
- Review logging so useful diagnostic information is retained without exposing secrets.
4. Failure exercises
Deliberately test controlled failures in the isolated lab:
- Stop SQL Server.
- Break or remove the API connection string.
- Deny a required database permission.
- Cause a stored procedure to return or raise an unexpected result.
- Submit invalid API payloads.
- Request nonexistent order IDs.
- Exercise deleted-order and restore edge cases.
- Break the frontend API proxy.
- Introduce a test failure and diagnose it.
- Restore the environment and prove the application works again.
Use snapshots and backups before destructive database or VM exercises.
5. Documentation and presentation
Complete or review:
- README.md — project overview, stack, architecture, setup, usage, and verification.
- PROJECT-STATUS.md — current progress and next milestones.
- ARCHITECTURE.md — component responsibilities and dependency flow.
- SECURITY.md — security decisions and limitations.
- FAILURE-RECOVERY.md — deliberate failure exercises and recovery evidence.
Prepare to explain the project without relying on a live demo: architecture, request flow, persistence, testing strategy, security decisions, failures encountered, and how fixes were verified.
6. Completion gate
Before moving on to the next major portfolio project:
- All required features work end to end.
- The full solution builds.
- All automated tests pass.
- Database deployment and recovery steps are documented and repeatable.
- Security decisions and limitations are documented.
- Failure exercises have been completed and verified.
- The user can explain the system's design and tradeoffs line by line at the appropriate level.
After OrderFlow is complete, conduct a formal first-standard code review. Then resume the Terraform lab. Red Hat/Linux and the broader engineering portfolio follow the agreed learning sequence.
Current Project Summary
Application: Full-stack Order Management System with a .NET 10 API, application layer, SQL-backed infrastructure, and React/TypeScript dashboard.

Database: SQL Server schema, constraints, synthetic data, view, soft-delete lifecycle, and seven stored procedures deployed and validated.

Integration: Dapper-based SQL repository implemented and registered through dependency injection. The API has successfully retrieved and mutated SQL-backed order data in the recorded runtime validation.

Testing: API, Application, and Infrastructure test projects are present. The latest recorded API and Application test results were 17/17 and 39/39 respectively; Infrastructure tests were also added and passing. Re-run the complete suite for a fresh authoritative result.

Frontend: React/TypeScript dashboard implemented, with search/filter and status-oriented presentation.