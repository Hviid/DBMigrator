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

        public void Rollback(string toVersionString)
        {
            _database.BeginTransaction();
            var databaseVersionString = _database.CheckDatabaseVersion();
            var databaseVersion = new Version(databaseVersionString);

            Logger.GetInstance().Log($"Rolling back to version {toVersionString}");
            var toVersion = new Version(toVersionString);
            var allFolderVersions = _dBFolder.allVersions;
            var versionToRollback = allFolderVersions.Where(v => v.Version <= databaseVersion && v.Version > toVersion).OrderBy(d => d.Version).ToList();

            if (versionToRollback.Count() == 0)
            {
                Logger.GetInstance().Log("No downgrades found");
            }

            foreach (var rollbackToVersion in versionToRollback)
            {
                Logger.GetInstance().Log($"Downgrading to version {rollbackToVersion.Name}");
                try {

                    foreach (var featureToRollback in rollbackToVersion.Features)
                    {
                        DowngradeFeature(featureToRollback);
                    }

                    _database.UpdateDatabaseVersion(rollbackToVersion);

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

        public void Upgrade(string toVersionString)
        {
            _database.BeginTransaction();

            var databaseVersionString = _database.CheckDatabaseVersion();
            var databaseVersion = new Version(databaseVersionString);
            var versionsToUpgrade = new List<DBVersion>();
            var allFolderVersions = _dBFolder.allVersions;

            if (!string.IsNullOrEmpty(toVersionString)){
                Logger.GetInstance().Log($"Upgrading to version {toVersionString}");
                var toVersion = new Version(toVersionString);
                versionsToUpgrade = allFolderVersions.Where(d => d.Version > databaseVersion && d.Version <= toVersion).OrderBy(d => d.Version).ToList();
            } else {
                Logger.GetInstance().Log("Upgrading to newest version");
                versionsToUpgrade = allFolderVersions.Where(d => d.Version > databaseVersion).OrderBy(d => d.Version).ToList();
            }

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

            //RunFeatureMigration(folder.Name, rollbackFolder);
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
