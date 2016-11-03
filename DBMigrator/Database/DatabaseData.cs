using DBMigrator.Model;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace DBMigrator
{
    public class DatabaseData
    {
        private readonly Database _database;
        private Logger _logger;

        public DatabaseData(Database database)
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
                var data = _database.ExecuteCommand("SELECT TOP 1 Version FROM DBData order by Date desc");
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
            _logger.Log("Creating DBData table");
            _database.ExecuteSingleCommand(@"CREATE TABLE DBData (
                            [ID] [int] IDENTITY(1,1) NOT NULL, 
                            Date datetime2 NOT NULL, 
                            Version varchar(max) NOT NULL, 
                            Feature varchar(max) NOT NULL, 
                            [Order] int NOT NULL, 
                            Script varchar(max) NOT NULL,
                            ExecutionTime int NOT NULL)");
        }

        public void UpdateDataWithFile(FuncViewStoredProcedureTriggerScript script)
        {
            var sw = new Stopwatch();
            sw.Start();
            _database.ExecuteCommand(script.SQL);
            sw.Stop();
            script.ExecutionTime = Convert.ToInt32(sw.ElapsedMilliseconds);
            _database.ExecuteCommand($@"INSERT INTO DBData (
                                                [Version], 
                                                [Date], 
                                                [Order], 
                                                Feature, 
                                                Script, 
                                                ScriptFileChecksum, 
                                                ExecutionTime) VALUES (
                                                '{script.Feature.Version.Name}',
                                                GETUTCDATE(), 
                                                {script.Order}, 
                                                '{script.Feature.Name}', 
                                                '{script.FileName}',
                                                '{script.Checksum}', 
                                                {script.ExecutionTime})");
        }

        public void DowngradeDataWithFile(Script script)
        {
            _database.ExecuteCommand(script.SQL);
            _database.ExecuteCommand($"DELETE FROM DBData WHERE Script = '{script.RollbackScript.FileName}'");
        }

        public void AppendDatabaseDataState(List<DBVersion> versions)
        {
            CheckDatabaseVersion();
            _database.Sqlconn.Open();
            var data = _database.ExecuteCommand("SELECT [Version], [Feature], [Order], [Script], [ScriptFileChecksum], [ExecutionTime] FROM [DBData]");
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
                    var checksum = data.GetString(4);
                    var executiontime = data.GetInt32(5);

                    var script = dbversion.AddAndOrGetFeature(feature).AddDataScript(scriptFileName, order);
                    script.Checksum = checksum;
                    script.ExecutionTime = executiontime;
                }
            }
            _database.Sqlconn.Close();
        }

    }
}
