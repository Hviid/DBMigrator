﻿using DBMigrator.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace DBMigrator.Test
{
    [TestClass]
    public class DbFolderTest
    {

        private List<DBVersion> Folder()
        {
            var version = new DBVersion("1.0.0");
            var feature = version.AddAndOrGetFeature("TestFeature");
            
            var script = feature.AddUpgradeScript("01_Test.sql", 1);
            script.Checksum = "A";

            return new List<DBVersion> { version };
        }

        [TestMethod]
        public void Load_Folder_Test()
        {
            var path = Path.Combine(Path.GetDirectoryName(typeof(VersionValidator).GetTypeInfo().Assembly.Location), "TestDBFolderStructure");
            var dirInfo = new DirectoryInfo(path);
            var dbFolder = new DBFolder(dirInfo);

            var differ = new VersionDiff();

            var diff = differ.Diff(dbFolder.allVersions, Folder());

            Assert.AreEqual(0, diff.Count);
        }

    }
}
