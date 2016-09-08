using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DBMigrator.Test
{
    [TestClass]
    public class DatabaseChecksumTest
    {
        [TestMethod]
        public void GetTablesViewsAndColumnsChecksum_doesnt_change()
        {
            throw new NotImplementedException();
            //TODO requires test database
            var database = new Database("", "", "", "");
            var checksum = database.GetTablesViewsAndColumnsChecksum();
            var checksum2 = database.GetTablesViewsAndColumnsChecksum();
            Assert.AreEqual(checksum, checksum2);
        }

        [TestMethod]
        public void TablesChecksum_changes()
        {
            throw new NotImplementedException();
            //TODO requires test database
            var database = new Database("", "", "", "");
            var checksum = database.GetTablesViewsAndColumnsChecksum();
            database.ExecuteSingleCommand("CREATE TABLE [dbo].[DBMigratorTest]([Test][nvarchar](max) NULL)");
            var checksum2 = database.GetTablesViewsAndColumnsChecksum();
            database.ExecuteSingleCommand("DROP TABLE [dbo].[DBMigratorTest]");
            Assert.AreNotEqual(checksum, checksum2);
        }

        [TestMethod]
        public void ViewsChecksum_changes()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void TableColumnsChecksum_changes()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void ViewColumnsChecksum_changes()
        {
            throw new NotImplementedException();
        }

    }
}
