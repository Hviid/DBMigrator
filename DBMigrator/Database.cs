using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SqlClient;
using DBMigrator.Model;
using System.Text;
using Microsoft.Extensions.Logging;
using System.IO;

namespace DBMigrator
{
    public class Database : IDisposable
    {
        private string executingPath;
        private SqlConnection sqlconn;
        private SqlTransaction trans;
        public List<DBVersion> allVersions;

        public Database(string servername, string database, string username, string password)
        {
            executingPath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            var connectionString = $"Data Source={servername};Initial Catalog={database};Persist Security Info=True;User ID={username};Password={password};MultipleActiveResultSets=True";
            sqlconn = new SqlConnection(connectionString);
            sqlconn.Open();
            allVersions = GetDBState();
        }

        public string CheckDatabaseVersion()
        {
            var version = "0.0.0.0";
            try
            {
                var data = ExecuteCommand("SELECT TOP 1 Version FROM DBVersion order by Date desc");
                data.Read();
                version = data.GetString(0);
                Logger.GetInstance().Log($"Found existing database version {version}");
            }
            catch (Exception ex)
            {
                CreateDBVersionTable();
            }
            return version;
        }

        public void CreateDBVersionTable()
        {
            Logger.GetInstance().Log("Creating DBVersion table");
            ExecuteCommand("CREATE TABLE DBVersion ([ID] [int] IDENTITY(1,1) NOT NULL, Version varchar(max) NOT NULL, Date datetime2 NOT NULL, Log xml NOT NULL, CONSTRAINT [PK_dbo.DBVersion] PRIMARY KEY CLUSTERED ([ID] ASC))");
            ExecuteCommand("CREATE TABLE DBVersionScripts (DBVersionID int NOT NULL, Feature varchar(max) NOT NULL, [Order] int NOT NULL, Script varchar(max) NOT NULL, Type varchar(max) NOT NULL, [Checksum] varchar(max) NOT NULL, ExecutionTime int NOT NULL)");
            ExecuteCommand("ALTER TABLE [DBVersionScripts] WITH CHECK ADD CONSTRAINT [FK.DBVersion.DBVersionScripts_DBVersionID] FOREIGN KEY([DBVersionID]) REFERENCES [DBVersion]([ID])");
        }

        public void UpdateDatabaseVersion(DBVersion version)
        {
            var versionStr = version.Version.ToString();
            Logger.GetInstance().Log($"Updating DBVersion version to {versionStr}");
            var data = ExecuteCommand($"INSERT INTO DBVersion (Version, Date, Log) OUTPUT Inserted.ID VALUES ('{versionStr}', GETUTCDATE(), '<xml>' + CHAR(13) + '{Logger.GetInstance().log.ToString()}</xml>')");
            using (data)
            {
                data.Read();
                version.ID = data.GetInt32(0);
            }
        }

        public void UpdateDataWithFile(Script script)
        {
            ExecuteCommand($"INSERT INTO DBVersionScripts (DBVersionID, [Order], Feature, Script, Type, Checksum, ExecutionTime) VALUES ('{script.Feature.Version.ID}', {script.Order}, '{script.Feature.Name}', '{script.Name}', '{script.Type.ToString()}', '{script.Checksum}', {script.ExecutionTime})");
            ExecuteCommand(script.SQL);
        }

        private SqlDataReader ExecuteCommand(string cmd)
        {
            using (SqlCommand command = new SqlCommand(cmd, sqlconn, trans))
            {
                var result = command.ExecuteReader();
                return result;
            }
        }

        public void BeginTransaction()
        {
            trans = sqlconn.BeginTransaction();
        }

        public void CommitTransaction()
        {
            trans.Commit();
        }

        public void RollbackTransaction()
        {
            trans.Rollback();
        }

        public void Close()
        {
            sqlconn.Close();
        }

        public List<DBVersion> GetDBState() {
            var result = new List<DBVersion>();
            var data = ExecuteCommand("SELECT [Version], [Feature], [Order], [Script], [Type], [Checksum], [ExecutionTime] FROM [DBVersion] INNER JOIN [DbversionScripts] ON [DBVersion].ID = [DbversionScripts].DBVersionID");
            while (data.Read())
            {
                var version = data.GetString(0);
                var feature = data.GetString(1);
                var order = data.GetInt32(2);
                var script = data.GetString(3);
                var type = data.GetString(4);
                var checksum = data.GetString(5);
                var executiontime = data.GetInt32(6);

                var dbversion = result.FirstOrDefault(v => v.Name == version);
                if (dbversion == null)
                {
                    dbversion = new DBVersion(new DirectoryInfo(Path.Combine(executingPath, version)));
                    result.Add(dbversion);
                }
                dbversion.AddOrUpdateFeature(feature, new Script(new FileInfo(Path.Combine(dbversion.Directory.FullName, feature, "Migrations", script)), order, (Script.SQLTYPE)Enum.Parse(typeof(Script.SQLTYPE), type), null, null));
            }
            return result;
        }

        //public Script GetNewestScript()
        //{
        //    var newestVersion = allVersions.OrderByDescending(v => v.Version).First();
        //}

        public void Dispose()
        {
            sqlconn.Close();
        }
    }
}
