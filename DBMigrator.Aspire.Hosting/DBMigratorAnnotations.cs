#nullable enable
using Aspire.Hosting.ApplicationModel;

namespace DBMigrator.Aspire.Hosting;

/// <summary>
/// Annotation for storing the migrations path.
/// </summary>
/// <param name="migrationsPath">The path to the migrations folder.</param>
public class DBMigratorMigrationsPathAnnotation(string migrationsPath) : IResourceAnnotation
{
    /// <summary>
    /// Gets the path to the migrations folder.
    /// </summary>
    public string MigrationsPath { get; } = migrationsPath;
}

/// <summary>
/// Annotation for storing the target version.
/// </summary>
/// <param name="targetVersion">The target version to migrate to.</param>
public class DBMigratorTargetVersionAnnotation(string? targetVersion) : IResourceAnnotation
{
    /// <summary>
    /// Gets the target version to migrate to.
    /// </summary>
    public string? TargetVersion { get; } = targetVersion;
}

/// <summary>
/// Annotation for storing the skip validation flag.
/// </summary>
/// <param name="skipValidation">Whether to skip validation.</param>
public class DBMigratorSkipValidationAnnotation(bool skipValidation) : IResourceAnnotation
{
    /// <summary>
    /// Gets a value indicating whether to skip validation.
    /// </summary>
    public bool SkipValidation { get; } = skipValidation;
}

/// <summary>
/// Annotation for storing the dry run flag.
/// </summary>
/// <param name="dryRun">Whether to run as dry run.</param>
public class DBMigratorDryRunAnnotation(bool dryRun) : IResourceAnnotation
{
    /// <summary>
    /// Gets a value indicating whether to run as dry run.
    /// </summary>
    public bool DryRun { get; } = dryRun;
}

/// <summary>
/// Annotation for storing a reference to the database resource.
/// </summary>
/// <param name="databaseResource">The database resource.</param>
public class DBMigratorDatabaseReferenceAnnotation(IResourceWithConnectionString databaseResource) : IResourceAnnotation
{
    /// <summary>
    /// Gets the database resource.
    /// </summary>
    public IResourceWithConnectionString DatabaseResource { get; } = databaseResource;
}

/// <summary>
/// Annotation for storing the project path reference.
/// </summary>
/// <param name="projectPath">The path to the project file.</param>
public class DBMigratorProjectReferenceAnnotation(string projectPath) : IResourceAnnotation
{
    /// <summary>
    /// Gets the path to the project file.
    /// </summary>
    public string ProjectPath { get; } = projectPath;
}
