using System;
using System.Collections.Generic;
using System.Text;

namespace DBMigrator.SQL
{
    static class DatabaseInfo
    {
        static string GetObjectsChangedSince(DateTime dateTime)
        {
            return "SELECT [name] " +
                ",[object_id] " +
                ",[principal_id] " +
                ",[schema_id] " +
                ",[parent_object_id] " +
                ",[type] " +
                ",[type_desc] " +
                ",[create_date] " +
                ",[modify_date] " +
                ",[is_ms_shipped] " +
                ",[is_published] ,[is_schema_published] " +
                "FROM [frapp_dev].[sys].[objects] " +
                "WHERE is_ms_shipped = 0 " +
                $"AND modify_date > {dateTime.ToString()}";
        }
    }
}
