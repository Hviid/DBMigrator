using DBMigrator.Middleware;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace DBMigrator.Test.Middleware
{
    [TestClass]
    public class PrePostMigrationScriptsTest
    {
        [TestMethod]
        public void LoadingPreScript()
        {
            var middleware = new DBMigrator.Middleware.Middleware();
            var path = Path.Combine(Path.GetDirectoryName(typeof(VersionValidator).GetTypeInfo().Assembly.Location), "TestDBFolderStructure");
            var dirInfo = new DirectoryInfo(path);
            middleware.RegisterMiddleware(new PrePostMigrationScripts(dirInfo));
            
            Assert.AreEqual(1, middleware.PreVersionsUpgradeScripts.Count);
            Assert.AreEqual("", middleware.PreVersionsUpgradeScripts[0].SQL);
        }

        [TestMethod]
        public void LoadingPostScript()
        {
            var middleware = new DBMigrator.Middleware.Middleware();
            var path = Path.Combine(Path.GetDirectoryName(typeof(VersionValidator).GetTypeInfo().Assembly.Location), "TestDBFolderStructure");
            var dirInfo = new DirectoryInfo(path);
            middleware.RegisterMiddleware(new PrePostMigrationScripts(dirInfo));

            Assert.AreEqual(1, middleware.PostVersionsUpgradeScripts.Count);
            Assert.AreEqual("", middleware.PostVersionsUpgradeScripts[0].SQL);
        }
    }
}
