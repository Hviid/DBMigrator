using DBMigrator.Model;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DBMigrator
{
    public class VersionDiff
    {
        public VersionDiff()
        {
        }

        public string UpgradeDiffText(List<DBVersion> diff)
        {
            var str = "";
            foreach (var version in diff)
            {
                str += $"version: {version.Name} \n";
                foreach (var feature in version.Features)
                {
                    str += $"--feature: {feature.Name} \n";
                    str += $"---Upgrades: \n";
                    foreach (var script in feature.UpgradeScripts)
                    {
                        str += $"----script: {script.FileName} \n";
                    }
                }
            }
            return str;
        }

        public string DowngradeDiffText(List<DBVersion> diff)
        {
            var str = "";
            foreach (var version in diff)
            {
                str += $"version: {version.Name} \n";
                foreach (var feature in version.Features)
                {
                    str += $"--feature: {feature.Name} \n";
                    str += $"---Rollbacks: \n";
                    foreach (var script in feature.UpgradeScripts)
                    {
                        str += $"----script: {script.RollbackScript.FileName} \n";
                    }
                }
            }
            return str;
        }

        /// <summary>
        /// Creates a diff of migrations found in source, but not in target
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public List<DBVersion> Diff(List<DBVersion> source, List<DBVersion> target)
        {
            var diff = new List<DBVersion>();
            foreach (var sourceVersion in source)
            {
                var targetVersion = target.SingleOrDefault(t => t.Name == sourceVersion.Name);
                if (targetVersion == null) {
                    diff.Add(sourceVersion);
                    continue;
                } 

                foreach (var sourceFeature in sourceVersion.Features)
                {
                    var targetFeature = targetVersion.Features.SingleOrDefault(s => s.Name == sourceFeature.Name);
                    if (targetFeature == null)
                    {
                        var diffVersion = diff.SingleOrDefault(t => t.Name == targetVersion.Name);
                        if(diffVersion == null)
                        {
                            diffVersion = new DBVersion(targetVersion.Name);
                            diff.Add(diffVersion);
                        }
                        CopyFeatureToVersion(sourceFeature, diffVersion);
                        continue;
                    }

                    foreach (var sourceUpgradeScript in sourceFeature.UpgradeScripts)
                    {
                        var targetScript = targetFeature.UpgradeScripts.SingleOrDefault(s => s.FileName == sourceUpgradeScript.FileName);
                        if (targetScript == null)
                        {
                            var diffVersion = diff.SingleOrDefault(t => t.Name == targetVersion.Name);
                            if (diffVersion == null)
                            {
                                diffVersion = new DBVersion(targetVersion.Name);
                                diff.Add(diffVersion);
                            }
                            

                            var diffFeature = diffVersion.Features.SingleOrDefault(t => t.Name == targetFeature.Name);
                            if (diffFeature == null)
                            {
                                diffFeature = diffVersion.AddAndOrGetFeature(targetFeature.Name, targetFeature.Order);
                            }
                            CopyUpgradeScriptToFeature(sourceUpgradeScript, diffFeature);
                        }
                    }
                }
            }
            return diff;
        }

        private void CopyFeatureToVersion(Feature sourceFeature, DBVersion target)
        {
            var feature = target.AddAndOrGetFeature(sourceFeature.Name, sourceFeature.Order);
            foreach (var script in sourceFeature.UpgradeScripts)
            {
                CopyUpgradeScriptToFeature(script, feature);
            }
        }

        private void CopyUpgradeScriptToFeature(UpgradeScript sourceScript, Feature target)
        {
            var script = target.AddUpgradeScript(sourceScript.FileName, sourceScript.Order);
            script.SQL = sourceScript.SQL;
            script.Checksum = sourceScript.Checksum;
        }
    }
}
