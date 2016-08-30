using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using DBMigrator.Model;
using Microsoft.Extensions.Logging;
using System.Text;

namespace DBMigrator
{
    public class Migrator : IDisposable
    {
        private Database _database;
        private DBFolder _dBFolder;
        
        public Migrator(Database database, DBFolder dbFolder)
        {
            _database = database;
            _dBFolder = dbFolder;
        }

        public void Rollback(List<DBVersion> versionsToRollback)
        {
            _database.BeginTransaction();

            if (versionsToRollback.Count() == 0)
            {
                Logger.GetInstance().Log("No downgrades found");
            }

            foreach (var rollbackToVersion in versionsToRollback)
            {
                Logger.GetInstance().Log($"Downgrading to version {rollbackToVersion.Name}");
                try {

                    _database.UpdateDatabaseVersion(rollbackToVersion);

                    foreach (var featureToRollback in rollbackToVersion.Features)
                    {
                        DowngradeFeature(featureToRollback);
                    }

                    _database.UpdateLog(rollbackToVersion);

                    //throw "test"
                    _database.CommitTransaction();
                } catch(Exception ex) {
                    throw ex;
                }
                finally {
                    _database.RollbackTransaction();
                }
            }

            _database.Close();
        }

        public void Upgrade(List<DBVersion> versionsToUpgrade)
        {
            _database.BeginTransaction();

            if(versionsToUpgrade.Count() == 0)
            {
                Logger.GetInstance().Log("No upgrades found");
                return;
            }
            
            try {
                foreach (var upgradeToVersion in versionsToUpgrade)
                {
                    Logger.GetInstance().Log($"--Upgrading to version {upgradeToVersion.Name}");

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
                Logger.GetInstance().Log(ex.Message);
                _database.RollbackTransaction();
            }

            _database.Close();
        }

        private void DowngradeFeature(Feature feature){
            Logger.GetInstance().Log($"Downgrading database feature {feature.Name}");

            foreach (var script in feature.RollbackScripts)
            {
                Logger.GetInstance().Log($"--------Running script: {script.Name}");
                _database.UpdateDataWithFile(script);
            }
        }

        private void UpgradeFeature(Feature feature){
            Logger.GetInstance().Log($"----Upgrading database with feature: {feature.Name}");
            
            foreach (var script in feature.UpgradeScripts)
            {
                Logger.GetInstance().Log($"--------Running script: {script.Name}");
                _database.UpdateDataWithFile(script);
            }
        }

        public void Dispose()
        {
            _database.Dispose();
        }
    }
}
