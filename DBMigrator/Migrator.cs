using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using DBMigrator.Model;
using Microsoft.Extensions.Logging;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

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
                

                    _database.UpdateDatabaseVersion(rollbackToVersion);

                    foreach (var featureToRollback in rollbackToVersion.Features)
                    {
                        DowngradeFeature(featureToRollback);
                    }

                    _database.UpdateLog(rollbackToVersion);

                    //throw "test"
                    _database.CommitTransaction();
                }
            } catch (Exception ex) {
                _logger.Log(ex.Message);
                _database.RollbackTransaction();
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

                    _database.UpdateDatabaseVersion(upgradeToVersion);

                    foreach (var featureToUpgrade in upgradeToVersion.Features)
                    {
                        UpgradeFeature(featureToUpgrade);
                    }

                    _database.UpdateLog(upgradeToVersion);
                }
                //throw "test"
                _database.CommitTransaction();
            } catch(Exception ex) {
                _logger.Log(ex.Message);
                _database.RollbackTransaction();
            }

            _database.Close();
        }

        private void DowngradeFeature(Feature feature){
            _logger.Log($"Downgrading database feature {feature.Name}");

            foreach (var script in feature.RollbackScripts)
            {
                _logger.Log($"--------Running script: {script.FileName}");
                _database.DatabaseSchema.DowngradeDataWithFile(script);
            }
        }

        private void UpgradeFeature(Feature feature){
            _logger.Log($"----Upgrading database with feature: {feature.Name}");
            
            foreach (var script in feature.UpgradeScripts)
            {
                _logger.Log($"--------Running script: {script.FileName}");
                _database.DatabaseSchema.UpdateDataWithFile(script);
            }

            foreach (var script in feature.FuncsSPsViewsTriggers)
            {
                _logger.Log($"--------Running script: {script.FileName}");
                _database.DatabaseFuncViewStoredProcedureTrigger.UpdateDataWithFile(script);
            }
        }

        public void Dispose()
        {
            _database.Dispose();
        }
    }
}
