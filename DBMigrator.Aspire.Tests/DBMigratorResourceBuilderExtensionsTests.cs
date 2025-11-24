using System.Linq;
using System.IO;
using DBMigrator.Aspire.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

namespace DBMigrator.Aspire.Tests;

// Mock project metadata for testing
public class TestMigrationsProject : IProjectMetadata
{
    public string ProjectPath => Path.Combine(Directory.GetCurrentDirectory(), "TestProject", "TestProject.csproj");
}

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
    public void AddDBMigrator_WithProjectReference_ResolvesPath()
    {
        var builder = DistributedApplication.CreateBuilder();
        
        var migrator = builder.AddDBMigrator<TestMigrationsProject>("test-migrator");
        
        Assert.IsNotNull(migrator);
        Assert.AreEqual("test-migrator", migrator.Resource.Name);
        
        // Verify the path was resolved from the project directory
        var expectedPath = Path.Combine(Directory.GetCurrentDirectory(), "TestProject", "Migrations");
        Assert.AreEqual(expectedPath, migrator.Resource.MigrationsPath);
        
        // Verify project reference annotation was added
        var projectAnnotation = migrator.Resource.Annotations
            .OfType<DBMigratorProjectReferenceAnnotation>()
            .FirstOrDefault();
        Assert.IsNotNull(projectAnnotation);
    }

    [TestMethod]
    public void AddDBMigrator_WithProjectReferenceAndDatabase_CreatesResourceWithBoth()
    {
        var builder = DistributedApplication.CreateBuilder();
        var sqlServer = builder.AddSqlServer("sql");
        var database = sqlServer.AddDatabase("testdb");
        
        var migrator = builder.AddDBMigrator<TestMigrationsProject, SqlServerDatabaseResource>(
            "test-migrator", 
            database);
        
        Assert.IsNotNull(migrator);
        Assert.AreEqual("test-migrator", migrator.Resource.Name);
        
        // Verify database reference
        var dbAnnotation = migrator.Resource.Annotations
            .OfType<DBMigratorDatabaseReferenceAnnotation>()
            .FirstOrDefault();
        Assert.IsNotNull(dbAnnotation);
        
        // Verify project reference
        var projectAnnotation = migrator.Resource.Annotations
            .OfType<DBMigratorProjectReferenceAnnotation>()
            .FirstOrDefault();
        Assert.IsNotNull(projectAnnotation);
    }

    [TestMethod]
    public void AddDBMigrator_WithProjectReferenceAndCustomPath_UsesCustomPath()
    {
        var builder = DistributedApplication.CreateBuilder();
        
        var migrator = builder.AddDBMigrator<TestMigrationsProject>(
            "test-migrator",
            relativeMigrationsPath: "Database/Scripts");
        
        Assert.IsNotNull(migrator);
        
        // Verify custom relative path was used (normalize path separators for comparison)
        var expectedPath = Path.Combine(Directory.GetCurrentDirectory(), "TestProject", "Database", "Scripts");
        var actualPath = Path.GetFullPath(migrator.Resource.MigrationsPath);
        var normalizedExpected = Path.GetFullPath(expectedPath);
        
        Assert.AreEqual(normalizedExpected, actualPath);
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

    [TestMethod]
    public void ProjectReferenceWithChainedConfiguration_WorksCorrectly()
    {
        var builder = DistributedApplication.CreateBuilder();
        var sqlServer = builder.AddSqlServer("sql");
        var database = sqlServer.AddDatabase("testdb");
        
        var migrator = builder.AddDBMigrator<TestMigrationsProject, SqlServerDatabaseResource>(
            "test-migrator", 
            database,
            relativeMigrationsPath: "CustomMigrations")
            .WithTargetVersion("3.0")
            .WithSkipValidation()
            .WithDryRun();
        
        Assert.IsNotNull(migrator);
        
        // Verify all annotations are present
        Assert.IsTrue(migrator.Resource.Annotations.OfType<DBMigratorProjectReferenceAnnotation>().Any());
        Assert.IsTrue(migrator.Resource.Annotations.OfType<DBMigratorDatabaseReferenceAnnotation>().Any());
        Assert.IsTrue(migrator.Resource.Annotations.OfType<DBMigratorTargetVersionAnnotation>().Any());
        Assert.IsTrue(migrator.Resource.Annotations.OfType<DBMigratorSkipValidationAnnotation>().Any());
        Assert.IsTrue(migrator.Resource.Annotations.OfType<DBMigratorDryRunAnnotation>().Any());
    }
}
