using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using DBMigrator.Model;
using Microsoft.Extensions.Logging;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using DBMigrator.SQL;
using System.Text.RegularExpressions;

namespace DBMigrator
{
    public class Migrator : IDisposable
    {
        private Database _database;
        private DBFolder _dBFolder;
        private readonly ILogger<Migrator> _logger;
        private Regex _goRegex = new Regex(@"[\n\r]GO\b");
        private Middleware.Middleware _middleware;

        public Migrator(Database database, DBFolder dbFolder, Middleware.Middleware middleware)
        {
            _middleware = middleware;
            _database = database;
            _dBFolder = dbFolder;
            var loggerFactory = Bootstrapper.GetConfiguredServiceProvider().GetRequiredService<ILoggerFactory>();
            _logger = loggerFactory.CreateLogger<Migrator>();
        }

        public void Rollback(List<DBVersion> versionsToRollback)
        {
            _database.BeginTransaction();

            if (versionsToRollback.Count() == 0)
            {
                _logger.LogInformation("No downgrades found");
            }

            try
            {
                foreach (var rollbackToVersion in versionsToRollback)
                {
                    _logger.LogInformation($"Downgrading to version {rollbackToVersion.Name}");

                    foreach (var featureToRollback in rollbackToVersion.Features)
                    {
                        DowngradeFeature(featureToRollback);
                    }

                    //throw "test"
                    _database.CommitTransaction();
                }
            } catch (Exception ex) {
                _logger.LogError(ex, ex.Message);
                _database.RollbackTransaction();
                throw;
            }

            _database.Close();
        }

        public void Upgrade(List<DBVersion> versionsToUpgrade)
        {
            _database.BeginTransaction();

            if(versionsToUpgrade.Count() == 0)
            {
                _logger.LogInformation("No upgrades found");
                return;
            }
            
            try
            {
                if(_middleware != null)
                {
                    foreach (var preUpgradeScript in _middleware.PreVersionsUpgradeScripts)
                    {
                        _logger.LogInformation($"--Running pre upgrade script: {preUpgradeScript.FileName}");
                        _database.ExecuteSingleCommand(preUpgradeScript.SQL);
                    }
                }

                foreach (var upgradeToVersion in versionsToUpgrade)
                {
                    _logger.LogInformation($"--Upgrading to version {upgradeToVersion.Name}");
                    
                    foreach (var featureToUpgrade in upgradeToVersion.Features)
                    {
                        UpgradeFeature(featureToUpgrade);
                    }
                }

                if (_middleware != null)
                {
                    foreach (var postUpgradeScript in _middleware.PostVersionsUpgradeScripts)
                    {
                        _logger.LogInformation($"--Running post upgrade script: {postUpgradeScript.FileName}");
                        _database.ExecuteSingleCommand(postUpgradeScript.SQL);
                    }
                }

                //throw new Exception("test");
                _database.CommitTransaction();
            } catch(Exception ex) {
                _logger.LogError(ex, ex.Message);
                _database.RollbackTransaction();
                throw ex;
            }
            _database.Close();
        }

        private void DowngradeFeature(Feature feature){
            _logger.LogInformation($"Downgrading database feature {feature.Name}");

            foreach (var script in feature.RollbackScripts)
            {
                _logger.LogInformation($"--------Running script: {script.FileName}");
                DowngradeWithFile(script);
            }
        }

        private void UpgradeFeature(Feature feature){
            _logger.LogInformation($"----Upgrading database with feature: {feature.Name}");
            
            foreach (var script in feature.UpgradeScripts)
            {
                _logger.LogInformation($"--------Running script: {script.FileName}");
                UpgradeWithFile(script);
            }
        }

        private IEnumerable<string> BatchByGoStatement(string sqltext)
        {
            return _goRegex.Split(sqltext).Where(cmd => !string.IsNullOrWhiteSpace(cmd));
        }

        private void UpgradeWithFile(UpgradeScript script)
        {
            var sw = new Stopwatch();
            sw.Start();
            try
            {
                _database.ExecuteMultipleCommands(BatchByGoStatement(script.SQL));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to run script '{script}' on database", script.FileName);
                throw;
            }
            sw.Stop();
            script.ExecutionTime = Convert.ToInt32(sw.ElapsedMilliseconds);

            try {
                var cmd = MigratorModelScripts.GetInsertDBVersionScript(
                    script.Feature.Version.Name,
                    script.Order,
                    script.Feature.Name,
                    script.FileName,
                    script.Checksum,
                    $"({ChecksumScripts.GetHashbytesFor(ChecksumScripts.TriggersChecksum)})",
                    $"({ChecksumScripts.GetHashbytesFor(ChecksumScripts.TablesViewsAndColumnsChecksumScript)})",
                    $"({ChecksumScripts.GetHashbytesFor(ChecksumScripts.FunctionsChecksum)})",
                    $"({ChecksumScripts.GetHashbytesFor(ChecksumScripts.StoredProceduresChecksum)})",
                    $"({ChecksumScripts.GetHashbytesFor(ChecksumScripts.IndexesChecksum)})",
                    script.ExecutionTime.Value
                );

                _database.ExecuteSingleCommand(cmd);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update database with entry for script '{script}'", script.FileName);
                throw;
            }
        }

        private void DowngradeWithFile(DowngradeScript script)
        {
            _database.ExecuteSingleCommand(script.SQL);
            _database.ExecuteSingleCommand(MigratorModelScripts.GetDeleteDBVersionScript(script.FileName.Replace("_rollback_", "_")));
        }

        public void Dispose()
        {
            _database.Dispose();
        }
    }
}
