using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DBMigrator.Model
{
    public class Feature
    {
        public DirectoryInfo Directory { get; set; }
        public string Name
        {
            get { return Directory.Name; }
        }
        public List<Script> UpgradeScripts { get; set; } = new List<Script>();
        public List<Script> FuncsSPsViewsTriggers { get; set; } = new List<Script>();
        public DBVersion Version { get; set; }

        public Feature(DirectoryInfo directory, DBVersion version)
        {
            Directory = directory;
            Version = version;
            
        }

        public void AddScript(Script script)
        {
            if (UpgradeScripts.Select(u => u.Order).Contains(script.Order))
                throw new Exception($"Could not add script {script.Name}, a script with {script.Order} already exists");

            UpgradeScripts.Add(script);
        }
    }
}
