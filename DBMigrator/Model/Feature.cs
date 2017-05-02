using System;
using System.Collections.Generic;
using System.Linq;

namespace DBMigrator.Model
{
    public class Feature
    {
        public string Name { get; }
        private List<UpgradeScript> _upgradeScripts { get; } = new List<UpgradeScript>();
        public IReadOnlyList<UpgradeScript> UpgradeScripts
        {
            get { return _upgradeScripts.AsReadOnly(); }
        }

        private List<FuncViewStoredProcedureTriggerScript> _funcsSPsViewsTriggersScripts { get; } = new List<FuncViewStoredProcedureTriggerScript>();
        public IReadOnlyList<FuncViewStoredProcedureTriggerScript> FuncsSPsViewsTriggersScripts
        {
            get { return _funcsSPsViewsTriggersScripts.AsReadOnly(); }
        }

        private List<DataScript> _dataScripts { get; } = new List<DataScript>();
        public IReadOnlyList<DataScript> DataScripts
        {
            get { return _dataScripts.AsReadOnly(); }
        }

        public List<DowngradeScript> RollbackScripts {
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

        public UpgradeScript AddUpgradeScript(string ScriptFile, int order)
        {
            if (UpgradeScripts.Select(u => u.Order).Contains(order))
                throw new Exception($"Could not add script {ScriptFile}, a script with {order} already exists");

            var script = new UpgradeScript(ScriptFile, order, this);

            _upgradeScripts.Add(script);
            return script;
        }

        public FuncViewStoredProcedureTriggerScript AddFuncViewStoredProcedureTriggerScript(string fileName, string type, string name, int order)
        {
            if (FuncsSPsViewsTriggersScripts.Select(u => u.Order).Contains(order))
                throw new Exception($"Could not add script {fileName}, a script with {order} already exists");

            var script = new FuncViewStoredProcedureTriggerScript(fileName, type, name, order, this);

            _funcsSPsViewsTriggersScripts.Add(script);
            return script;
        }

        public DataScript AddDataScript(string scriptName, int order)
        {
            if (DataScripts.Select(u => u.Order).Contains(order))
                throw new Exception($"Could not add script {scriptName}, a script with {order} already exists");

            var script = new DataScript(scriptName, order, this);

            _dataScripts.Add(script);
            return script;
        }
    }
}
