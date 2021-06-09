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
            var database = new Database(@"(localdb)\mssqllocaldb", "MyDatabase", "", "", "");
            database.GetDBState();
            database.ExecuteSingleCommand("DELETE FROM DBVersionScripts");
            var versions = database.GetDBState();

            Assert.AreEqual(0, versions.Count);
        }

        [TestMethod]
        public void Versions_one_versions_test()
        {
            var version = new DBVersion("1.0.0");
            var script = version.AddAndOrGetFeature("Feature", 0).AddUpgradeScript("01_test.sql", 1);
            script.Checksum = "A";
            script.SQL = "SELECT * FROM DBVersionScripts";


            var database = new Database(@"(localdb)\mssqllocaldb", "MyDatabase", "", "", "");
            database.GetDBState();
            database.ExecuteSingleCommand("DELETE FROM DBVersionScripts");

            var migrator = new Migrator(database, null, null);
            migrator.Upgrade(new List<DBVersion>{ version });

            var versions = database.GetDBState();

            Assert.AreEqual(1, versions.Count);
            Assert.AreEqual(1, versions.First().Features.Count());
            Assert.AreEqual(1, versions.First().Features.First().UpgradeScripts.Count);
            var dbScript = versions.First().Features.First().UpgradeScripts.First();
            Assert.AreEqual(1, dbScript.Order);
            Assert.AreEqual("A", dbScript.Checksum);
            Assert.AreNotEqual(dbScript.ExecutionTime, null);
        }


        [TestMethod]
        public void Message_test()
        {
            var database = new Database(@"(localdb)\mssqllocaldb", "MyDatabase", "", "", "");
            database.GetDBState();
            database.ExecuteSingleCommand("PRINT 'Test1'");
            database.ExecuteSingleCommand("RAISERROR('Test2', 0, 1, 'asd')");
            
            //See test output windows
        }
    }
}
