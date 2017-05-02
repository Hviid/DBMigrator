using DBMigrator.Model;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace DBMigrator
{
    public class DatabaseFuncViewStoredProcedureTrigger
    {
        private readonly Database _database;
        private Logger _logger;

        public DatabaseFuncViewStoredProcedureTrigger(Database database)
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
                var data = _database.ExecuteCommand("SELECT TOP 1 Version FROM DBFuncViewStoredProcedureTriggerScripts order by Date desc");
                if (data.HasRows)
                {
                    data.Read();
                    version = data.GetString(0);
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

        private void CreateDBVersionTable()
        {
            _logger.Log("Creating DBFuncViewStoredProcedureTriggerScripts table");
            _database.ExecuteSingleCommand(@"CREATE TABLE DBFuncViewStoredProcedureTriggerScripts (
                            [ID] [int] IDENTITY(1,1) NOT NULL, 
                            Date datetime2 NOT NULL, 
                            Version varchar(max) NOT NULL, 
                            Feature varchar(max) NOT NULL, 
                            [Order] int NOT NULL, 
                            Script varchar(max) NOT NULL, 
                            Type varchar(max) NOT NULL, 
                            Name varchar(max) NOT NULL,
                            [ScriptFileChecksum] varchar(max) NOT NULL, 
                            [DatabaseTriggersChecksum] varchar(max) NOT NULL, 
                            [DatabaseTablesAndViewsChecksum] varchar(max) NOT NULL, 
                            [DatabaseFunctionsChecksum] varchar(max) NOT NULL, 
                            [DatabaseStoredProceduresChecksum] varchar(max) NOT NULL, 
                            [DatabaseIndexesChecksum] varchar(max) NOT NULL, 
                            ExecutionTime int NOT NULL)");
        }

        public void UpdateDataWithFile(FuncViewStoredProcedureTriggerScript script)
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
            _database.ExecuteCommand($@"INSERT INTO DBFuncViewStoredProcedureTriggerScripts (
                                                [Version], 
                                                [Date], 
                                                [Order], 
                                                Feature, 
                                                Script, 
                                                Type, 
                                                Name,
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
                                                '{script.Type.ToString()}', 
                                                '{script.Name}',
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
            _database.ExecuteCommand($"DELETE FROM DBFuncViewStoredProcedureTriggerScripts WHERE Script = '{script.FileName}'");
        }

        public void AppendDatabaseFuncViewStoredProcedureTriggerState(List<DBVersion> versions)
        {
            CheckDatabaseVersion();
            _database.Sqlconn.Open();
            var data = _database.ExecuteCommand("SELECT [Version], [Feature], [Order], [Script], [Type], [Name], [ScriptFileChecksum], [ExecutionTime] FROM [DBFuncViewStoredProcedureTriggerScripts]");
            while (data.Read())
            {
                var version = data.GetString(0);

                var dbversion = versions.FirstOrDefault(v => v.Name == version);
                if (dbversion == null)
                {
                    throw new Exception($"Could not find version {version}");
                }

                string feature = null;
                if (!data.IsDBNull(1))
                {
                    feature = data.GetString(1);
                    var order = data.GetInt32(2);
                    var scriptFileName = data.GetString(3);
                    var type = data.GetString(4);
                    var name = data.GetString(5);
                    var checksum = data.GetString(6);
                    var executiontime = data.GetInt32(7);

                    var script = dbversion.AddAndOrGetFeature(feature).AddFuncViewStoredProcedureTriggerScript(scriptFileName, type, name, order);
                    script.Checksum = checksum;
                    script.ExecutionTime = executiontime;
                }
            }
            _database.Sqlconn.Close();
        }

    }
}
