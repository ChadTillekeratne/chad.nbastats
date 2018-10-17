using System;
using System.Collections.Generic;
using System.Text;

using System.Data;
using System.Data.SqlClient;

namespace Chad.NBA.Stats
{
    public class SqlSchemaUpdaterHelper
    {
        public static void CreateTable(SqlConnection sqlCon, DataTable sourceTable, String identityColumnName, String tableName, bool createRowIdentityColumn = false)
        {
            try
            {
                StringBuilder s = new System.Text.StringBuilder();

                s.Append("CREATE TABLE [" + tableName + "] (");

                // Add row identity column (if enabled)
                if (createRowIdentityColumn)
                    s.Append(" [" + tableName + "Id] [UNIQUEIDENTIFIER] ROWGUIDCOL NOT NULL, ");

                if (!String.IsNullOrEmpty(identityColumnName))
                {
                    s.Append(" " + identityColumnName + " [UNIQUEIDENTIFIER] NOT NULL,");
                }

                // Add each column
                if (sourceTable.Columns.Count > 0)
                {
                    foreach (DataColumn col in sourceTable.Columns)
                    {
                        s.Append("[" + col.ColumnName + "] " + GetDatabaseDataType(col.DataType) + " NULL, ");
                    }
                }

                if (!String.IsNullOrEmpty(identityColumnName))
                {
                    s.Append(" CONSTRAINT [PK_" + tableName + "] PRIMARY KEY CLUSTERED ( ");
                    s.Append(" [" + tableName + "Id] ASC ) ");
                }

                s.Append(" )");

                SqlCommand cmd = new SqlCommand(s.ToString(), sqlCon);

                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                //
                Console.WriteLine(ex.ToString());
            }
        }

        public static Boolean DatabaseObjectExists(String objectName, SqlConnection sqlCon)
        {
            SqlCommand cmd = new SqlCommand("SELECT * FROM sys.all_objects WHERE name = '" + objectName + "' ", sqlCon);
            return cmd.ExecuteScalar() != null;
        }

        protected static String GetDatabaseDataType(Type t)
        {
            // Data is all string 


            switch (t.ToString())
            {
                case "System.DateTime":
                    return "DateTime";
                case "System.Int64":
                    return "int";
                case "System.Double":
                    return "float";
                case "System.Boolean":
                case "System.String":
                default:
                    return "VARCHAR(MAX)";
            }

        }

        protected static String CleanString(String s)
        {
            return s;
        }

    }
}
