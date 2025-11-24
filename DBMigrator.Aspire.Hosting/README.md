# DBMigrator.Aspire.Hosting

This package provides .NET Aspire integration for DBMigrator, allowing you to run database migrations as part of your Aspire application lifecycle.

## Installation

```bash
dotnet add package DBMigrator.Aspire.Hosting
dotnet add package Aspire.Hosting.SqlServer
```

## Usage

### Basic Setup

In your Aspire AppHost project, add DBMigrator to your distributed application:

```csharp
var builder = DistributedApplication.CreateBuilder(args);

// Add SQL Server
var sqlServer = builder.AddSqlServer("sql");
var database = sqlServer.AddDatabase("mydb");

// Add DBMigrator with path to migrations folder
var dbMigrator = builder.AddDBMigrator("dbmigrator", database, "./Migrations");

// Register the lifecycle hook to run migrations before app starts
builder.AddDBMigratorLifecycleHook();

// Your application resources can wait for migrations to complete
var api = builder.AddProject<Projects.MyApi>("api")
    .WaitForDBMigrator(dbMigrator);

builder.Build().Run();
```

### Advanced Configuration

#### Specify Target Version

Migrate to a specific version instead of the latest:

```csharp
var dbMigrator = builder.AddDBMigrator("dbmigrator", database, "./Migrations")
    .WithTargetVersion("2.0");
```

#### Skip Database Validation

Skip validation checks before migration:

```csharp
var dbMigrator = builder.AddDBMigrator("dbmigrator", database, "./Migrations")
    .WithSkipValidation();
```

#### Dry Run Mode

Run migrations without committing changes (useful for testing):

```csharp
var dbMigrator = builder.AddDBMigrator("dbmigrator", database, "./Migrations")
    .WithDryRun();
```

#### Multiple Databases

Migrate multiple databases:

```csharp
var sqlServer = builder.AddSqlServer("sql");

var db1 = sqlServer.AddDatabase("database1");
var migrator1 = builder.AddDBMigrator("migrator1", db1, "./Migrations/Database1");

var db2 = sqlServer.AddDatabase("database2");
var migrator2 = builder.AddDBMigrator("migrator2", db2, "./Migrations/Database2");

builder.AddDBMigratorLifecycleHook();
```

#### Using Any Database with Connection String

DBMigrator works with any resource that implements `IResourceWithConnectionString`:

```csharp
// Works with SQL Server
var sqlDb = builder.AddSqlServer("sql").AddDatabase("mydb");
var migrator = builder.AddDBMigrator("migrator", sqlDb, "./Migrations");

// Can also work with other database types if they provide connection strings
```

## How It Works

1. The `DBMigratorResource` is added to your Aspire application model
2. The `DBMigratorLifecycleHook` runs before your application starts
3. DBMigrator connects to the specified database using the connection string
4. Migrations are discovered from the specified folder path
5. The database is validated (unless skipped)
6. Pending migrations are applied in order
7. Your application resources that depend on the database start after migrations complete

## Migration Folder Structure

DBMigrator expects migrations to be organized by version and feature:

```
Migrations/
??? 1.0/
?   ??? Feature1/
?   ?   ??? Migrations/
?   ?       ??? 1_CreateTables.sql
?   ?       ??? 1_rollback_CreateTables.sql
?   ??? Feature2/
?       ??? Migrations/
?           ??? 1_AddIndexes.sql
??? 2.0/
    ??? Feature3/
        ??? Migrations/
            ??? 1_AddNewColumn.sql
```

## Features

- ? Automatic migration execution during Aspire app startup
- ? Integration with Aspire's resource management
- ? Support for multiple databases
- ? Works with any database resource that provides a connection string
- ? Configurable target versions
- ? Optional database validation
- ? Dry run mode for testing
- ? Comprehensive logging
- ? Dependency management with `WaitFor`

## Requirements

- .NET 9.0 or later
- .NET Aspire 9.0 or later
- SQL Server (or any database supported by DBMigrator)

## See Also

- [DBMigrator Documentation](../README.md)
- [.NET Aspire Documentation](https://learn.microsoft.com/dotnet/aspire/)
