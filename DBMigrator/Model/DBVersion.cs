﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DBMigrator.Model
{
    public class DBVersion : IComparable<DBVersion>
    {
        public int ID { get; set; }
        public string Name { get { return Version.ToString(); } }
        public Version Version { get; }
        public string TriggerChecksum { get; set; }
        public string FunctionsChecksum { get; set; }
        public string StoredProceduresChecksum { get; set; }
        public string TablesViewsAndColumnsChecksum { get; set; }
        private List<Feature> _features { get; } = new List<Feature>();
        public IReadOnlyCollection<Feature> Features {
            get { return _features.AsReadOnly(); }
        }

        public DBVersion(string version)
        {
            try
            {
                Version = new Version(version);
            }
            catch
            {
                throw new ArgumentException($"Could not convert '{version}' to a Version", version);
            }
            if (!version.Equals(Version.ToString()))
            {
                throw new ArgumentException($"Could not convert '{version}' to a matching Version object, got '{Version}'\nNote that DBMigrator doesn't support leading zeroes", version);
            }
        }

        public Feature AddAndOrGetFeature(string featureName)
        {
            var feature = Features.FirstOrDefault(f => f.Name == featureName);
            if (feature == null)
            {
                feature = new Feature(featureName, this);
                _features.Add(feature);
            }
            return feature;
        }
        
        public int CompareTo(DBVersion obj)
        {
            return Version.CompareTo(obj.Version);
        }
    }
}
