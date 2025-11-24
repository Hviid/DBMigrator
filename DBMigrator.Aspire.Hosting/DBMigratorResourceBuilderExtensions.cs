#nullable enable
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

namespace DBMigrator.Aspire.Hosting;

/// <summary>
/// Provides extension methods for adding DBMigrator to an Aspire application.
/// </summary>
public static class DBMigratorResourceBuilderExtensions
{
    /// <summary>
    /// Adds a DBMigrator resource to the application with the specified migrations path.
    /// </summary>
    /// <param name="builder">The distributed application builder.</param>
    /// <param name="name">The name of the resource.</param>
    /// <param name="migrationsPath">The path to the migrations folder.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<DBMigratorResource> AddDBMigrator(
        this IDistributedApplicationBuilder builder,
        string name,
        string migrationsPath)
    {
        var resource = new DBMigratorResource(name, migrationsPath);
        
        return builder.AddResource(resource)
            .WithAnnotation(new DBMigratorMigrationsPathAnnotation(migrationsPath));
    }

    /// <summary>
    /// Adds a DBMigrator resource to the application with a reference to a database resource.
    /// </summary>
    /// <typeparam name="T">The type of the database resource.</typeparam>
    /// <param name="builder">The distributed application builder.</param>
    /// <param name="name">The name of the resource.</param>
    /// <param name="database">The database resource to migrate.</param>
    /// <param name="migrationsPath">The path to the migrations folder.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<DBMigratorResource> AddDBMigrator<T>(
        this IDistributedApplicationBuilder builder,
        string name,
        IResourceBuilder<T> database,
        string migrationsPath)
        where T : IResourceWithConnectionString
    {
        var migrator = builder.AddDBMigrator(name, migrationsPath)
            .WithAnnotation(new DBMigratorDatabaseReferenceAnnotation(database.Resource));

        return migrator;
    }

    /// <summary>
    /// Configures the target version for the DBMigrator migration.
    /// </summary>
    /// <param name="builder">The resource builder.</param>
    /// <param name="version">The target version to migrate to. If null or empty, migrates to the latest version.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<DBMigratorResource> WithTargetVersion(
        this IResourceBuilder<DBMigratorResource> builder,
        string? version = null)
    {
        return builder.WithAnnotation(new DBMigratorTargetVersionAnnotation(version));
    }

    /// <summary>
    /// Configures DBMigrator to skip database validation before migration.
    /// </summary>
    /// <param name="builder">The resource builder.</param>
    /// <param name="skipValidation">Whether to skip validation. Default is false.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<DBMigratorResource> WithSkipValidation(
        this IResourceBuilder<DBMigratorResource> builder,
        bool skipValidation = true)
    {
        return builder.WithAnnotation(new DBMigratorSkipValidationAnnotation(skipValidation));
    }

    /// <summary>
    /// Configures DBMigrator to run as a dry run (no actual database changes).
    /// </summary>
    /// <param name="builder">The resource builder.</param>
    /// <param name="dryRun">Whether to run as dry run. Default is true.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<DBMigratorResource> WithDryRun(
        this IResourceBuilder<DBMigratorResource> builder,
        bool dryRun = true)
    {
        return builder.WithAnnotation(new DBMigratorDryRunAnnotation(dryRun));
    }

    /// <summary>
    /// Adds a reference to a database that the migrator will run against.
    /// </summary>
    /// <typeparam name="T">The type of the database resource.</typeparam>
    /// <param name="builder">The resource builder.</param>
    /// <param name="database">The database resource.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<DBMigratorResource> WithDatabaseReference<T>(
        this IResourceBuilder<DBMigratorResource> builder,
        IResourceBuilder<T> database)
        where T : IResourceWithConnectionString
    {
        return builder
            .WithAnnotation(new DBMigratorDatabaseReferenceAnnotation(database.Resource));
    }

    /// <summary>
    /// Waits for the DBMigrator migration to complete before starting dependent resources.
    /// </summary>
    /// <typeparam name="T">The resource type.</typeparam>
    /// <param name="builder">The resource builder.</param>
    /// <param name="dbMigrator">The DBMigrator resource to wait for.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<T> WaitForDBMigrator<T>(
        this IResourceBuilder<T> builder,
        IResourceBuilder<DBMigratorResource> dbMigrator)
        where T : IResourceWithWaitSupport
    {
        return builder.WaitFor(dbMigrator);
    }
}
