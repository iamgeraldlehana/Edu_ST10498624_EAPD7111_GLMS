# Part 2: Core Prototype and Unit Testing

**Student Number:** ST10498624

TechMove Global Logistics Management System is an ASP.NET Core MVC application.
The pages, database code and main rules are kept in one project.

## Solution

- `GLMS.sln` - Visual Studio solution.
- `src/GLMS.Web` - MVC web application and EF Core SQL Server context.
- `tests/GLMS.Tests` - xUnit tests.
- `database/migrations/InitialCreate.sql` - SQL Server migration script.
- `documents/ST10498624_EAPD7111_Part_2.pdf` - Part 2 document.

## Implemented Features

- Client, Contract and ServiceRequest models. One client can have many contracts,
  and one contract can have many requests.
- Contract search by status and date range.
- PDF agreement upload and download.
- Service request rule that blocks expired and on-hold contracts.
- Gets a USD-to-ZAR rate from an API and calculates the ZAR cost.
- Abstract Factory for Standard and Premium contracts and their pricing rules.
- Adapter for the currency API response.
- Observer for saved Finance and Compliance updates on contract status changes.
- Unit tests for currency calculation, PDF validation and contract workflow.

## Local Check

`dotnet test GLMS.sln --configuration Release` passes 33 tests.

Some tests use a temporary database held in memory. They check the code without
connecting to SQL Server.

## Run

Requires the .NET 8 SDK and SQL Server. The default connection string in
`src/GLMS.Web/appsettings.json` uses Windows LocalDB.
To use another SQL Server database, set the
`ConnectionStrings__GlmsDatabase` environment variable.

```sh
dotnet restore GLMS.sln
dotnet tool restore
dotnet ef database update --project src/GLMS.Web
dotnet run --project src/GLMS.Web --urls http://localhost:5188
```

The SQL script in `database/migrations/InitialCreate.sql` creates all the tables,
including the department updates table. For a new database, either run this
script or use the EF database update command above.

## Test

```sh
dotnet test GLMS.sln --configuration Release
```

The GitHub Actions workflow runs the build and tests after a push or pull
request.
