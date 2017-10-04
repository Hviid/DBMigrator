using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace DBMigrator.Model
{
    public class Script
    {
        public const string ORDERED_FILENAME_REGEX = @"(\d+)_(\w+)";
        public const string MIGRATIONS_UPGRADE_FILENAME_REGEX = @"^(\d+)_(?!rollback)(\w+)";
        public const string MIGRATIONS_ROLLBACK_FILENAME_REGEX = @"^(\d+)_(rollback)(\w+)";
        public const string CREATE_STORED_PROCEDURE_REGEX = @"CREATE\s+PROCEDURE\s+(\w+)";
        public const string CREATE_FUNCTIONS_REGEX = @"CREATE\s+FUNCTION\s+(\w+)";
        public const string CREATE_TRIGGERS_REGEX = @"CREATE\s+TRIGGER\s+(\w+)";
        public const string CREATE_VIEWS_REGEX = @"CREATE\s+VIEW\s+(\w+)";
        public const string ILLEGAL_REGEX = @"ALTER";
        public const string FUNC_PROCEDURE_VIEW_TRIGGER_REGEX = @"CREATE (FUNCTION|PROCEDURE|VIEW|TRIGGER) (\w+)";
        
        public Feature Feature { get; }

        public int Order { get; }

        public Script(string fileName, int order, Feature feature)
        {
            FileName = fileName;
            Feature = feature;
            Order = order;
        }
        
        public string FileName { get; }
        public DowngradeScript RollbackScript { get; set; }
        public string SQL { get; set; }
        
    }
}







