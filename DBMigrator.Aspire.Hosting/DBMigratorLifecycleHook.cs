#nullable enable
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Lifecycle;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DBMigrator.Aspire.Hosting;

/// <summary>
/// A lifecycle hook that executes DBMigrator migrations before the application starts.
/// </summary>
#pragma warning disable CS0618 // Type or member is obsolete - will be updated in future version
internal class DBMigratorLifecycleHook(
    ILogger<DBMigratorLifecycleHook> logger,
    ResourceNotificationService notificationService) : IDistributedApplicationLifecycleHook
#pragma warning restore CS0618
{
    public async Task BeforeStartAsync(DistributedApplicationModel appModel, CancellationToken cancellationToken = default)
    {
        var migratorResources = appModel.Resources.OfType<DBMigratorResource>();

        foreach (var resource in migratorResources)
        {
            logger.LogInformation("Running DBMigrator for resource: {ResourceName}", resource.Name);

            try
            {
                await ExecuteMigrationAsync(resource, cancellationToken);
                logger.LogInformation("DBMigrator completed successfully for resource: {ResourceName}", resource.Name);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "DBMigrator failed for resource: {ResourceName}", resource.Name);
                throw;
            }
        }
    }

    private async Task ExecuteMigrationAsync(DBMigratorResource resource, CancellationToken cancellationToken)
    {
        var migrationsPath = resource.Annotations.OfType<DBMigratorMigrationsPathAnnotation>().FirstOrDefault()?.MigrationsPath 
            ?? resource.MigrationsPath;
        var targetVersion = resource.Annotations.OfType<DBMigratorTargetVersionAnnotation>().FirstOrDefault()?.TargetVersion;
        var skipValidation = resource.Annotations.OfType<DBMigratorSkipValidationAnnotation>().FirstOrDefault()?.SkipValidation ?? false;
        var dryRun = resource.Annotations.OfType<DBMigratorDryRunAnnotation>().FirstOrDefault()?.DryRun ?? false;

        if (!Directory.Exists(migrationsPath))
        {
            throw new DirectoryNotFoundException($"Migrations directory not found: {migrationsPath}");
        }

        // Get the database reference from our custom annotation
        var databaseReference = resource.Annotations.OfType<DBMigratorDatabaseReferenceAnnotation>().FirstOrDefault();
        if (databaseReference == null)
        {
            throw new InvalidOperationException(
                $"No database reference found for DBMigrator resource: {resource.Name}. " +
                "Use .WithReference(database) to add a database reference.");
        }

        var databaseResource = databaseReference.DatabaseResource;
        var referencedResourceName = databaseResource.Name;

        // Wait for the database to be ready
        logger.LogInformation("Waiting for database {DatabaseName} to be ready", referencedResourceName);
        await notificationService.WaitForResourceAsync(referencedResourceName, KnownResourceStates.Running, cancellationToken);

        // Get connection string from the database resource
        var connectionString = await databaseResource.ConnectionStringExpression.GetValueAsync(cancellationToken)
            ?? throw new InvalidOperationException($"Unable to retrieve connection string for database: {referencedResourceName}");

        // Parse connection string to get server and database details
        var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connectionString);
        var serverName = builder.DataSource;
        var databaseName = builder.InitialCatalog;
        var username = builder.UserID;
        var password = builder.Password;

        logger.LogInformation("Connecting to server: {Server}, database: {Database}", serverName, databaseName);

        await Task.Run(() =>
        {
            using var database = new Database(serverName, databaseName, username, password, "");
            var dbFolder = new DBFolder(new DirectoryInfo(migrationsPath));
            var dbVersions = database.GetDBState();
            var differ = new VersionDiff();
            var middleware = new Middleware.Middleware();
            middleware.RegisterMiddleware(new Middleware.PrePostMigrationScripts(new DirectoryInfo(migrationsPath)));

            logger.LogInformation("Calculating migration diff for version: {Version}", targetVersion ?? "latest");
            var diff = differ.Diff(dbFolder.GetVersions(targetVersion), dbVersions).ToList();

            if (diff.Count > 0)
            {
                var diffText = differ.UpgradeDiffText(diff);
                logger.LogInformation("Migrations to apply:\n{DiffText}", diffText);

                if (!skipValidation)
                {
                    logger.LogInformation("Validating database");
                    var validator = new VersionValidator();
                    var dbValidator = new DatabaseValidator(database);
                    validator.ValidateVersions(dbFolder.allVersions, dbVersions);
                    dbValidator.Validate();
                    logger.LogInformation("Database validation passed");
                }

                using var migrator = new Migrator(database, dbFolder, middleware);
                migrator.Upgrade(diff, dryRun);

                if (dryRun)
                {
                    logger.LogWarning("DRY RUN: Migrations were not committed to the database");
                }
            }
            else
            {
                logger.LogInformation("Database is already up to date");
            }
        }, cancellationToken);
    }
}
