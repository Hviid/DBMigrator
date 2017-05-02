using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DBMigrator.Model
{
    public class FuncViewStoredProcedureTriggerScript : Script
    {
        public string Type { get; }
        public string Name { get; }

        public FuncViewStoredProcedureTriggerScript(string fileName, string type, string name, int order, Feature feature) : base(fileName, order, feature)
        {
            Type = type;
            Name = name;
        }
    }
}
