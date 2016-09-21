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
            
            feature.AddScript("TestScript.sql", 1, Script.SQLTYPE.Upgrade);

            var version2 = new DBVersion("2.0.0");
            var feature2 = version2.AddAndOrGetFeature("TestFeature2");
            
            feature2.AddScript("TestScript2.sql", 1, Script.SQLTYPE.Upgrade);

            return new List<DBVersion> { version, version2 };
        }

        private List<DBVersion> Folder()
        {
            var version = new DBVersion("1.0.0");
            var feature = version.AddAndOrGetFeature("TestFeature");
            
            feature.AddScript("TestScript.sql", 1, Script.SQLTYPE.Upgrade);

            return new List<DBVersion> { version };
        }

        private List<DBVersion> ExpectedDiff()
        {
            throw new NotImplementedException();
        }


        [TestMethod]
        public void Test()
        {
            throw new NotImplementedException();
        }
    }
}
