# Agent instructions

## Project

RenewOne-Api is an ASP.NET Core Web API targeting .NET 10. The solution is split across five projects: `RenewOne.API`, `RenewOne.Application`, `RenewOne.Domain`, `RenewOne.Infrastructure`, and `RenewOne.Persistence`. The API uses EF Core with SQL Server, JWT bearer authentication, SignalR, and Serilog. Integrations include payment, vehicle registration lookup, file storage, and report export services.

The README currently contains only a title. Use the implementation and project files as the source of truth, and inspect neighboring code before making architectural assumptions.

## Project boundaries

- `RenewOne.API/`: ASP.NET Core startup and dependency registration (`Program.cs`), controllers, hubs, and OpenAPI setup.
- `RenewOne.Application/`: DTOs, filters, application interfaces, business logic services, shared results, and pagination helpers.
- `RenewOne.Domain/`: entities, enums, and domain base types.
- `RenewOne.Persistence/`: `AppDbContext`, EF Core mappings, repositories, unit of work, and migrations.
- `RenewOne.Infrastructure/`: implementations for external services and infrastructure options.

Current project references are not a perfectly strict Clean Architecture arrangement. Follow the existing references and patterns; do not move layers or add a framework-wide abstraction just for one feature.

## Working conventions

- Trace a request through the controller, application interface and logic service, repository interface and implementation, and entity/DTO before changing behavior.
- Keep HTTP concerns in controllers, business rules in application logic services, persistence queries in repositories, and external integration details behind the existing service interfaces.
- Register new implementations in `RenewOne.API/Program.cs` using the existing DI lifetime and naming patterns. Check whether the interface and implementation already exist before adding another.
- Use async APIs and pass `CancellationToken` through controller, service, and repository calls when the surrounding API supports it.
- Preserve the endpoint's current authorization attributes, route shape, DTO contract, and `Result<T>` response envelope. Controllers differ in how they translate `Result.StatusCode` to HTTP responses; follow the specific controller's established behavior.
- Follow nullable reference types and implicit usings, both enabled in the project files. Match nearby C# formatting and namespace style.
- For read-only queries, follow the repository's existing tracking behavior; many repository query methods use `AsNoTracking()`.
- For schema changes, update the entity/model configuration as needed and create/review an EF Core migration. Do not modify historical migrations to represent a new schema change, and do not apply a migration to a shared or production database without an explicit request.
- `appsettings.json` and `appsettings.Development.json` are ignored local configuration files. Treat their contents as secrets: do not print them, commit credentials, or hard-code keys, tokens, connection strings, or payment credentials into source.
- Avoid logging authentication material, payment data, or unnecessary personal information.
- Preserve existing workspace changes and generated migration history. Do not use destructive Git commands to clean the workspace.

## Useful commands

- `dotnet build RenewOne.API/RenewOne.API.csproj` — build the API and referenced projects.
- `dotnet run --project RenewOne.API/RenewOne.API.csproj` — run locally when local configuration and database access are set up.
- `dotnet ef migrations add <Name> --project RenewOne.Persistence/RenewOne.Persistence.csproj --startup-project RenewOne.API/RenewOne.API.csproj` — scaffold a database migration.

No test project was found during the initial repository review. Check for a test project before relying on `dotnet test`; report only the checks that were actually run.
