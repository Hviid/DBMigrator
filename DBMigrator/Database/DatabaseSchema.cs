using DBMigrator.Model;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace DBMigrator
{
    public class DatabaseSchema
    {
        private readonly Database _database;
        private Logger _logger;

        public DatabaseSchema(Database database)
        {
            _logger = Bootstrapper.GetConfiguredServiceProvider().GetRequiredService<Logger>();
            _database = database;
        }

        public string CheckDatabaseVersion()
        {
            var version = "0.0.0.0";
            try
            {
                _database.Sqlconn.Open();
                //var data = ExecuteCommand("SELECT TOP 1 Version FROM DBVersion order by Date desc");
                var data = _database.ExecuteCommand("SELECT TOP 1 Version FROM DBVersionScripts order by Date desc");
                if (data.HasRows)
                {
                    data.Read();
                    version = data.GetString(0);
                    _logger.Log($"Found existing database version {version}");
                }
                _database.Sqlconn.Close();
            }
            catch (Exception ex)
            {
                _database.Sqlconn.Close();
                CreateDBVersionTable();
            }
            return version;
        }

        private void CreateDBVersionTable2()
        {
            _logger.Log("Creating DBVersion table");
            _database.ExecuteSingleCommand("CREATE TABLE DBVersion ([ID] [int] IDENTITY(1,1) NOT NULL, Version varchar(max) NOT NULL, Date datetime2 NOT NULL, Log xml NOT NULL, CONSTRAINT [PK_dbo.DBVersion] PRIMARY KEY CLUSTERED ([ID] ASC))");
            _database.ExecuteSingleCommand("CREATE TABLE DBVersionScripts (DBVersionID int NOT NULL, Feature varchar(max) NOT NULL, [Order] int NOT NULL, Script varchar(max) NOT NULL, Type varchar(max) NOT NULL, [Checksum] varchar(max) NOT NULL, ExecutionTime int NOT NULL)");
            _database.ExecuteSingleCommand("ALTER TABLE [DBVersionScripts] WITH CHECK ADD CONSTRAINT [FK.DBVersion.DBVersionScripts_DBVersionID] FOREIGN KEY([DBVersionID]) REFERENCES [DBVersion]([ID])");
        }

        private void CreateDBVersionTable()
        {
            _logger.Log("Creating DBVersion table");
            _database.ExecuteSingleCommand(@"CREATE TABLE DBVersionScripts (
                            [ID] [int] IDENTITY(1,1) NOT NULL, 
                            Date datetime2 NOT NULL, 
                            Version varchar(max) NOT NULL, 
                            Feature varchar(max) NOT NULL, 
                            [Order] int NOT NULL, 
                            Script varchar(max) NOT NULL, 
                            Type varchar(max) NOT NULL, 
                            [ScriptFileChecksum] varchar(max) NOT NULL, 
                            [DatabaseTriggersChecksum] varchar(max) NOT NULL, 
                            [DatabaseTablesAndViewsChecksum] varchar(max) NOT NULL, 
                            [DatabaseFunctionsChecksum] varchar(max) NOT NULL, 
                            [DatabaseStoredProceduresChecksum] varchar(max) NOT NULL, 
                            [DatabaseIndexesChecksum] varchar(max) NOT NULL, 
                            ExecutionTime int NOT NULL)");
        }

        public void UpdateDataWithFile(UpgradeScript script)
        {
            var sw = new Stopwatch();
            sw.Start();
            _database.ExecuteCommand(script.SQL);
            sw.Stop();
            script.ExecutionTime = Convert.ToInt32(sw.ElapsedMilliseconds);
            var databaseTriggersChecksum = _database.GetTriggersChecksum();
            var databaseTablesAndViewsChecksum = _database.GetTablesViewsAndColumnsChecksum();
            var databaseFunctionsChecksum = _database.GetFunctionsChecksum();
            var databaseStoredProceduresChecksum = _database.GetStoredProceduresChecksum();
            var databaseIndexesChecksum = _database.GetIndexesChecksum();
            //ExecuteCommand($"INSERT INTO DBVersionScripts (DBVersionID, [Order], Feature, Script, Type, Checksum, ExecutionTime) VALUES ('{script.Feature.Version.ID}', {script.Order}, '{script.Feature.Name}', '{script.FileName}', '{script.Type.ToString()}', '{script.Checksum}', {script.ExecutionTime})");
            _database.ExecuteCommand($@"INSERT INTO DBVersionScripts (
                                                [Version], 
                                                [Date], 
                                                [Order], 
                                                Feature, 
                                                Script, 
                                                Type, 
                                                ScriptFileChecksum, 
                                                DatabaseTriggersChecksum, 
                                                DatabaseTablesAndViewsChecksum, 
                                                DatabaseFunctionsChecksum, 
                                                DatabaseStoredProceduresChecksum, 
                                                DatabaseIndexesChecksum, 
                                                ExecutionTime) VALUES (
                                                '{script.Feature.Version.Name}',
                                                GETUTCDATE(), 
                                                {script.Order}, 
                                                '{script.Feature.Name}', 
                                                '{script.FileName}', 
                                                'Upgrade', 
                                                '{script.Checksum}', 
                                                '{databaseTriggersChecksum}', 
                                                '{databaseTablesAndViewsChecksum}', 
                                                '{databaseFunctionsChecksum}', 
                                                '{databaseStoredProceduresChecksum}', 
                                                '{databaseIndexesChecksum}', 
                                                {script.ExecutionTime})");
        }

        public void DowngradeDataWithFile(DowngradeScript script)
        {
            _database.ExecuteCommand(script.SQL);
            _database.ExecuteCommand($"DELETE FROM DBVersionScripts WHERE Script = '{script.FileName.Replace("_rollback_","_")}'");
        }

        public List<DBVersion> GetDBState()
        {
            CheckDatabaseVersion();
            _database.Sqlconn.Open();
            var result = new List<DBVersion>();
            //var data = ExecuteCommand("SELECT [Version], [Feature], [Order], [Script], [Type], [Checksum], [ExecutionTime] FROM [DBVersion] LEFT JOIN [DbversionScripts] ON [DBVersion].ID = [DbversionScripts].DBVersionID");
            var data = _database.ExecuteCommand("SELECT [Version], [Feature], [Order], [Script], [Type], [ScriptFileChecksum], [ExecutionTime] FROM [DBVersionScripts]");
            while (data.Read())
            {
                var version = data.GetString(0);

                var dbversion = result.FirstOrDefault(v => v.Name == version);
                if (dbversion == null)
                {
                    dbversion = new DBVersion(version);
                    result.Add(dbversion);
                }

                string feature = null;
                if (!data.IsDBNull(1))
                {
                    feature = data.GetString(1);
                    var order = data.GetInt32(2);
                    var scriptFileName = data.GetString(3);
                    var type = data.GetString(4);
                    var checksum = data.GetString(5);
                    var executiontime = data.GetInt32(6);

                    var script = dbversion.AddAndOrGetFeature(feature).AddUpgradeScript(scriptFileName, order);
                    script.Checksum = checksum;
                    script.ExecutionTime = executiontime;
                }
            }
            _database.Sqlconn.Close();
            return result;
        }

    }
}
