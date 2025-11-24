using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using DBMigrator.Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// Add SQL Server
var sqlServer = builder.AddSqlServer("sql")
    .WithLifetime(ContainerLifetime.Persistent);

var database = sqlServer.AddDatabase("exampledb");

// ========================================
// Option 1: Using explicit path (current)
// ========================================
var dbMigrator = builder.AddDBMigrator("dbmigrator", database, "./Migrations")
    .WithTargetVersion("1.0"); // Optional: specify target version

// ========================================
// Option 2: Using project reference (new in .NET 10/Aspire 13)
// ========================================
// If you have a separate migrations project:
// var dbMigrator = builder.AddDBMigrator<Projects.MyMigrationsProject>("dbmigrator", database)
//     .WithTargetVersion("1.0");

// Or with custom relative path:
// var dbMigrator = builder.AddDBMigrator<Projects.MyMigrationsProject>(
//     "dbmigrator", 
//     database, 
//     relativeMigrationsPath: "Database/Scripts")
//     .WithTargetVersion("1.0");

// Register the lifecycle hook to execute migrations before app starts
builder.AddDBMigratorLifecycleHook();

// Example: Add an API that depends on the database being migrated
// var api = builder.AddProject<Projects.MyApi>("api")
//     .WaitForDBMigrator(dbMigrator);

builder.Build().Run();
