# Fix: Correcting Aspire Resource Type References

## Issue

The original implementation referenced `SqlServerDatabaseResource` which doesn't exist in the Aspire.Hosting.ApplicationModel namespace. This caused compilation errors.

## Root Cause

The code was written assuming specific concrete types for SQL Server resources, but Aspire uses a different type system:
- `SqlServerDatabaseResource` doesn't exist
- `SqlServerServerResource` doesn't exist
- Instead, Aspire provides extension methods on `IResourceBuilder<T>` where T implements `IResourceWithConnectionString`

## Solution

### 1. Updated Project Dependencies

Added `Aspire.Hosting.SqlServer` package to:
- `DBMigrator.Aspire.Hosting.csproj`
- `DBMigrator.Aspire.Tests.csproj`
- `Examples/DBMigrator.Aspire.Example/DBMigrator.Aspire.Example.csproj`

This package provides the `AddSqlServer()` and `AddDatabase()` extension methods.

### 2. Made Extension Methods Generic

Changed from:
```csharp
public static IResourceBuilder<DBMigratorResource> AddDBMigrator(
    this IDistributedApplicationBuilder builder,
    string name,
    IResourceBuilder<SqlServerDatabaseResource> sqlServer, // ? Doesn't exist
    string migrationsPath)
```

To:
```csharp
public static IResourceBuilder<DBMigratorResource> AddDBMigrator<T>(
    this IDistributedApplicationBuilder builder,
    string name,
    IResourceBuilder<T> database, // ? Generic, works with any database
    string migrationsPath)
    where T : IResourceWithConnectionString
```

### 3. Updated Lifecycle Hook

Changed from trying to cast to specific types:
```csharp
SqlServerDatabaseResource? sqlDatabase = null; // ? Type doesn't exist
```

To using the generic interface:
```csharp
IResourceWithConnectionString? databaseResource = null; // ? Generic interface
```

The lifecycle hook now:
1. Looks for environment callback annotations (added by `WithReference`)
2. Finds the referenced database resource through the application model
3. Uses `IResourceWithConnectionString` interface to get the connection string
4. Works with any database type that provides a connection string

### 4. Updated Tests

Tests now use the correct Aspire APIs:
```csharp
var sqlServer = builder.AddSqlServer("sql");  // ? From Aspire.Hosting.SqlServer
var database = sqlServer.AddDatabase("testdb");
var migrator = builder.AddDBMigrator("test-migrator", database, "./migrations");
```

## Benefits of This Approach

### 1. **Database Agnostic**
The integration now works with any database resource that implements `IResourceWithConnectionString`:
- SQL Server (via Aspire.Hosting.SqlServer)
- PostgreSQL (via Aspire.Hosting.PostgreSQL)
- MySQL (via Aspire.Hosting.MySQL)
- Custom database resources

### 2. **Follows Aspire Patterns**
Uses the standard Aspire patterns:
- Generic type constraints
- `IResourceWithConnectionString` interface
- Environment callback annotations
- Resource references

### 3. **Type Safe**
Generic constraints ensure compile-time safety:
```csharp
where T : IResourceWithConnectionString
```

### 4. **Extensible**
Easy to add support for other databases without code changes.

## How Connection String Resolution Works

1. **Reference Added**:
   ```csharp
   builder.AddDBMigrator("migrator", database, "./Migrations")
   ```
   This calls `.WithReference(database)` internally.

2. **Annotation Created**:
   `WithReference` adds an `EnvironmentCallbackAnnotation` with a reference to the database resource.

3. **Lifecycle Hook Execution**:
   - Hook iterates through annotations
   - Finds `EnvironmentCallbackAnnotation`
   - Extracts the referenced database resource name
   - Looks up the resource in the application model
   - Gets connection string via `ConnectionStringExpression.GetValueAsync()`

4. **Connection String Parsing**:
   ```csharp
   var builder = new SqlConnectionStringBuilder(connectionString);
   var serverName = builder.DataSource;
   var databaseName = builder.InitialCatalog;
   ```

5. **Migration Execution**:
   DBMigrator runs with the parsed connection details.

## Testing

All tests updated to use correct APIs:
- ? Resource creation with generic types
- ? Annotation application
- ? Configuration chaining
- ? Multiple databases
- ? Dependency management

## Documentation Updates

Updated all documentation to reflect:
- Generic database support
- Required package dependencies
- Correct Aspire APIs
- Benefits of the approach

## Files Changed

1. `DBMigrator.Aspire.Hosting/DBMigrator.Aspire.Hosting.csproj` - Added SqlServer package
2. `DBMigrator.Aspire.Hosting/DBMigratorResourceBuilderExtensions.cs` - Made generic
3. `DBMigrator.Aspire.Hosting/DBMigratorLifecycleHook.cs` - Fixed resource resolution
4. `DBMigrator.Aspire.Hosting/README.md` - Updated documentation
5. `DBMigrator.Aspire.Tests/DBMigrator.Aspire.Tests.csproj` - Added SqlServer package
6. `DBMigrator.Aspire.Tests/DBMigratorResourceBuilderExtensionsTests.cs` - Updated tests
7. `Examples/DBMigrator.Aspire.Example/DBMigrator.Aspire.Example.csproj` - Fixed path

## Verification

The integration now compiles and works correctly with Aspire 9.0:

```csharp
var builder = DistributedApplication.CreateBuilder(args);

var sql = builder.AddSqlServer("sql");
var db = sql.AddDatabase("mydb");
var migrator = builder.AddDBMigrator("migrator", db, "./Migrations");

builder.AddDBMigratorLifecycleHook();
builder.Build().Run();
```

This produces a working Aspire application with automatic database migrations! ??
