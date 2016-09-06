using DBMigrator.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DBMigrator.Test.Comparers
{
    public class FeatureComparer : IEqualityComparer<Feature>
    {
        public bool Equals(Feature x, Feature y)
        {
            var result = x.Name == y.Name;
            result = result && x.UpgradeScripts.SequenceEqual(y.UpgradeScripts, new ScriptComparer());
            return result;
        }

        public int GetHashCode(Feature obj)
        {
            var scriptComparer = new ScriptComparer();
            var result = obj.Name.GetHashCode();
            foreach (var script in obj.UpgradeScripts)
            {
                result = result ^ scriptComparer.GetHashCode(script);
            }
            return result;
        }
    }
}
