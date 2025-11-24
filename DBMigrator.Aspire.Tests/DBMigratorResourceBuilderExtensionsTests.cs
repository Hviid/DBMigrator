using DBMigrator.Aspire.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Aspire.Hosting;

namespace DBMigrator.Aspire.Tests;

[TestClass]
public class DBMigratorResourceBuilderExtensionsTests
{
    [TestMethod]
    public void AddDBMigrator_WithMigrationsPath_CreatesResource()
    {
        var builder = DistributedApplication.CreateBuilder();
        
        var migrator = builder.AddDBMigrator("test-migrator", "./migrations");
        
        Assert.IsNotNull(migrator);
        Assert.AreEqual("test-migrator", migrator.Resource.Name);
        Assert.AreEqual("./migrations", migrator.Resource.MigrationsPath);
    }

    [TestMethod]
    public void AddDBMigrator_WithDatabase_CreatesResourceWithReference()
    {
        var builder = DistributedApplication.CreateBuilder();
        var sqlServer = builder.AddSqlServer("sql");
        var database = sqlServer.AddDatabase("testdb");
        
        var migrator = builder.AddDBMigrator("test-migrator", database, "./migrations");
        
        Assert.IsNotNull(migrator);
        Assert.AreEqual("test-migrator", migrator.Resource.Name);
        Assert.AreEqual("./migrations", migrator.Resource.MigrationsPath);
    }

    [TestMethod]
    public void WithTargetVersion_AddsAnnotation()
    {
        var builder = DistributedApplication.CreateBuilder();
        
        var migrator = builder.AddDBMigrator("test-migrator", "./migrations")
            .WithTargetVersion("1.0");
        
        var annotation = migrator.Resource.Annotations
            .OfType<DBMigratorTargetVersionAnnotation>()
            .FirstOrDefault();
        
        Assert.IsNotNull(annotation);
        Assert.AreEqual("1.0", annotation.TargetVersion);
    }

    [TestMethod]
    public void WithSkipValidation_AddsAnnotation()
    {
        var builder = DistributedApplication.CreateBuilder();
        
        var migrator = builder.AddDBMigrator("test-migrator", "./migrations")
            .WithSkipValidation();
        
        var annotation = migrator.Resource.Annotations
            .OfType<DBMigratorSkipValidationAnnotation>()
            .FirstOrDefault();
        
        Assert.IsNotNull(annotation);
        Assert.IsTrue(annotation.SkipValidation);
    }

    [TestMethod]
    public void WithDryRun_AddsAnnotation()
    {
        var builder = DistributedApplication.CreateBuilder();
        
        var migrator = builder.AddDBMigrator("test-migrator", "./migrations")
            .WithDryRun();
        
        var annotation = migrator.Resource.Annotations
            .OfType<DBMigratorDryRunAnnotation>()
            .FirstOrDefault();
        
        Assert.IsNotNull(annotation);
        Assert.IsTrue(annotation.DryRun);
    }

    [TestMethod]
    public void WaitForDBMigrator_ConfiguresDependency()
    {
        var builder = DistributedApplication.CreateBuilder();
        var sqlServer = builder.AddSqlServer("sql");
        var database = sqlServer.AddDatabase("testdb");
        var migrator = builder.AddDBMigrator("test-migrator", database, "./migrations");
        
        // This would typically be a project or container
        var dependentResource = builder.AddContainer("dependent", "alpine")
            .WaitForDBMigrator(migrator);
        
        Assert.IsNotNull(dependentResource);
    }

    [TestMethod]
    public void MultipleConfigurations_CanBeChained()
    {
        var builder = DistributedApplication.CreateBuilder();
        var sqlServer = builder.AddSqlServer("sql");
        var database = sqlServer.AddDatabase("testdb");
        
        var migrator = builder.AddDBMigrator("test-migrator", database, "./migrations")
            .WithTargetVersion("2.0")
            .WithSkipValidation()
            .WithDryRun();
        
        Assert.IsNotNull(migrator);
        Assert.AreEqual("test-migrator", migrator.Resource.Name);
        
        var versionAnnotation = migrator.Resource.Annotations.OfType<DBMigratorTargetVersionAnnotation>().FirstOrDefault();
        var skipValidationAnnotation = migrator.Resource.Annotations.OfType<DBMigratorSkipValidationAnnotation>().FirstOrDefault();
        var dryRunAnnotation = migrator.Resource.Annotations.OfType<DBMigratorDryRunAnnotation>().FirstOrDefault();
        
        Assert.IsNotNull(versionAnnotation);
        Assert.AreEqual("2.0", versionAnnotation.TargetVersion);
        Assert.IsNotNull(skipValidationAnnotation);
        Assert.IsTrue(skipValidationAnnotation.SkipValidation);
        Assert.IsNotNull(dryRunAnnotation);
        Assert.IsTrue(dryRunAnnotation.DryRun);
    }
}
