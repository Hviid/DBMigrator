using Aspire.Hosting;
using Aspire.Hosting.Lifecycle;
using Microsoft.Extensions.DependencyInjection;

namespace DBMigrator.Aspire.Hosting;

/// <summary>
/// Provides extension methods for configuring DBMigrator in an Aspire application.
/// </summary>
public static class DBMigratorHostingExtensions
{
    /// <summary>
    /// Registers the DBMigrator lifecycle hook with the application.
    /// </summary>
    /// <param name="builder">The distributed application builder.</param>
    /// <returns>The distributed application builder.</returns>
    public static IDistributedApplicationBuilder AddDBMigratorLifecycleHook(
        this IDistributedApplicationBuilder builder)
    {
#pragma warning disable CS0618 // Type or member is obsolete - will be updated in future version
        builder.Services.AddLifecycleHook<DBMigratorLifecycleHook>();
#pragma warning restore CS0618
        return builder;
    }
}

