using System;
using System.Collections.Generic;
using System.Text;

namespace DBMigrator.SQL
{
    class ChecksumScripts
    {
        public string TablesViewsAndColumnsChecksumScript
        {
            get => @"SELECT TABLE_SCHEMA, 
                        DATA_TYPE, 
                        TABLE_NAME, 
                        COLUMN_NAME, 
                        NUMERIC_PRECISION, 
                        DATETIME_PRECISION, 
                        CHARACTER_MAXIMUM_LENGTH FROM INFORMATION_SCHEMA.COLUMNS";
        }

        public string StoredProceduresChecksum
        {
            get => @"SELECT TABLE_SCHEMA, 
                        DATA_TYPE, 
                        TABLE_NAME, 
                        COLUMN_NAME, 
                        NUMERIC_PRECISION, 
                        DATETIME_PRECISION, 
                        CHARACTER_MAXIMUM_LENGTH FROM INFORMATION_SCHEMA.COLUMNS";
        }

        public string FunctionsChecksum
        {
            get => @"SELECT TABLE_SCHEMA, 
                        DATA_TYPE, 
                        TABLE_NAME, 
                        COLUMN_NAME, 
                        NUMERIC_PRECISION, 
                        DATETIME_PRECISION, 
                        CHARACTER_MAXIMUM_LENGTH FROM INFORMATION_SCHEMA.COLUMNS";
        }
        public string TriggersChecksum
        {
            get => @"SELECT TABLE_SCHEMA, 
                        DATA_TYPE, 
                        TABLE_NAME, 
                        COLUMN_NAME, 
                        NUMERIC_PRECISION, 
                        DATETIME_PRECISION, 
                        CHARACTER_MAXIMUM_LENGTH FROM INFORMATION_SCHEMA.COLUMNS";
        }
        public string IndexesChecksum
        {
            get => @"SELECT TABLE_SCHEMA, 
                        DATA_TYPE, 
                        TABLE_NAME, 
                        COLUMN_NAME, 
                        NUMERIC_PRECISION, 
                        DATETIME_PRECISION, 
                        CHARACTER_MAXIMUM_LENGTH FROM INFORMATION_SCHEMA.COLUMNS";
        }

        //http://www.bidn.com/blogs/TomLannen/bidn-blog/2265/using-hashbytes-to-compare-columns
        public string GetHashbytesFor(string query)
        {
            return $@"DECLARE @xml nvarchar(MAX) = ({query} FOR XML AUTO); SELECT HASHBYTES('SHA1', @xml)";
        }
    }
}
