using DBMigrator.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace DBMigrator
{
    public class DBFolder
    {
        private DirectoryInfo _executingDirectory;
        public List<DBVersion> allVersions;


        public DBFolder(string executingDirectoryPath = null)
        {
            if (String.IsNullOrEmpty(executingDirectoryPath))
            {
                _executingDirectory = GetExecutingDir();
            }
            else
            {
                _executingDirectory = new DirectoryInfo(executingDirectoryPath);
            }
            allVersions = GetFolderState();
        }

        public static DirectoryInfo GetExecutingDir()
        {
            return new DirectoryInfo(Path.GetDirectoryName(typeof(Validator).GetTypeInfo().Assembly.Location));
            //new DirectoryInfo(Path.GetDirectoryName(AppContext.BaseDirectory));
            //new DirectoryInfo(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location));
        }

        public List<DBVersion> GetVersionsUpTo(string version)
        {
            var upToVersion = new Version(version);
            return allVersions.Where(v => v.Version <= upToVersion).ToList();
        }

        private List<DBVersion> GetFolderState()
        {
            var allVersions = _executingDirectory.GetDirectories().Select(d => new DBVersion(d.Name)).ToList();

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
                version.AddAndOrGetFeature(featureFolder.Name);
            }
            foreach (var feature in version.Features)
            {
                FindMigrationsForFeature(feature);
                FindFuncViewStoredProcedureTriggerForFeature(feature);
            }
        }

        private void FindMigrationsForFeature(Feature feature)
        {
            var migrationsPath = Path.Combine(GetFeaturePath(feature), "Migrations");

            var sqlScriptsNames = Directory.GetFiles(migrationsPath, "*.sql");

            foreach (var scriptName in sqlScriptsNames.Select(s => Path.GetFileName(s)))
            {
                var match = Regex.Match(scriptName, Script.MIGRATIONS_UPGRADE_FILENAME_REGEX);

                if (match.Success)
                {
                    var order = int.Parse(match.Groups[1].Value);
                    
                    var script = feature.AddUpgradeScript(scriptName, order);
                    var filePath = Path.Combine(migrationsPath, scriptName);
                    script.SQL = GetFileContent(filePath);
                    script.Checksum = GetFileChecksum(filePath);
                    script.RollbackScript = FindRollback(script);
                }
                else if (!Regex.IsMatch(scriptName, Script.MIGRATIONS_ROLLBACK_FILENAME_REGEX))
                {
                    throw new Exception($"file {scriptName} aren't a rollback script, and doesn't match the expected regex format: {Script.MIGRATIONS_UPGRADE_FILENAME_REGEX}");
                }
            }
        }

        private void FindFuncViewStoredProcedureTriggerForFeature(Feature feature)
        {
            var funcViewStoredProcedureTriggerPath = Path.Combine(GetFeaturePath(feature), "FuncViewStoredProcedureTrigger");

            if (Directory.Exists(funcViewStoredProcedureTriggerPath))
            {
                var sqlScriptsNames = Directory.GetFiles(funcViewStoredProcedureTriggerPath, "*.sql");

                foreach (var scriptName in sqlScriptsNames.Select(s => Path.GetFileName(s)))
                {
                    var scriptNameMatch = Regex.Match(scriptName, Script.FuncViewStoredProcedureTrigger_FILENAME_REGEX);

                    if (scriptNameMatch.Success)
                    {
                        var order = int.Parse(scriptNameMatch.Groups[1].Value);

                        var filePath = Path.Combine(funcViewStoredProcedureTriggerPath, scriptName);
                        var content = GetFileContent(filePath);

                        var contentMatch = Regex.Match(content, Script.bla, RegexOptions.IgnoreCase);

                        if (!contentMatch.Success)
                            throw new Exception($"Could not match content with {Script.bla}");

                        var type = scriptNameMatch.Groups[1].Value;
                        var name = scriptNameMatch.Groups[2].Value;

                        var script = feature.AddFuncViewStoredProcedureTriggerScript(scriptName, type, name, order);
                        
                        script.SQL = content;
                        script.Checksum = GetFileChecksum(filePath);
                    }
                    else
                    {
                        throw new Exception($"file {scriptName} doesn't match the expected regex format: {Script.FuncViewStoredProcedureTrigger_FILENAME_REGEX}");
                    }
                }
            }
        }

        private DirectoryInfo GetVersionDirectory(DBVersion version)
        {
            return new DirectoryInfo(Path.Combine(_executingDirectory.FullName, version.Name));
        }

        private string GetFeaturePath(Feature feature)
        {
            var versionFolderPath = Path.Combine(_executingDirectory.FullName, feature.Version.Name);
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

        private Script FindRollback(UpgradeScript upgradeScript)
        {
            var match = Regex.Match(upgradeScript.FileName, Script.MIGRATIONS_UPGRADE_FILENAME_REGEX);

            var rollbackFileName = $"{match.Groups[1]}_rollback_{match.Groups[2]}.sql";

            var filePath = Path.Combine(GetFeaturePath(upgradeScript.Feature), "Migrations", rollbackFileName);
            if (System.IO.File.Exists(filePath))
            {
                var rollbackScript =  new DowngradeScript(rollbackFileName, upgradeScript.Order, upgradeScript.Feature);
                rollbackScript.RollbackScript = upgradeScript;
                rollbackScript.SQL = GetFileContent(filePath);
                rollbackScript.Checksum = GetFileChecksum(filePath);
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
