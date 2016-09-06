using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace DBMigrator.Model
{
    public class Script
    {
        public const string MIGRATIONS_UPGRADE_FILENAME_REGEX = @"(\d+)_(?!rollback)(\w+)";
        public const string MIGRATIONS_ROLLBACK_FILENAME_REGEX = @"(\d+)_(rollback)(\w+)";
        public const string CREATE_STORED_PROCEDURE_REGEX = @"CREATE\s+PROCEDURE\s+(\w+)";
        public const string CREATE_FUNCTIONS_REGEX = @"CREATE\s+FUNCTION\s+(\w+)";
        public const string CREATE_TRIGGERS_REGEX = @"CREATE\s+TRIGGER\s+(\w+)";
        public const string CREATE_VIEWS_REGEX = @"CREATE\s+VIEW\s+(\w+)";
        public const string ILLEGAL_REGEX = @"ALTER";

        private List<SQLTYPE> _migrationTypes = new List<SQLTYPE> { SQLTYPE.Upgrade, SQLTYPE.Rollback };

        public enum SQLTYPE { Upgrade, Rollback, View, Function, StoredProcedure, Trigger };

        public Feature Feature { get; }

        public Script(string ScriptFile, int order, SQLTYPE type, Feature feature)
        {
            FileName = ScriptFile;
            Feature = feature;
            Order = order;

            if (!_migrationTypes.Contains(type))
            {
                if (Regex.IsMatch(SQL, ILLEGAL_REGEX)) throw new Exception($"Not allowed to have ALTER in {type.ToString()} files");
            }
        }


        public int Order { get; }
        public string FileName { get; }
        public SQLTYPE Type { get; }
        public string Checksum { get; set; }
        public int ExecutionTime { get; set; }
        public Script RollbackScript { get; private set; }
        public string SQL { get; set; }
    }
}






//SELECT
//CHECKSUM_AGG(CHECKSUM
//(TABLE_SCHEMA
//, DATA_TYPE
//, TABLE_NAME
//, COLUMN_NAME
//, NUMERIC_PRECISION
//, DATETIME_PRECISION
//, CHARACTER_MAXIMUM_LENGTH)) as TablesViewsAndColumnsChecksum
// FROM INFORMATION_SCHEMA.COLUMNS

// SELECT
//CHECKSUM_AGG(CHECKSUM
//([SPECIFIC_CATALOG]
//      , [SPECIFIC_SCHEMA]
//      , [SPECIFIC_NAME]
//      , [ROUTINE_CATALOG]
//      , [ROUTINE_SCHEMA]
//      , [ROUTINE_NAME]
//      , [ROUTINE_TYPE]
//      , [DATA_TYPE]
//      , [CHARACTER_MAXIMUM_LENGTH]
//      , [CHARACTER_OCTET_LENGTH]
//      , [NUMERIC_PRECISION]
//      , [DATETIME_PRECISION]
//      , [ROUTINE_BODY]
//      , [ROUTINE_DEFINITION]
//      , [IS_DETERMINISTIC]
//      , [SQL_DATA_ACCESS]
//      , [IS_NULL_CALL])) as StoredProceduresChecksum
// FROM[amphi].[INFORMATION_SCHEMA].[ROUTINES]
//WHERE ROUTINE_TYPE = 'PROCEDURE'

//SELECT
//CHECKSUM_AGG(CHECKSUM
//([SPECIFIC_CATALOG]
//      , [SPECIFIC_SCHEMA]
//      , [SPECIFIC_NAME]
//      , [ROUTINE_CATALOG]
//      , [ROUTINE_SCHEMA]
//      , [ROUTINE_NAME]
//      , [ROUTINE_TYPE]
//      , [DATA_TYPE]
//      , [CHARACTER_MAXIMUM_LENGTH]
//      , [CHARACTER_OCTET_LENGTH]
//      , [NUMERIC_PRECISION]
//      , [DATETIME_PRECISION]
//      , [ROUTINE_BODY]
//      , [ROUTINE_DEFINITION]
//      , [IS_DETERMINISTIC]
//      , [SQL_DATA_ACCESS]
//      , [IS_NULL_CALL])) as FunctionsChecksum
// FROM[amphi].[INFORMATION_SCHEMA].[ROUTINES]
//WHERE ROUTINE_TYPE = 'FUNCTION'

//SELECT
//CHECKSUM_AGG(CHECKSUM
//([name]
//      , [sys].[all_objects].[object_id]
//      , [principal_id]
//      , [schema_id]
//      , [parent_object_id]
//      , [type]
//      , [type_desc]
//      , [is_ms_shipped]
//      , [is_published]
//      , [is_schema_published]

//      , [definition])) as TriggersChecksum
//FROM[sys].[all_objects]
//INNER JOIN[sys].[sql_modules]
//ON[sys].[sql_modules].[object_id] = [sys].[all_objects].[object_id]
//WHERE type = 'TR'

