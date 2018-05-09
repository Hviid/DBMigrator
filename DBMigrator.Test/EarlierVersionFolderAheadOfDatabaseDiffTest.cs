using DBMigrator.Model;
using DBMigrator.Test.Comparers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DBMigrator.Test
{
    [TestClass]
    public class EarlierVersionFolderAheadOfDatabaseDiffTest
    {
        private List<DBVersion> Folder()
        {
            var version = new DBVersion("1.0.0");
            var feature = version.AddAndOrGetFeature("TestFeature", 0);

            feature.AddUpgradeScript("TestScript.sql", 1);

            var version2 = new DBVersion("2.0.0");
            var feature2 = version2.AddAndOrGetFeature("TestFeature2", 0);

            feature2.AddUpgradeScript("TestScript2.sql", 1);

            return new List<DBVersion> { version, version2 };
        }

        private List<DBVersion> Database()
        {
            var version = new DBVersion("1.0.0");
            var feature = version.AddAndOrGetFeature("TestFeature", 0);

            feature.AddUpgradeScript("TestScript.sql", 1);

            return new List<DBVersion> { version };
        }

        private List<DBVersion> ExpectedDiff()
        {
            var version2 = new DBVersion("2.0.0");
            var feature2 = version2.AddAndOrGetFeature("TestFeature2", 0);

            feature2.AddUpgradeScript("TestScript2.sql", 1);

            return new List<DBVersion> { version2 };
        }


        [TestMethod]
        public void Diff_found_test()
        {
            var differ = new VersionDiff();

            var diff = differ.Diff(Folder(), Database());

            Assert.AreEqual(1, diff.Count);
            Assert.AreEqual(ExpectedDiff().First().Name, diff.First().Name);

            Assert.AreEqual(1, diff.First().Features.Count());
            Assert.AreEqual(ExpectedDiff().First().Features.First().Name, diff.First().Features.First().Name);

            Assert.AreEqual(1, diff.First().Features.First().UpgradeScripts.Count);
            Assert.AreEqual(ExpectedDiff().First().Features.First().Name, diff.First().Features.First().Name);
        }
    }
}
