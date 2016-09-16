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

            var database = new Database("");
            database.ExecuteSingleCommand("DELETE FROM DBVersion");

            database.UpdateDatabaseVersion(version);

            var versions = database.GetDBState();

            Assert.AreEqual(1, versions.Count);
        }
    }
}
