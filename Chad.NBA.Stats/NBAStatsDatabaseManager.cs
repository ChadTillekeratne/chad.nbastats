using System;
using System.Collections.Generic;
using System.Text;

using System.Data.SqlClient;
using System.Data;

namespace Chad.NBA.Stats
{
    public class NBAStatsDatabaseManager : IDisposable
    {
        private SqlConnection _sqlCon;


        public NBAStatsDatabaseManager(String sqlConnectionString)
        {
            ConnectToDatabase(sqlConnectionString);

        }

        #region SQL Connection

        private void ConnectToDatabase(String sqlConnectionString)
        {
            _sqlCon = new SqlConnection(sqlConnectionString);

            _sqlCon.Open();
        }

        #endregion

        #region Insert Data

        public void InsertData(DataSet ds, List<String> onlyInsertTableNames = null)
        {
            foreach(DataTable dt in ds.Tables)
            {
                // Skip the table id not in the list
                if (onlyInsertTableNames != null)
                {
                    if (!onlyInsertTableNames.Contains(dt.TableName))
                        continue;
                }

                InsertData(dt);
            }
        }

        public void InsertData(DataTable dt)
        {
            String tableName = dt.TableName;

            // Create the table if it doesn't exist
            if(!SqlSchemaUpdaterHelper.DatabaseObjectExists(tableName,_sqlCon))
            {
                SqlSchemaUpdaterHelper.CreateTable(_sqlCon, dt, null, tableName);
            }

            // Insert the data
            using (SqlBulkCopy bulk = new SqlBulkCopy(_sqlCon))
            {
                bulk.DestinationTableName = tableName;

                bulk.WriteToServer(dt);
            }

        }

        #endregion

        public void Dispose()
        {
            
        }
    }
}
