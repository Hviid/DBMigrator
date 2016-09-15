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
        public DatabaseTest()
        {
            Bootstrapper.ConfigureServices(new ServiceCollection());
            //var path = Path.Combine(DBFolder.GetExecutingDir().FullName, "DBMigratorTest.mdf");
            var database = new Database("");
            //TO DO couldn't it just aswell do everything in the master database?
            database.ExecuteSingleCommand("if db_id('DBMigratorTest') is null CREATE DATABASE DBMigratorTest");
            database.Dispose();
        }

        [TestMethod]
        public void Versions_noversions_test()
        {
            var database = new Database("DBMigratorTest");
            var versions = database.GetDBState();

            Assert.AreEqual(0, versions.Count);
        }
    }
}
