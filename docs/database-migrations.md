# Database migrations (Azure SQL Server)

The API uses **EF Core migrations** generated from [`MasterMindDbContext`](../src/backend/MasterMind.API/Data/MasterMindDbContext.cs). Migration files live under [`src/backend/MasterMind.API/Data/Migrations/`](../src/backend/MasterMind.API/Data/Migrations/).

## Design-time connection (creating migrations)

By default, `dotnet ef` uses [`MasterMindDbContextFactory`](../src/backend/MasterMind.API/Data/MasterMindDbContextFactory.cs), which targets **SQL Server LocalDB**:

`Server=(localdb)\mssqllocaldb;Database=MasterMindDesign;Trusted_Connection=True;TrustServerCertificate=True`

Override with:

```powershell
$env:MASTERMIND_DESIGN_CONNECTION = "Server=tcp:your-server.database.windows.net;Database=YourDb;User ID=...;Password=...;Encrypt=True;"
```

## Commands

From [`src/backend/MasterMind.API`](../src/backend/MasterMind.API):

```powershell
dotnet ef migrations add MigrationName -c MasterMindDbContext -o Data/Migrations
dotnet ef database update -c MasterMindDbContext --connection "YOUR_AZURE_SQL_CONNECTION_STRING"
```

Generate a SQL script for DBA review:

```powershell
dotnet ef migrations script -c MasterMindDbContext -o migrate.sql --connection "YOUR_CONNECTION_STRING"
```

## Runtime behavior

- **Azure SQL / SQL Server**: On startup the app runs `Database.MigrateAsync()` so new migrations apply automatically.
- **PostgreSQL**: Startup still runs the embedded raw SQL bootstrap (Railway / legacy); migrations are not applied there unless you align strategy later.
- **SQLite** (local fallback): Startup uses `EnsureCreatedAsync`; migration SQL is SQL Server–specific and is not applied on SQLite.

## Existing production databases

If tables **already exist** with a different shape than `InitialCreate`, `Migrate()` may fail (duplicate object errors). Options:

1. Apply a one-time baseline (align schema manually or use EF baseline workflows), then use incremental migrations only.
2. Deploy to an **empty** Azure SQL database so `InitialCreate` applies cleanly.

## Related

See [`docs/ai/gotchas.md`](ai/gotchas.md) for Azure SQL vs `EnsureCreated`.

## Example: Classes API (GET vs POST)

Listing classes for a session uses **GET** with a query string only:

```http
GET /api/classes?sessionId=2
Authorization: Bearer …
```

Creating a class uses **POST** with a JSON body (do not send the body on GET):

```http
POST /api/classes
Authorization: Bearer …
Content-Type: application/json

{"name":"Class 10 - CBSE","medium":"English","board":"CBSE","subjects":["Mathematics","Science"],"isActive":true}
```
