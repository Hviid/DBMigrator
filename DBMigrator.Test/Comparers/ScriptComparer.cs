using DBMigrator.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DBMigrator.Test.Comparers
{
    public class ScriptComparer : IEqualityComparer<UpgradeScript>
    {
        public bool Equals(UpgradeScript x, UpgradeScript y)
        {
            var result = x.FileName == y.FileName;
            result = result && x.Order == y.Order;
            result = result && x.Checksum == y.Checksum;
            //result = result && x.Type == y.Type;
            return result;
        }

        public int GetHashCode(UpgradeScript obj)
        {
            var result = obj.FileName.GetHashCode();
            result = result ^ obj.Order.GetHashCode();
            result = result ^ obj.Checksum.GetHashCode();
            //result = result ^ obj.Type.GetHashCode();
            return result;
        }
    }
}
