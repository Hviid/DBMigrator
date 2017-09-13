using System;
using System.Collections.Generic;
using System.Text;

namespace DBMigrator.SQL
{
    static class ChecksumScripts
    {
        public static string TablesViewsAndColumnsChecksumScript
        {
            get => "SELECT TABLE_SCHEMA, " +
                "DATA_TYPE, " +
                "TABLE_NAME, " +
                "COLUMN_NAME, " +
                "NUMERIC_PRECISION, " +
                "DATETIME_PRECISION, " +
                "CHARACTER_MAXIMUM_LENGTH FROM INFORMATION_SCHEMA.COLUMNS";
        }

        public static string StoredProceduresChecksum
        {
            get => "SELECT" +
                " [SPECIFIC_CATALOG]" +
                ", [SPECIFIC_SCHEMA]" +
                ", [SPECIFIC_NAME]" +
                ", [ROUTINE_CATALOG]" +
                ", [ROUTINE_SCHEMA]" +
                ", [ROUTINE_NAME]" +
                ", [ROUTINE_TYPE]" +
                ", [DATA_TYPE]" +
                ", [CHARACTER_MAXIMUM_LENGTH]" +
                ", [CHARACTER_OCTET_LENGTH]" +
                ", [NUMERIC_PRECISION]" +
                ", [DATETIME_PRECISION]" +
                ", [ROUTINE_BODY]" +
                ", [ROUTINE_DEFINITION]" +
                ", [IS_DETERMINISTIC]" +
                ", [SQL_DATA_ACCESS]" +
                ", [IS_NULL_CALL] FROM [INFORMATION_SCHEMA].[ROUTINES]" +
                " WHERE ROUTINE_TYPE = 'PROCEDURE'";
        }

        public static string FunctionsChecksum
        {
            get => "SELECT" +
                " [SPECIFIC_CATALOG]" +
                ", [SPECIFIC_SCHEMA]" +
                ", [SPECIFIC_NAME]" +
                ", [ROUTINE_CATALOG]" +
                ", [ROUTINE_SCHEMA]" +
                ", [ROUTINE_NAME]" +
                ", [ROUTINE_TYPE]" +
                ", [DATA_TYPE]" +
                ", [CHARACTER_MAXIMUM_LENGTH]" +
                ", [CHARACTER_OCTET_LENGTH]" +
                ", [NUMERIC_PRECISION]" +
                ", [DATETIME_PRECISION]" +
                ", [ROUTINE_BODY]" +
                ", [ROUTINE_DEFINITION]" +
                ", [IS_DETERMINISTIC]" +
                ", [SQL_DATA_ACCESS]" +
                ", [IS_NULL_CALL] FROM[INFORMATION_SCHEMA].[ROUTINES]" +
                " WHERE ROUTINE_TYPE = 'FUNCTION'";
        }
        public static string TriggersChecksum
        {
            get => "SELECT" +
                " [name]" +
                ", [sys].[all_objects].[object_id]" +
                ", [principal_id]" +
                ", [schema_id]" +
                ", [parent_object_id]" +
                ", [type]" +
                ", [type_desc]" +
                ", [is_ms_shipped]" +
                ", [is_published]" +
                ", [is_schema_published]" +
                ", [definition] FROM[sys].[all_objects]" +
                " INNER JOIN[sys].[sql_modules]" +
                " ON[sys].[sql_modules].[object_id] = [sys].[all_objects].[object_id]" +
                " WHERE type = 'TR'";
        }
        public static string IndexesChecksum
        {
            get => "SELECT" +
                " [CONSTRAINT_CATALOG]" +
                ", [CONSTRAINT_SCHEMA]" +
                ", [CONSTRAINT_NAME]" +
                ", [TABLE_CATALOG]" +
                ", [TABLE_SCHEMA]" +
                ", [TABLE_NAME]" +
                ", [CONSTRAINT_TYPE]" +
                ", [IS_DEFERRABLE]" +
                ", [INITIALLY_DEFERRED]" +
                " FROM[INFORMATION_SCHEMA].[TABLE_CONSTRAINTS]";
        }

        //http://www.bidn.com/blogs/TomLannen/bidn-blog/2265/using-hashbytes-to-compare-columns
        public static string GetHashbytesFor(string query)
        {
            return $@"SELECT DBMIGRATOR_BIG_HASHBYTES(({query} FOR XML AUTO))";
        }

        public static string CustomHashbytesFunction
        {
            get => "CREATE FUNCTION DBMIGRATOR_BIG_HASHBYTES " +
                "(" +
                    "@string varchar(max)" +
                ")" +
                "RETURNS varbinary(max)" +
                "AS" +
                "BEGIN" +
                    "DECLARE @size int = (SELECT DATALENGTH(@string))" +
                    "DECLARE @taken int = 0" +
                    "DECLARE @strToConvert varchar(max)" +
                    "DECLARE @chunkSize int = 4000" +
                    "DECLARE @result varbinary(max)" +
                    "WHILE @size > @taken" +
                    "BEGIN" +
                        "SET @strToConvert = (SELECT SUBSTRING(@string, @taken, @chunkSize))" +
                        "SET @result = (SELECT HASHBYTES('sha1', CONCAT(@strToConvert, CONVERT(varchar(max), @result))))" +
                        "SET @taken = @taken + @chunkSize" +
                    "END" +
                    "RETURN @result" +
                "END";
        }
    }
}
