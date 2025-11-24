using Aspire.Hosting.ApplicationModel;

namespace DBMigrator.Aspire.Hosting;

/// <summary>
/// Represents a DBMigrator resource that can be used to run database migrations.
/// </summary>
/// <param name="name">The name of the resource.</param>
/// <param name="migrationsPath">The path to the migrations folder.</param>
public class DBMigratorResource(string name, string migrationsPath) : Resource(name), IResourceWithConnectionString
{
    /// <summary>
    /// Gets the path to the migrations folder.
    /// </summary>
    public string MigrationsPath { get; } = migrationsPath;

    /// <summary>
    /// Gets the connection string expression for the resource.
    /// </summary>
    public ReferenceExpression ConnectionStringExpression => 
        ReferenceExpression.Create($"{this}");
}
