using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DBMigrator.Model
{
    public class DBVersion : IComparable<DBVersion>
    {
        public int ID { get; set; }
        public DirectoryInfo Directory { get; set; }
        public string Name
        {
            get { return Directory.Name; }
        }
        public Version Version { get; set; }
        public List<Feature> Features { get; set; } = new List<Feature>();

        public DBVersion(DirectoryInfo directory)
        {
            Directory = directory;

            try
            {
                Version = new Version(directory.Name);
            }
            catch
            {
                throw new ArgumentException($"Could not convert '{directory.Name}' to a Version", directory.Name);
            }
        }

        public void AddOrUpdateFeature(string featureName, Script script)
        {
            var feature = Features.FirstOrDefault(f => f.Name == featureName);
            if(feature == null)
            {
                feature = new Feature(new DirectoryInfo(Path.Combine(Directory.FullName, featureName)), this);
                Features.Add(feature);
            }
            feature.AddScript(script);
        }
        
        public int CompareTo(DBVersion obj)
        {
            return Version.CompareTo(obj.Version);
        }
    }
}
