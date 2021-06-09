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
            var database = new Database(@"(localdb)\mssqllocaldb", "MyDatabase", "", "", "");
            var checksum = database.GetDatabaseCurrentChecksums();
            var checksum2 = database.GetDatabaseCurrentChecksums();
            Assert.AreEqual(BitConverter.ToString(checksum.DatabaseTablesAndViewsChecksum), BitConverter.ToString(checksum2.DatabaseTablesAndViewsChecksum));
        }

        [TestMethod]
        public void TablesChecksum_changes()
        {
            var database = new Database(@"(localdb)\mssqllocaldb", "MyDatabase", "", "", "");
            var checksum = database.GetDatabaseCurrentChecksums();
            database.ExecuteSingleCommand("CREATE TABLE [dbo].[DBMigratorTable]([Test][nvarchar](max) NULL)");
            var checksum2 = database.GetDatabaseCurrentChecksums();
            database.ExecuteSingleCommand("DROP TABLE [dbo].[DBMigratorTable]");
            Assert.AreNotEqual(BitConverter.ToString(checksum.DatabaseTablesAndViewsChecksum), BitConverter.ToString(checksum2.DatabaseTablesAndViewsChecksum));
        }

        [TestMethod]
        public void ViewsChecksum_changes()
        {
            var database = new Database(@"(localdb)\mssqllocaldb", "MyDatabase", "", "", "");
            database.ExecuteSingleCommand("CREATE TABLE [dbo].[DBMigratorTable]([Test][nvarchar](max) NULL)");
            var checksum = database.GetDatabaseCurrentChecksums();
            database.ExecuteSingleCommand("CREATE VIEW [dbo].DBMigratorView AS SELECT Test FROM [dbo].[DBMigratorTable]");
            var checksum2 = database.GetDatabaseCurrentChecksums();
            database.ExecuteSingleCommand("DROP VIEW [dbo].[DBMigratorView]");
            database.ExecuteSingleCommand("DROP TABLE [dbo].[DBMigratorTable]");
            Assert.AreNotEqual(BitConverter.ToString(checksum.DatabaseTablesAndViewsChecksum), BitConverter.ToString(checksum2.DatabaseTablesAndViewsChecksum));
        }

        [TestMethod]
        public void TableColumnsChecksum_changes()
        {
            var database = new Database(@"(localdb)\mssqllocaldb", "MyDatabase", "", "", "");
            database.ExecuteSingleCommand("CREATE TABLE [dbo].[DBMigratorTable]([Test][nvarchar](max) NULL)");
            var checksum = database.GetDatabaseCurrentChecksums();
            database.ExecuteSingleCommand("ALTER TABLE [dbo].[DBMigratorTable] ADD Test2 int");
            var checksum2 = database.GetDatabaseCurrentChecksums();
            database.ExecuteSingleCommand("DROP TABLE [dbo].[DBMigratorTable]");
            Assert.AreNotEqual(BitConverter.ToString(checksum.DatabaseTablesAndViewsChecksum), BitConverter.ToString(checksum2.DatabaseTablesAndViewsChecksum));
        }

        [TestMethod]
        public void TableColumnsDataTypeChecksum_changes()
        {
            var database = new Database(@"(localdb)\mssqllocaldb", "MyDatabase", "", "", "");
            database.ExecuteSingleCommand("CREATE TABLE [dbo].[DBMigratorTable]([Test][nvarchar](max) NULL)");
            var checksum = database.GetDatabaseCurrentChecksums();
            database.ExecuteSingleCommand("ALTER TABLE [dbo].[DBMigratorTable] ALTER COLUMN [Test] int");
            var checksum2 = database.GetDatabaseCurrentChecksums();
            database.ExecuteSingleCommand("DROP TABLE [dbo].[DBMigratorTable]");
            Assert.AreNotEqual(BitConverter.ToString(checksum.DatabaseTablesAndViewsChecksum), BitConverter.ToString(checksum2.DatabaseTablesAndViewsChecksum));
        }

        [TestMethod]
        public void ViewColumnsChecksum_changes()
        {
            var database = new Database(@"(localdb)\mssqllocaldb", "MyDatabase", "", "", "");
            database.ExecuteSingleCommand("CREATE TABLE [dbo].[DBMigratorTable]([Test][nvarchar](max) NULL, Test2 int)");
            database.ExecuteSingleCommand("CREATE VIEW [dbo].DBMigratorView AS SELECT Test FROM [dbo].[DBMigratorTable]");
            var checksum = database.GetDatabaseCurrentChecksums();
            database.ExecuteSingleCommand("ALTER VIEW [dbo].DBMigratorView AS SELECT Test, Test2 FROM [dbo].[DBMigratorTable]");
            var checksum2 = database.GetDatabaseCurrentChecksums();
            database.ExecuteSingleCommand("DROP VIEW [dbo].[DBMigratorView]");
            database.ExecuteSingleCommand("DROP TABLE [dbo].[DBMigratorTable]");
            Assert.AreNotEqual(BitConverter.ToString(checksum.DatabaseTablesAndViewsChecksum), BitConverter.ToString(checksum2.DatabaseTablesAndViewsChecksum));
        }

    }
}
