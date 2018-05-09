﻿using DBMigrator.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Security.Cryptography;

namespace DBMigrator
{
    public class DBFolder
    {
        private DirectoryInfo _migrationsDirectory;
        public List<DBVersion> allVersions;


        public DBFolder(DirectoryInfo migrationsDirectory)
        {
            _migrationsDirectory = migrationsDirectory;
            allVersions = GetFolderState();
        }

        public List<DBVersion> GetVersions(string upToVersionStr)
        {
            if(string.IsNullOrEmpty(upToVersionStr))
                return allVersions.ToList();

            Version upToVersion;
            try
            {
                upToVersion = new Version(upToVersionStr);
            }
            catch (Exception)
            {
                throw new ArgumentException($"Could not parse {upToVersionStr} into Version object", nameof(upToVersionStr));
            }
            
            return allVersions.Where(v => v.Version <= upToVersion).ToList();
        }

        private List<DBVersion> GetFolderState()
        {
            var allVersions = _migrationsDirectory.GetDirectories().Select(d => new DBVersion(d.Name)).ToList();

            foreach (var version in allVersions)
            {
                FindFeaturesForVersion(version);
            }

            return allVersions;
        }

        private void FindFeaturesForVersion(DBVersion version)
        {
            var featureFolders = GetVersionDirectory(version).GetDirectories();
            foreach (var featureFolder in featureFolders)
            {
                var (order, featureName) = Feature.GetFeatureNameAndOrder(featureFolder.Name);

                version.AddAndOrGetFeature(featureName, order);
            }
            foreach (var feature in version.Features)
            {
                FindMigrationsForFeature(feature);
            }
        }

        private void FindMigrationsForFeature(Feature feature)
        {
            var migrationsPath = Path.Combine(GetFeaturePath(feature), "Migrations");

            var sqlScriptsNames = Directory.GetFiles(migrationsPath, "*.sql");

            foreach (var scriptName in sqlScriptsNames.Select(s => Path.GetFileName(s)))
            {
                var match = Regex.Match(scriptName, MigrationScript.MIGRATIONS_UPGRADE_FILENAME_REGEX);

                if (match.Success)
                {
                    var order = int.Parse(match.Groups[1].Value);
                    
                    var script = feature.AddUpgradeScript(scriptName, order);
                    var filePath = Path.Combine(migrationsPath, scriptName);
                    script.AddFileInfo(GetFileContent(filePath), GetFileChecksum(filePath));
                    script.RollbackScript = FindRollback(script);
                }
                else if (!Regex.IsMatch(scriptName, MigrationScript.MIGRATIONS_ROLLBACK_FILENAME_REGEX))
                {
                    throw new Exception($"file {scriptName} aren't a rollback script, and doesn't match the expected regex format: {MigrationScript.MIGRATIONS_UPGRADE_FILENAME_REGEX}");
                }
            }
        }

        private DirectoryInfo GetVersionDirectory(DBVersion version)
        {
            return new DirectoryInfo(Path.Combine(_migrationsDirectory.FullName, version.Name));
        }

        private string GetFeaturePath(Feature feature)
        {
            var versionFolderPath = Path.Combine(_migrationsDirectory.FullName, feature.Version.Name);
            return Path.Combine(versionFolderPath, feature.Name);
        }

        public void AddRollbacks(List<DBVersion> versions)
        {
            foreach (var version in versions)
            {
                foreach (var feature in version.Features)
                {
                    foreach (var script in feature.UpgradeScripts)
                    {
                        script.RollbackScript = FindRollback(script);
                    }
                }
            }
        }

        private DowngradeScript FindRollback(UpgradeScript upgradeScript)
        {
            var match = Regex.Match(upgradeScript.FileName, MigrationScript.MIGRATIONS_UPGRADE_FILENAME_REGEX);

            var rollbackFileName = $"{match.Groups[1]}_rollback_{match.Groups[2]}.sql";

            var filePath = Path.Combine(GetFeaturePath(upgradeScript.Feature), "Migrations", rollbackFileName);
            if (System.IO.File.Exists(filePath))
            {
                var rollbackScript =  new DowngradeScript(rollbackFileName, upgradeScript.Order, upgradeScript.Feature);
                rollbackScript.UpgradeScript = upgradeScript;
                rollbackScript.SQL = GetFileContent(filePath);
                return rollbackScript;
            }
            return null;
        }

        private string GetFileChecksum(string filePath)
        {
            using (var stream = new BufferedStream(System.IO.File.OpenRead(filePath), 1200000))
            {
                SHA256 sha = SHA256.Create();
                byte[] checksum = sha.ComputeHash(stream);
                return BitConverter.ToString(checksum).Replace("-", String.Empty);
            }
        }

        private string GetFileContent(string filePath)
        {
            return System.IO.File.ReadAllText(filePath);
        }
    }
}
