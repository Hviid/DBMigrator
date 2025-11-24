# DBMigrator Aspire Integration Example

This example demonstrates how to use DBMigrator with .NET Aspire for automated database migrations.

## Project Structure

```
DBMigrator.Aspire.Example/
??? Program.cs                          # Aspire AppHost configuration
??? appsettings.json                    # Logging configuration
??? Migrations/                         # Database migration scripts
    ??? 1.0/
        ??? UserManagement/
            ??? Migrations/
                ??? 1_CreateUsersTable.sql
                ??? 1_rollback_CreateUsersTable.sql
                ??? 2_AddEmailIndex.sql
```

## Running the Example

1. Make sure you have .NET 9.0 SDK installed
2. Install .NET Aspire workload:
   ```bash
   dotnet workload install aspire
   ```

3. Navigate to the example directory:
   ```bash
   cd Examples/DBMigrator.Aspire.Example
   ```

4. Run the application:
   ```bash
   dotnet run
   ```

The Aspire dashboard will open in your browser, and you'll see:
- A SQL Server container being started
- DBMigrator running migrations before other resources start
- Migration logs in the dashboard

## What Happens

1. **SQL Server Container Starts**: Aspire starts a SQL Server container
2. **Database Creation**: The `exampledb` database is created
3. **Migration Execution**: DBMigrator lifecycle hook runs:
   - Connects to the database
   - Reads migration scripts from `./Migrations`
   - Validates the database state
   - Applies pending migrations (version 1.0)
   - Creates the `Users` table
   - Adds an index on the `Email` column
4. **Application Ready**: Other resources can now start

## Key Features Demonstrated

- ? SQL Server integration with Aspire
- ? Automatic migration execution on startup
- ? Structured migration folder organization
- ? Version-based migrations (1.0)
- ? Feature-based organization (UserManagement)
- ? Rollback script support
- ? Comprehensive logging

## Customization

### Change Target Version

To migrate to a different version, modify `Program.cs`:

```csharp
var dbMigrator = builder.AddDBMigrator("dbmigrator", database, "./Migrations")
    .WithTargetVersion("2.0"); // Change to your target version
```

### Skip Validation

For development scenarios where you want to skip validation:

```csharp
var dbMigrator = builder.AddDBMigrator("dbmigrator", database, "./Migrations")
    .WithSkipValidation();
```

### Dry Run Mode

Test migrations without committing:

```csharp
var dbMigrator = builder.AddDBMigrator("dbmigrator", database, "./Migrations")
    .WithDryRun();
```

### Add Your API

Integrate with your own API project:

```csharp
var api = builder.AddProject<Projects.MyApi>("api")
    .WithReference(database)
    .WaitForDBMigrator(dbMigrator);
```

This ensures your API only starts after migrations complete.

## Adding New Migrations

1. Create a new version folder (e.g., `2.0`)
2. Create a feature folder inside it (e.g., `ProductCatalog`)
3. Create a `Migrations` subfolder
4. Add your SQL scripts following the naming convention:
   - Upgrade: `<order>_<description>.sql`
   - Rollback: `<order>_rollback_<description>.sql`

Example:
```
Migrations/
??? 2.0/
    ??? ProductCatalog/
        ??? Migrations/
            ??? 1_CreateProductsTable.sql
            ??? 1_rollback_CreateProductsTable.sql
```

## Troubleshooting

### Migrations Not Running

Check the Aspire dashboard logs for the DBMigrator resource to see any errors.

### Connection Issues

Ensure the SQL Server container is healthy before migrations run. The lifecycle hook waits for the container to be ready.

### Path Issues

The migrations path is relative to the AppHost project directory. Make sure the path in `AddDBMigrator` correctly points to your migrations folder.

## Learn More

- [DBMigrator Documentation](../../README.md)
- [.NET Aspire Documentation](https://learn.microsoft.com/dotnet/aspire/)
- [DBMigrator.Aspire.Hosting Package](../../DBMigrator.Aspire.Hosting/README.md)
