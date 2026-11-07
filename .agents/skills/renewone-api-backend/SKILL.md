---
name: renewone-api-backend
description: Use when implementing or repairing API endpoints, business rules, persistence, authorization, EF Core schema, or external integrations in the RenewOne .NET backend.
---

# RenewOne API backend workflow

Use this workflow for backend changes in RenewOne-Api. The repository-root `AGENTS.md` describes project boundaries and general conventions.

## 1. Trace the current behavior

- Inspect the endpoint/controller, its application interface and logic service, repository interface and implementation, relevant entity/DTO, and registrations in `RenewOne.API/Program.cs`.
- Check existing authorization attributes, route and binding conventions, validation, and error/response mapping in the same feature.
- Treat source as authoritative. The README is only a placeholder, and similar endpoints may have different response behavior.
- Check whether the behavior already exists in another service or repository before creating a duplicate abstraction.

## 2. Keep changes in the established layers

- Controllers in `RenewOne.API` handle HTTP routing, model binding, authorization attributes, and conversion of application results to HTTP responses.
- Business decisions belong in `RenewOne.Application` logic services and their interfaces. Keep request and response contracts in the existing DTO folders.
- Put EF Core queries and persistence operations in `RenewOne.Persistence` repositories. Use `AppDbContext` and the existing unit-of-work pattern for writes and transactions.
- Keep provider-specific behavior in `RenewOne.Infrastructure` behind interfaces already declared by the application layer where available.
- Register new services in `Program.cs` and match the existing lifetime. Use asynchronous methods and propagate `CancellationToken` when the endpoint path accepts one.
- Preserve the `Result<T>` envelope and endpoint-specific status mapping. Do not change the API contract or authorization policy incidentally while refactoring.

## 3. Handle persistence changes carefully

- For schema changes, update the domain entity and any `AppDbContext` mapping, then create an EF Core migration in `RenewOne.Persistence/Migrations`.
- Scaffold migrations from the repository root with:

  ```bash
  dotnet ef migrations add <Name> --project RenewOne.Persistence/RenewOne.Persistence.csproj --startup-project RenewOne.API/RenewOne.API.csproj
  ```

- Review the migration `Up` and `Down`, generated metadata, and model snapshot. Keep data migration and rollback behavior safe; do not edit old migrations to encode a new change.
- Do not apply a migration to a shared or production database unless the user explicitly asks for it.

## 4. Protect configuration and external boundaries

- Read configuration through the existing `IConfiguration` / options patterns. Local `appsettings.json` files are ignored and may contain secrets; never expose their contents or commit credentials.
- Keep payment, storage, RC lookup, and other provider-specific calls behind their existing service interfaces. Avoid logging tokens, payment credentials, or unnecessary personal data.
- For user-scoped data, verify role checks and current-user/claim handling against the existing implementation rather than trusting client-supplied identifiers alone.

## 5. Verify and report

- Build with `dotnet build RenewOne.API/RenewOne.API.csproj` when appropriate to the task.
- The initial repository review did not find a test project. Recheck before using `dotnet test`; do not imply the API has test coverage if no test project exists.
- If behavior changes and a suitable test project is present or requested, add focused coverage using its existing framework and patterns.
- Summarize the files and API behavior changed, database migration impact if any, and exactly which verification commands ran.
