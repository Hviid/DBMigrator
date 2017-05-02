using DBMigrator.Model;
using DBMigrator.Test.Comparers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;

namespace DBMigrator.Test
{
    [TestClass]
    public class DatabaseAheadOfFolderDiffTest
    {
        private List<DBVersion> Database()
        {
            var version = new DBVersion("1.0.0");
            var feature = version.AddAndOrGetFeature("TestFeature");
            
            feature.AddUpgradeScript("TestScript.sql", 1);

            var version2 = new DBVersion("2.0.0");
            var feature2 = version2.AddAndOrGetFeature("TestFeature2");
            
            feature2.AddUpgradeScript("TestScript2.sql", 1);

            return new List<DBVersion> { version, version2 };
        }

        private List<DBVersion> Folder()
        {
            var version = new DBVersion("1.0.0");
            var feature = version.AddAndOrGetFeature("TestFeature");
            
            feature.AddUpgradeScript("TestScript.sql", 1);

            return new List<DBVersion> { version };
        }


        [TestMethod]
        public void No_diff_test()
        {
            var differ = new VersionDiff();

            var diff = differ.Diff(Folder(), Database());

            Assert.AreEqual(0, diff.Count);
        }
    }
}
