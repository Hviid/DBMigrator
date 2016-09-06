using DBMigrator.Model;
using DBMigrator.Test.Comparers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;

namespace DBMigrator.Test
{
    [TestClass]
    public class FolderAheadOfDatabaseDiffTest
    {
        private List<DBVersion> Database()
        {
            var version = new DBVersion("1.0.0");
            var feature = version.AddFeature("TestFeature");

            var script = new Script("TestScript.sql", 1, Script.SQLTYPE.Upgrade, feature);
            feature.AddScript(script);

            return new List<DBVersion> { version };
        }

        private List<DBVersion> Folder()
        {
            var version = new DBVersion("1.0.0");
            var feature = version.AddFeature("TestFeature");

            var script = new Script("TestScript.sql", 1, Script.SQLTYPE.Upgrade, feature);
            feature.AddScript(script);

            var version2 = new DBVersion("2.0.0");
            var feature2 = version2.AddFeature("TestFeature2");

            var script2 = new Script("TestScript2.sql", 1, Script.SQLTYPE.Upgrade, feature2);
            feature2.AddScript(script2);

            return new List<DBVersion> { version, version2 };
        }

        private List<DBVersion> ExpectedDiff()
        {
            var version = new DBVersion("2.0.0");
            var feature = version.AddFeature("TestFeature2");

            var script = new Script("TestScript2.sql", 1, Script.SQLTYPE.Upgrade, feature);
            feature.AddScript(script);

            return new List<DBVersion> { version };
        }


        [TestMethod]
        public void Test()
        {
            var versionDiff = new VersionDiff();

            var actualDiff = versionDiff.Diff(Folder(), Database());
            var expectedDiff = ExpectedDiff();
            
            CollectionAssert.AreEqual(expectedDiff, actualDiff, new DBVersionComparer());
        }
    }
}
