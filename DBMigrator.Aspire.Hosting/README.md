# DBMigrator.Aspire.Hosting

This package provides .NET Aspire integration for DBMigrator, allowing you to run database migrations as part of your Aspire application lifecycle.

## Installation

```bash
dotnet add package DBMigrator.Aspire.Hosting
dotnet add package Aspire.Hosting.SqlServer
```

## Usage

### Basic Setup with Path

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

### Using Project References (Recommended)

**New in .NET 10/Aspire 13:** Use project references to automatically resolve migrations path, similar to Aspire's `AddProject`:

```csharp
var builder = DistributedApplication.CreateBuilder(args);

var sqlServer = builder.AddSqlServer("sql");
var database = sqlServer.AddDatabase("mydb");

// Add DBMigrator using project reference - automatically finds Migrations folder
var dbMigrator = builder.AddDBMigrator<Projects.MyMigrations>("dbmigrator", database);

// Register the lifecycle hook
builder.AddDBMigratorLifecycleHook();

var api = builder.AddProject<Projects.MyApi>("api")
    .WaitForDBMigrator(dbMigrator);

builder.Build().Run();
```

#### With Custom Migrations Path

Specify a different relative path within the project:

```csharp
// Migrations are in MyMigrations/Database/Scripts
var dbMigrator = builder.AddDBMigrator<Projects.MyMigrations>(
    "dbmigrator", 
    database, 
    relativeMigrationsPath: "Database/Scripts");
```

#### Without Database Reference

If you want to configure the database reference separately:

```csharp
var dbMigrator = builder.AddDBMigrator<Projects.MyMigrations>("dbmigrator")
    .WithDatabaseReference(database)
    .WithTargetVersion("2.0");
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

#### Chaining Multiple Options

```csharp
var dbMigrator = builder.AddDBMigrator<Projects.MyMigrations>("dbmigrator", database)
    .WithTargetVersion("2.0")
    .WithSkipValidation()
    .WithDryRun();
```

#### Multiple Databases

Migrate multiple databases:

```csharp
var sqlServer = builder.AddSqlServer("sql");

var db1 = sqlServer.AddDatabase("database1");
var migrator1 = builder.AddDBMigrator<Projects.Database1Migrations>("migrator1", db1);

var db2 = sqlServer.AddDatabase("database2");
var migrator2 = builder.AddDBMigrator<Projects.Database2Migrations>("migrator2", db2);

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
2. When using project references, the migrations path is automatically resolved from the project directory
3. The `DBMigratorLifecycleHook` runs before your application starts
4. DBMigrator connects to the specified database using the connection string
5. Migrations are discovered from the specified folder path
6. The database is validated (unless skipped)
7. Pending migrations are applied in order
8. Your application resources that depend on the database start after migrations complete

## Migration Folder Structure

DBMigrator expects migrations to be organized by version and feature:

```
MyMigrations/
??? Migrations/              # Default folder name
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

## API Reference

### Extension Methods

#### `AddDBMigrator(name, migrationsPath)`
Adds DBMigrator with an explicit path to migrations.

#### `AddDBMigrator<TProject>(name, relativeMigrationsPath)`
Adds DBMigrator using a project reference to automatically resolve the migrations path.

#### `AddDBMigrator(name, database, migrationsPath)`
Adds DBMigrator with database reference and explicit migrations path.

#### `AddDBMigrator<TProject, TDatabase>(name, database, relativeMigrationsPath)`
Adds DBMigrator with database reference using project reference for migrations path.

#### `WithTargetVersion(version)`
Configures the target version to migrate to.

#### `WithSkipValidation(skipValidation = true)`
Skips database validation before migration.

#### `WithDryRun(dryRun = true)`
Runs migrations without committing changes.

#### `WithDatabaseReference(database)`
Adds a database reference to an existing DBMigrator resource.

#### `WaitForDBMigrator(dbMigrator)`
Configures a resource to wait for DBMigrator to complete before starting.

#### `AddDBMigratorLifecycleHook()`
Registers the lifecycle hook that executes migrations before app startup.

## Features

- ? Automatic migration execution during Aspire app startup
- ? Integration with Aspire's resource management
- ? **Project reference support** for automatic path resolution
- ? Support for multiple databases
- ? Works with any database resource that provides a connection string
- ? Configurable target versions
- ? Optional database validation
- ? Dry run mode for testing
- ? Comprehensive logging
- ? Dependency management with `WaitFor`

## Requirements

- .NET 9.0 or later (.NET 10.0 for project reference support)
- .NET Aspire 9.0 or later (Aspire 13.0 for project reference support)
- SQL Server (or any database supported by DBMigrator)

## See Also

- [DBMigrator Documentation](../README.md)
- [.NET Aspire Documentation](https://learn.microsoft.com/dotnet/aspire/)
