using DBMigrator.Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// Add SQL Server
var sqlServer = builder.AddSqlServer("sql")
    .WithLifetime(ContainerLifetime.Persistent);

var database = sqlServer.AddDatabase("exampledb");

// Add DBMigrator - it will run migrations from the specified folder
var dbMigrator = builder.AddDBMigrator("dbmigrator", database, "./Migrations")
    .WithTargetVersion("1.0"); // Optional: specify target version

// Register the lifecycle hook to execute migrations before app starts
builder.AddDBMigratorLifecycleHook();

// Example: Add an API that depends on the database being migrated
// var api = builder.AddProject<Projects.MyApi>("api")
//     .WaitForDBMigrator(dbMigrator);

builder.Build().Run();
