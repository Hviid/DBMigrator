using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DBMigrator.Model
{
    public class Feature
    {
        public const string ORDERED_FEATURENAME_REGEX = @"(\d+)_(\w+)|(\w+)";

        public static (int, string) GetFeatureNameAndOrder(string featurename)
        {
            var match = Regex.Match(featurename, Feature.ORDERED_FEATURENAME_REGEX);

            if (match.Success)
            {
                if (!String.IsNullOrEmpty(match.Groups[1].Value))
                {
                    var order = int.Parse(match.Groups[1].Value);
                    var featurename1 = match.Groups[2].Value;
                    return (order, featurename1);
                } else
                {
                    var featurename2 = match.Groups[3].Value;
                    return (0, featurename2);
                }
            }
            throw new ArgumentOutOfRangeException(nameof(featurename), featurename, $"Argument {nameof(featurename)} does not adhere to regex");
        }

        public int Order { get; }
        public string Name { get; }
        private List<UpgradeScript> _upgradeScripts { get; } = new List<UpgradeScript>();
        public IReadOnlyList<UpgradeScript> UpgradeScripts
        {
            get { return _upgradeScripts.AsReadOnly(); }
        }

        public List<DowngradeScript> RollbackScripts {
            get {
                return UpgradeScripts.OrderByDescending(u => u.Order).Select(u => u.RollbackScript).ToList();
            }
        }
        
        public DBVersion Version { get; }

        public Feature(string featureName, DBVersion version, int order)
        {
            Name = featureName;
            Version = version;
        }

        public UpgradeScript AddUpgradeScript(string ScriptFile, int order)
        {
            if (UpgradeScripts.Select(u => u.Order).Contains(order))
                throw new Exception($"Could not add script {ScriptFile}, a script with {order} already exists");

            var script = new UpgradeScript(ScriptFile, order, this);

            _upgradeScripts.Add(script);
            return script;
        }
    }
}
