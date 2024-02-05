using DBMigrator.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DBMigrator.Test
{
    [TestClass]
    public class DiffTest
    {

        [TestMethod]
        public void Diff_Should_Return_Empty_List_When_Source_And_Target_Are_Identical()
        {
            // Arrange
            var versionDiff = new VersionDiff();
            var source = new List<DBVersion>();
            var target = new List<DBVersion>();

            // Act
            var result = versionDiff.Diff(source, target);

            // Assert
            Assert.IsTrue(!result.Any());
        }

        [TestMethod]
        public void Diff_Should_Return_Difference_Between_Source_And_Target()
        {
            // Arrange
            var versionDiff = new VersionDiff();

            var version = new DBVersion("1.0.0");
            var feature = version.AddAndOrGetFeature("TestFeature", 0);
            var script = feature.AddUpgradeScript("01_Test.sql", 1);
            script.Checksum = "A";
            script.SQL = "CREATE TABLE Users (Id INT, Name VARCHAR(50))";
            var source = new List<DBVersion> { version };

            var target = new List<DBVersion>();

            // Act
            var result = versionDiff.Diff(source, target);

            // Assert
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("1.0.0", result.First().Name);
            Assert.AreEqual(1, result.First().Features.Count());
            Assert.AreEqual("TestFeature", result.First().Features.First().Name);
            Assert.AreEqual(1, result.First().Features.First().UpgradeScripts.Count());
            Assert.AreEqual("01_Test.sql", result.First().Features.First().UpgradeScripts.First().FileName);
            Assert.AreEqual("CREATE TABLE Users (Id INT, Name VARCHAR(50))", result.First().Features.First().UpgradeScripts.First().SQL);
        }

        [TestMethod]
        public void Diff_Should_Use_Proper_Order_If_Target_Order_Is_Zero_With_Incompletely_Migrated_Features()
        {
            // Arrange
            var versionDiff = new VersionDiff();

            var versionSource = new DBVersion("1.0.0");
            var featureSource = versionSource.AddAndOrGetFeature("TestFeature", 1);
            var scriptSource1 = featureSource.AddUpgradeScript("01_Test1.sql", 1);
            var scriptSource2 = featureSource.AddUpgradeScript("02_Test2.sql", 2);
            scriptSource1.Checksum = "A";
            scriptSource1.SQL = "CREATE TABLE Users (Id INT, Name VARCHAR(50))";
            scriptSource2.Checksum = "B";
            scriptSource2.SQL = "CREATE TABLE Books (Id INT, Name VARCHAR(50))";
            var source = new List<DBVersion> { versionSource };

            var versionTarget = new DBVersion("1.0.0");
            var featureTarget = versionTarget.AddAndOrGetFeature("TestFeature", 0);
            var scriptTarget = featureTarget.AddUpgradeScript("01_Test1.sql", 1);
            scriptTarget.Checksum = "A";
            scriptTarget.SQL = "CREATE TABLE Users (Id INT, Name VARCHAR(50))";
            var target = new List<DBVersion> { versionTarget };

            // Act
            var result = versionDiff.Diff(source, target);

            // Assert
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("1.0.0", result.First().Name);
            Assert.AreEqual(1, result.First().Features.Count());
            Assert.AreEqual("TestFeature", result.First().Features.First().Name);
            Assert.AreEqual(1, result.First().Features.First().Order);
            Assert.AreEqual(1, result.First().Features.First().UpgradeScripts.Count());
            Assert.AreEqual("02_Test2.sql", result.First().Features.First().UpgradeScripts.First().FileName);
            Assert.AreEqual("CREATE TABLE Books (Id INT, Name VARCHAR(50))", result.First().Features.First().UpgradeScripts.First().SQL);
        }
    }
}
