# Project Reference Support for AddDBMigrator

## Summary

Added support for project references in `AddDBMigrator` extension methods, similar to how Aspire's `AddProject` works. This allows automatic resolution of migrations path from project directories using compile-time generated project metadata.

## What Was Added

### New Extension Method Overloads

1. **`AddDBMigrator<TProject>(name, relativeMigrationsPath)`**
   - Accepts a project reference via generic type parameter
   - Automatically resolves migrations path from project directory
   - Default relative path: "Migrations"

2. **`AddDBMigrator<TProject, TDatabase>(name, database, relativeMigrationsPath)`**
   - Combines project reference with database reference
   - Provides full configuration in a single call
   - Supports fluent chaining with other configuration methods

### New Annotation

- **`DBMigratorProjectReferenceAnnotation`**: Stores the project path reference for tracking and potential future enhancements

## Usage Examples

### Basic Usage with Project Reference

```csharp
var builder = DistributedApplication.CreateBuilder(args);

var sqlServer = builder.AddSqlServer("sql");
var database = sqlServer.AddDatabase("mydb");

// Automatically finds Migrations folder in the referenced project
var dbMigrator = builder.AddDBMigrator<Projects.MyMigrationsProject>("dbmigrator", database);

builder.AddDBMigratorLifecycleHook();
builder.Build().Run();
```

### Custom Migrations Path

```csharp
// Migrations are in MyMigrationsProject/Database/Scripts
var dbMigrator = builder.AddDBMigrator<Projects.MyMigrationsProject>(
    "dbmigrator", 
    database, 
    relativeMigrationsPath: "Database/Scripts");
```

### With Configuration Chaining

```csharp
var dbMigrator = builder.AddDBMigrator<Projects.MyMigrationsProject>("dbmigrator", database)
    .WithTargetVersion("2.0")
    .WithSkipValidation()
    .WithDryRun();
```

### Separate Database Reference

```csharp
var dbMigrator = builder.AddDBMigrator<Projects.MyMigrationsProject>("dbmigrator")
    .WithDatabaseReference(database)
    .WithTargetVersion("2.0");
```

## Benefits

1. **Type Safety**: Compile-time checking of project references
2. **Convention Over Configuration**: Follows Aspire patterns
3. **Automatic Path Resolution**: No manual path string configuration needed
4. **Refactoring Friendly**: Moves and renames are caught at compile time
5. **Cleaner Code**: More concise and expressive API

## Backward Compatibility

All existing overloads remain unchanged:
- `AddDBMigrator(name, migrationsPath)` - Still supported
- `AddDBMigrator(name, database, migrationsPath)` - Still supported

The new overloads are purely additive and optional.

## Testing

- Added 4 new unit tests specifically for project reference functionality
- All 11 unit tests passing (100%)
- Tests cover:
  - Path resolution from project metadata
  - Custom relative path handling
  - Database reference integration
  - Configuration chaining
  - Annotation verification

## Files Modified

1. **DBMigratorResourceBuilderExtensions.cs**: Added new generic overloads
2. **DBMigratorAnnotations.cs**: Added `DBMigratorProjectReferenceAnnotation`
3. **DBMigratorResourceBuilderExtensionsTests.cs**: Added comprehensive tests
4. **README.md**: Updated documentation with examples
5. **Program.cs** (Example): Added usage comments

## API Reference

### Method Signatures

```csharp
// New overloads
public static IResourceBuilder<DBMigratorResource> AddDBMigrator<TProject>(
    this IDistributedApplicationBuilder builder,
    string name,
    string relativeMigrationsPath = "Migrations")
    where TProject : IProjectMetadata, new()

public static IResourceBuilder<DBMigratorResource> AddDBMigrator<TProject, TDatabase>(
    this IDistributedApplicationBuilder builder,
    string name,
    IResourceBuilder<TDatabase> database,
    string relativeMigrationsPath = "Migrations")
    where TProject : IProjectMetadata, new()
    where TDatabase : IResourceWithConnectionString

// Existing overloads (unchanged)
public static IResourceBuilder<DBMigratorResource> AddDBMigrator(
    this IDistributedApplicationBuilder builder,
    string name,
    string migrationsPath)

public static IResourceBuilder<DBMigratorResource> AddDBMigrator<T>(
    this IDistributedApplicationBuilder builder,
    string name,
    IResourceBuilder<T> database,
    string migrationsPath)
    where T : IResourceWithConnectionString
```

## Implementation Details

- Uses `IProjectMetadata` interface (same as Aspire's AddProject)
- Resolves project directory via `Path.GetDirectoryName(projectMetadata.ProjectPath)`
- Combines project directory with relative migrations path using `Path.Combine`
- Stores project path in annotation for potential future use
- All path operations use System.IO.Path for cross-platform compatibility

## Requirements

- .NET 10.0+ / Aspire 13.0+ (for project metadata generation)
- Existing path-based overloads work on .NET 9.0+ / Aspire 9.0+

## Commit

- **Hash**: 96d7355
- **Message**: "feat: Add project reference support to AddDBMigrator"
- **Branch**: upgrade-aspire-to-net10
