# DBMigrator Aspire Integration Architecture

## Overview

The DBMigrator Aspire integration allows database migrations to run automatically as part of the .NET Aspire application lifecycle. This document explains how the integration works and its design decisions.

## Components

### 1. DBMigratorResource

The `DBMigratorResource` represents a migration task in the Aspire application model. It implements:
- `IResource` - Makes it a first-class Aspire resource
- `IResourceWithConnectionString` - Allows connection string management

**Key Properties:**
- `Name` - The unique resource identifier
- `MigrationsPath` - Path to the migration scripts folder

### 2. Resource Builder Extensions

The `DBMigratorResourceBuilderExtensions` class provides fluent API methods:

```csharp
// Basic creation
AddDBMigrator(name, migrationsPath)

// With SQL Server reference
AddDBMigrator(name, sqlServerDatabase, migrationsPath)
AddDBMigrator(name, sqlServerServer, databaseName, migrationsPath)

// Configuration methods
WithTargetVersion(version)
WithSkipValidation(skipValidation)
WithDryRun(dryRun)

// Dependency management
WaitForDBMigrator(dbMigrator)
```

### 3. Annotations

Annotations store configuration for the migrator:

- `DBMigratorMigrationsPathAnnotation` - Path to migrations
- `DBMigratorTargetVersionAnnotation` - Target migration version
- `DBMigratorSkipValidationAnnotation` - Skip validation flag
- `DBMigratorDryRunAnnotation` - Dry run mode flag

### 4. Lifecycle Hook

The `DBMigratorLifecycleHook` implements `IDistributedApplicationLifecycleHook` to:
- Execute migrations before the application starts
- Wait for SQL Server to be ready
- Parse configuration from annotations
- Run the migration process
- Report success or failure

## Execution Flow

```
1. Application starts
   ?
2. Aspire initializes resources
   ?
3. SQL Server container starts
   ?
4. DBMigratorLifecycleHook.BeforeStartAsync() triggered
   ?
5. Wait for SQL Server to be ready
   ?
6. Load migration configuration from annotations
   ?
7. Connect to database
   ?
8. Read migration scripts from folder
   ?
9. Calculate diff (pending migrations)
   ?
10. Validate database (if not skipped)
    ?
11. Execute migrations
    ?
12. Log results
    ?
13. Application resources start
```

## Design Decisions

### Why a Lifecycle Hook?

We use a lifecycle hook (`IDistributedApplicationLifecycleHook`) because:
- Migrations must run **before** application services start
- We need access to the full application model
- It integrates naturally with Aspire's startup sequence
- Errors can properly prevent application startup

### Why Not an Executable Resource?

An executable resource (like running migrations via a container) was considered but rejected because:
- DBMigrator is a .NET library, not a standalone executable
- We'd need to package it as a container image
- Configuration would be more complex (environment variables vs. code)
- Less type-safe and harder to debug

### Connection String Management

The integration resolves connection strings by:
1. Looking for referenced SQL Server database resources
2. Waiting for the database to be ready
3. Using Aspire's built-in `GetConnectionStringAsync()` method
4. Parsing the connection string to extract server/database/credentials

### Dependency Management

The `WaitForDBMigrator` extension allows other resources to depend on migrations:

```csharp
builder.AddProject<Projects.Api>("api")
    .WaitForDBMigrator(dbMigrator);
```

This ensures the API doesn't start until migrations complete.

## Error Handling

### Migration Failures

If a migration fails:
1. The exception is logged with full details
2. The lifecycle hook throws the exception
3. Aspire stops application startup
4. The error appears in the Aspire dashboard

### Connection Issues

If the database isn't ready:
1. The hook waits using `ResourceNotificationService`
2. Timeout is controlled by Aspire's configuration
3. Clear error messages indicate the problem

### Configuration Errors

Invalid configuration (e.g., missing migrations folder):
- Detected early during hook execution
- Clear exception messages
- Application fails fast

## Testing Strategy

### Unit Tests

Test the resource builder extensions:
- Resource creation
- Annotation application
- Fluent API chaining

### Integration Tests

Use `Aspire.Hosting.Testing` to test:
- Full application startup with migrations
- Multiple database scenarios
- Error conditions

### Example Test

```csharp
[TestMethod]
public async Task DBMigrator_RunsMigrationsSuccessfully()
{
    var appHost = await DistributedApplicationTestingBuilder
        .CreateAsync<Projects.TestAppHost>();
    
    await using var app = await appHost.BuildAsync();
    await app.StartAsync();
    
    // Verify migrations ran
    var migrator = app.Services.GetRequiredService<DBMigratorResource>();
    Assert.IsTrue(migrator.IsHealthy);
}
```

## Future Enhancements

### Potential Improvements

1. **Health Checks** - Report migration status as a health check endpoint
2. **Dashboard Integration** - Show migration details in Aspire dashboard
3. **Rollback Support** - Add downgrade operations
4. **Parallel Migrations** - Support multiple databases in parallel
5. **Migration History UI** - Visual representation of applied migrations
6. **Azure SQL Support** - Enhanced support for Azure SQL authentication

### Extension Points

The design allows for:
- Custom middleware hooks
- Alternative migration strategies
- Different database providers (with code changes)
- Plugin-based validation rules

## Performance Considerations

### Startup Time

Migrations run synchronously during startup:
- Small overhead for script loading
- Database validation adds ~1-2 seconds
- Actual migration time depends on scripts
- Consider using `WithSkipValidation()` in dev environments

### Resource Usage

The lifecycle hook:
- Runs in the AppHost process
- Doesn't require additional containers
- Minimal memory footprint
- No persistent background processes

## Security Considerations

### Connection Strings

- Managed by Aspire's secret management
- Not logged in plain text
- Follow Aspire's security model

### Migration Scripts

- Loaded from local filesystem only
- No remote script execution
- Scripts should be in source control
- Review scripts before deployment

## Troubleshooting

### Common Issues

**"Migrations directory not found"**
- Check the path is relative to AppHost project
- Ensure migrations are included in project output

**"No SQL Server reference found"**
- Add `.WithReference(sqlServer)` to migrator
- Ensure SQL Server resource exists

**"Unable to retrieve connection string"**
- SQL Server might not be ready
- Check container logs in dashboard

### Debug Mode

Enable detailed logging:

```json
{
  "Logging": {
    "LogLevel": {
      "DBMigrator": "Debug",
      "DBMigrator.Aspire.Hosting": "Debug"
    }
  }
}
```

## References

- [.NET Aspire Documentation](https://learn.microsoft.com/dotnet/aspire/)
- [Aspire Resource Model](https://learn.microsoft.com/dotnet/aspire/fundamentals/app-host-overview)
- [Lifecycle Hooks](https://learn.microsoft.com/dotnet/aspire/fundamentals/app-host-overview#lifecycle-hooks)
- [DBMigrator Main Documentation](../README.md)
