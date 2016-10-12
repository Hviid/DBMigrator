using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static DBMigrator.Model.Script;

namespace DBMigrator.Model
{
    public class Feature
    {
        public string Name { get; }
        private List<Script> _upgradeScripts { get; } = new List<Script>();
        private List<Script> _funcsSPsViewsTriggers { get; } = new List<Script>();
        public IReadOnlyList<Script> UpgradeScripts
        {
            get { return _upgradeScripts.AsReadOnly(); }
        }

        public List<Script> RollbackScripts {
            get {
                return UpgradeScripts.OrderByDescending(u => u.Order).Select(u => u.RollbackScript).ToList();
            }
        }

        
        public DBVersion Version { get; }

        public Feature(string featureName, DBVersion version)
        {
            Name = featureName;
            Version = version;
        }

        public Script AddScript(string ScriptFile, int order, SQLTYPE type)
        {
            if (UpgradeScripts.Select(u => u.Order).Contains(order))
                throw new Exception($"Could not add script {ScriptFile}, a script with {order} already exists");

            var script = new Script(ScriptFile, order, type, this);

            _upgradeScripts.Add(script);
            return script;
        }
    }
}
