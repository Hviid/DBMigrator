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
        }

        public Feature AddFeature(string featureName)
        {
            var feature = Features.FirstOrDefault(f => f.Name == featureName);
            if (feature == null)
            {
                feature = new Feature(featureName, this);
                _features.Add(feature);
            }
            return feature;
        }

        public void AddOrUpdateFeature(string featureName, Script script)
        {
            var feature = AddFeature(featureName);
            if (script != null)
                feature.AddScript(script);
        }
        
        public int CompareTo(DBVersion obj)
        {
            return Version.CompareTo(obj.Version);
        }
    }
}
