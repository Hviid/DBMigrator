using DBMigrator.Model;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DBMigrator
{
    public class VersionDiff
    {
        private string executingPath;
        public VersionDiff()
        {
            executingPath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
        }

        public string DiffText(List<DBVersion> diff)
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
                    str += $"---FuncsSPsViewsTriggers: \n";
                    foreach (var script in feature.FuncsSPsViewsTriggersScripts)
                    {
                        str += $"----script: {script.FileName} \n";
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
                                diffFeature = diffVersion.AddAndOrGetFeature(targetFeature.Name);
                            }
                            CopyUpgradeScriptToFeature(sourceUpgradeScript, diffFeature);
                        }
                    }

                    foreach (var sourceFuncsSPsViewsTriggersScript in sourceFeature.FuncsSPsViewsTriggersScripts)
                    {
                        var targetScript = targetFeature.FuncsSPsViewsTriggersScripts.SingleOrDefault(s => s.FileName == sourceFuncsSPsViewsTriggersScript.FileName);
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
                                diffVersion.AddAndOrGetFeature(targetFeature.Name);
                            }
                            CopyFuncsSPsViewsTriggersScriptToFeature(sourceFuncsSPsViewsTriggersScript, diffFeature);
                        }
                    }
                }
            }
            return diff;
        }

        private void CopyFeatureToVersion(Feature sourceFeature, DBVersion target)
        {
            var feature = target.AddAndOrGetFeature(sourceFeature.Name);
            foreach (var script in sourceFeature.UpgradeScripts)
            {
                CopyUpgradeScriptToFeature(script, feature);
            }

            foreach (var script in sourceFeature.FuncsSPsViewsTriggersScripts)
            {
                CopyFuncsSPsViewsTriggersScriptToFeature(script, feature);
            }

        }

        private void CopyUpgradeScriptToFeature(UpgradeScript sourceScript, Feature target)
        {
            var script = target.AddUpgradeScript(sourceScript.FileName, sourceScript.Order);
            script.SQL = sourceScript.SQL;
        }

        private void CopyFuncsSPsViewsTriggersScriptToFeature(FuncViewStoredProcedureTriggerScript sourceScript, Feature target)
        {
            var script = target.AddFuncViewStoredProcedureTriggerScript(sourceScript.FileName, sourceScript.Type, sourceScript.Name, sourceScript.Order);
            script.SQL = sourceScript.SQL;
        }
    }
}
