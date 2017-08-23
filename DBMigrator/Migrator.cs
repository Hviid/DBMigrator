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

namespace DBMigrator
{
    public class Migrator : IDisposable
    {
        private Database _database;
        private DBFolder _dBFolder;
        private Logger _logger;
        
        public Migrator(Database database, DBFolder dbFolder)
        {
            _database = database;
            _dBFolder = dbFolder;
            _logger = Bootstrapper.GetConfiguredServiceProvider().GetRequiredService<Logger>();
        }

        public void Rollback(List<DBVersion> versionsToRollback)
        {
            _database.BeginTransaction();

            if (versionsToRollback.Count() == 0)
            {
                _logger.Log("No downgrades found");
            }

            try
            {
                foreach (var rollbackToVersion in versionsToRollback)
                {
                _logger.Log($"Downgrading to version {rollbackToVersion.Name}");

                    foreach (var featureToRollback in rollbackToVersion.Features)
                    {
                        DowngradeFeature(featureToRollback);
                    }

                    //throw "test"
                    _database.CommitTransaction();
                }
            } catch (Exception ex) {
                _logger.Log(ex.Message);
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
                _logger.Log("No upgrades found");
                return;
            }
            
            try {
                foreach (var upgradeToVersion in versionsToUpgrade)
                {
                    _logger.Log($"--Upgrading to version {upgradeToVersion.Name}");
                    
                    foreach (var featureToUpgrade in upgradeToVersion.Features)
                    {
                        UpgradeFeature(featureToUpgrade);
                    }
                }
                //throw "test"
                _database.CommitTransaction();
            } catch(Exception ex) {
                _logger.Log(ex.Message);
                _database.RollbackTransaction();
                throw;
            }

            _database.Close();
        }

        private void DowngradeFeature(Feature feature){
            _logger.Log($"Downgrading database feature {feature.Name}");

            foreach (var script in feature.RollbackScripts)
            {
                _logger.Log($"--------Running script: {script.FileName}");
                DowngradeWithFile(script);
            }
        }

        private void UpgradeFeature(Feature feature){
            _logger.Log($"----Upgrading database with feature: {feature.Name}");
            
            foreach (var script in feature.UpgradeScripts)
            {
                _logger.Log($"--------Running script: {script.FileName}");
                UpgradeWithFile(script);
            }
        }

        private void UpgradeWithFile(UpgradeScript script)
        {
            var sw = new Stopwatch();
            sw.Start();
            _database.ExecuteSingleCommand(script.SQL);
            sw.Stop();
            script.ExecutionTime = Convert.ToInt32(sw.ElapsedMilliseconds);

            _database.ExecuteSingleCommand(MigratorModelScripts.GetInsertDBVersionScript(
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
            ));
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
