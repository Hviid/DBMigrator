using DBMigrator.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DBMigrator.Test
{
    [TestClass]
    public class DatabaseTest
    {
        [TestMethod]
        public void Versions_noversions_test()
        {
            var database = new Database("");
            database.ExecuteSingleCommand("DELETE FROM DBVersion");
            var versions = database.GetDBState();

            Assert.AreEqual(0, versions.Count);
        }

        [TestMethod]
        public void Versions_one_versions_test()
        {
            var version = new DBVersion("1.0.0");
            var script = version.AddAndOrGetFeature("Feature").AddScript("01_test.sql", 1, Script.SQLTYPE.Upgrade);
            script.Checksum = "A";
            script.SQL = "SELECT * FROM DBVersion";


            var database = new Database("");
            database.ExecuteSingleCommand("DELETE FROM DBVersionScripts");
            database.ExecuteSingleCommand("DELETE FROM DBVersion");

            database.UpdateDatabaseVersion(version);
            database.UpdateDataWithFile(script);

            var versions = database.GetDBState();

            Assert.AreEqual(1, versions.Count);
            Assert.AreEqual(1, versions.First().Features.Count);
            Assert.AreEqual(1, versions.First().Features.First().UpgradeScripts.Count);
            var dbScript = versions.First().Features.First().UpgradeScripts.First();
            Assert.AreEqual(1, dbScript.Order);
            Assert.AreEqual("A", dbScript.Checksum);
            Assert.IsTrue(dbScript.ExecutionTime > 0);
        }
    }
}
