using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace CommonEFUtils {
    public static class SqlUtils {

        private static readonly string UserDefinedTableTypeSchemaSQLFormatString = GetUserDefinedTableTypeSchemaSQLFormatString();

        /// <summary>
        /// Executes a stored procedure and returns the result set.
        /// </summary>
        /// <param name="connection">Connection to use for this call</param>
        /// <param name="hshQueryParams">Hashtable containing query parameters as key/value pairs</param>
        /// <param name="strProcName">Name of the query</param>
        /// <param name="retTable">Contains the returned result set</param>
        /// <returns>true on success, false on failure</returns>
        public static bool TryGetSqlData(SqlConnection connection, Hashtable hshQueryParams, string strProcName, ref DataTable retTable, CommandType cmdType, int? commandTimeout = null) { return TryGetSqlData(connection, null, hshQueryParams, strProcName, ref retTable, cmdType, commandTimeout); }

        /// <summary>
        /// Executes a stored procedure and returns the result set.
        /// </summary>
        /// <param name="connection">Connection to use for this call</param>
        /// <param name="hshQueryParams">Hashtable containing query parameters as key/value pairs</param>
        /// <param name="strProcName">Name of the query</param>
        /// <param name="retTable">Contains the returned result set</param>
        /// <returns>true on success, false on failure</returns>
        public static bool TryGetSqlData(SqlConnection connection, SqlTransaction transaction, Hashtable hshQueryParams, string strProcName, ref DataTable retTable, CommandType cmdType, int? commandTimeout = null) {
            bool bOkay = false;
            try {
                retTable = GetSqlData(connection, hshQueryParams, strProcName, cmdType, commandTimeout);
                bOkay = true;
            } catch (Exception) { /*Ignore*/ }
            return bOkay;
        }

        /// <summary>
        /// Executes a stored procedure or SQL query and returns the result set.
        /// </summary>
        /// <param name="connection">Connection to use for this call</param>
        /// <param name="hshQueryParams">Hashtable containing query parameters as key/value pairs</param>
        /// <param name="strProcNameOrQuery">Name of the proc or SQL for the query</param>
        /// <returns>DataTable containing returned data</returns>
        /// <exception cref="System.Exception" />
        public static DataTable GetSqlData(SqlConnection connection, Hashtable hshQueryParams, string strProcNameOrQuery, CommandType cmdType, int? commandTimeout = null) { return GetSqlData(connection, null, hshQueryParams, strProcNameOrQuery, cmdType, commandTimeout); }

        /// <summary>
        /// Executes a stored procedure or SQL query and returns the result set.
        /// </summary>
        /// <param name="connection">Connection to use for this call</param>
        /// <param name="transaction">Transaction to use for this call</param>
        /// <param name="hshQueryParams">Hashtable containing query parameters as key/value pairs</param>
        /// <param name="strProcName">Name of the proc or SQL for the query</param>
        /// <returns>DataTable containing returned data</returns>
        /// <exception cref="System.Exception" />
        public static DataTable GetSqlData(SqlConnection connection, SqlTransaction transaction, Hashtable hshQueryParams, string strProcNameOrQuery, CommandType cmdType, int? commandTimeout = null) {
            DataTable retTable = new DataTable();
            SqlCommand command = new SqlCommand(strProcNameOrQuery, connection);
            if (transaction != null)
                command.Transaction = transaction;
            command.CommandType = cmdType;
            if (commandTimeout.HasValue)
                command.CommandTimeout = commandTimeout.Value;

            if (hshQueryParams != null) {
                foreach (DictionaryEntry entry in hshQueryParams) {
                    command.Parameters.AddWithValue(entry.Key.ToString(), entry.Value.ToString());
                }
            }
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            adapter.Fill(retTable);
            return retTable;
        }

        /// <summary>
        /// Execute a Non-Query/stored Procedure
        /// </summary>
        /// <param name="connection">Connection to use for this call</param>
        /// <param name="hshQueryParams">Hashtable containing query parameters as key/value pairs</param>
        /// <param name="strNonQuery">Name of the query</param>
        /// <param name="strOutputValue">Name of output parameter if any</param>
        /// <param name="outputValue">Contains returned data, default: -1</param>
        /// <returns>true on success, false on failure</returns>
        public static bool TryPostSqlData(SqlConnection connection, Hashtable hshQueryParams, string strNonQuery, CommandType cmdType, int? commandTimeout = null) { return TryPostSqlData(connection, null, hshQueryParams, strNonQuery, cmdType, commandTimeout); }

        /// <summary>
        /// Execute a Non-Query/stored Procedure
        /// </summary>
        /// <param name="connection">Connection to use for this call</param>
        /// <param name="transaction">Transaction to use for this call</param>
        /// <param name="hshQueryParams">Hashtable containing query parameters as key/value pairs</param>
        /// <param name="strNonQuery">Name of the query</param>
        /// <param name="strOutputValue">Name of output parameter if any</param>
        /// <param name="outputValue">Contains returned data, default: -1</param>
        /// <returns>true on success, false on failure</returns>
        public static bool TryPostSqlData(SqlConnection connection, SqlTransaction transaction, Hashtable hshQueryParams, string strNonQuery, CommandType cmdType, int? commandTimeout = null) {
            int outputValue = -1;
            return TryPostSqlData(connection, transaction, hshQueryParams, strNonQuery, null, cmdType, out outputValue, commandTimeout);
        }

        /// <summary>
        /// Execute a Non-Query stored Procedure
        /// </summary>
        /// <param name="connection">Connection to use for this call</param>
        /// <param name="hshQueryParams">Hashtable containing query parameters as key/value pairs</param>
        /// <param name="strNonQuery">Name of the query</param>
        /// <param name="strOutputValue">Name of output parameter if any</param>
        /// <param name="outputValue">Contains returned data, default: -1</param>
        /// <returns>true on success, false on failure</returns>
        public static bool TryPostSqlData(SqlConnection connection, Hashtable hshQueryParams, string strNonQuery, string strOutputValue, CommandType cmdType, out int outputValue, int? commandTimeout = null) { return TryPostSqlData(connection, null, hshQueryParams, strNonQuery, strOutputValue, cmdType, out outputValue, commandTimeout); }

        /// <summary>
        /// Execute a Non-Query stored Procedure
        /// </summary>
        /// <param name="connection">Connection to use for this call</param>
        /// <param name="transaction">Transaction to use for this call</param>
        /// <param name="hshQueryParams">Hashtable containing query parameters as key/value pairs</param>
        /// <param name="strNonQuery">Name of the query</param>
        /// <param name="strOutputValue">Name of output parameter if any</param>
        /// <param name="outputValue">Contains returned data, default: -1</param>
        /// <returns>true on success, false on failure</returns>
        public static bool TryPostSqlData(SqlConnection connection, SqlTransaction transaction, Hashtable hshQueryParams, string strNonQuery, string strOutputValue, CommandType cmdType, out int outputValue, int? commandTimeout = null) {
            bool bOkay = false;
            outputValue = -1;
            try {
                outputValue = PostSqlData(connection, hshQueryParams, strNonQuery, strOutputValue, cmdType, commandTimeout);
                bOkay = true;
            } catch (Exception) { /*Ignore*/ }
            return bOkay;
        }

        /// <summary>
        /// Execute a Non-Query stored Procedure
        /// </summary>
        /// <param name="connection">Connection to use for this call</param>
        /// <param name="hshQueryParams">Hashtable containing query parameters as key/value pairs</param>
        /// <param name="strNonQuery">Name of the query</param>
        /// <param name="strOutputValue">Name of output parameter if any</param>
        /// <returns>int containing returned data, default: -1</returns>
        /// <exception cref="System.Exception" />
        public static int PostSqlData(SqlConnection connection, Hashtable hshQueryParams, string strNonQuery, string strOutputValue, CommandType cmdType, int? commandTimeout = null) { return PostSqlData(connection, null, hshQueryParams, strNonQuery, strOutputValue, cmdType, commandTimeout); }

        /// <summary>
        /// Execute a Non-Query stored Procedure
        /// </summary>
        /// <param name="connection">Connection to use for this call</param>
        /// <param name="transaction">Transaction to use for this call</param>
        /// <param name="hshQueryParams">Hashtable containing query parameters as key/value pairs</param>
        /// <param name="strNonQuery">Name of the query</param>
        /// <param name="strOutputValue">Name of output parameter if any</param>
        /// <returns>int containing returned data, default: -1</returns>
        /// <exception cref="System.Exception" />
        public static int PostSqlData(SqlConnection connection, SqlTransaction transaction, Hashtable hshQueryParams, string strNonQuery, string strOutputValue, CommandType cmdType, int? commandTimeout = null) {
            int returnValue = -1;
            SqlCommand command = new SqlCommand(strNonQuery, connection);
            if (transaction != null)
                command.Transaction = transaction;
            command.CommandType = cmdType;
            if (commandTimeout.HasValue)
                command.CommandTimeout = commandTimeout.Value;

            SqlParameter returnParam = null;
            if (hshQueryParams != null) {
                foreach (DictionaryEntry entry in hshQueryParams) {
                    command.Parameters.AddWithValue(entry.Key.ToString(), entry.Value);
                }
            }
            if (!string.IsNullOrEmpty(strOutputValue)) {
                returnParam = new SqlParameter(strOutputValue, SqlDbType.VarChar, 20);
                returnParam.Direction = ParameterDirection.Output;
                command.Parameters.Add(returnParam);
            }
            try {
                //For transactional processing connection will be opened already
                if (connection.State == ConnectionState.Closed)
                    connection.Open();
                command.ExecuteNonQuery();
                if (command.Parameters.Contains(returnParam))
                    int.TryParse(command.Parameters[returnParam.ParameterName].Value.ToString(), out returnValue);
            } catch (Exception ex) { throw ex; } finally {
                //Do not close connection for transactional processing
                if (command.Transaction == null)
                    connection.Close();
            }
            return returnValue;
        }

        //methods to get properties from DataRow objects
        public delegate void PropertyExceptionHandler(string rowKey, string colName, Exception ex);
        public static bool GetDBBoolean(string rowKey, DataRow row, string columnName, PropertyExceptionHandler exceptionHandler = null) {
            bool retVal = false;
            try { retVal = row[columnName] != DBNull.Value ? Convert.ToBoolean(row[columnName]) : false; } catch (Exception ex) { if (exceptionHandler != null) exceptionHandler.Invoke(rowKey, columnName, ex); }
            return retVal;
        }
        public static decimal GetDBDecimal(string rowKey, DataRow row, string columnName, PropertyExceptionHandler exceptionHandler = null) {
            decimal retVal = 0;
            try { retVal = row[columnName] != DBNull.Value ? (!string.IsNullOrEmpty(row[columnName].ToString()) ? Convert.ToDecimal(row[columnName]) : 0) : 0; } catch (Exception ex) { if (exceptionHandler != null) exceptionHandler.Invoke(rowKey, columnName, ex); }
            return retVal;
        }
        public static string GetDBString(string rowKey, DataRow row, string columnName, PropertyExceptionHandler exceptionHandler = null) {
            string retVal = string.Empty;
            try { retVal = row[columnName] != DBNull.Value ? row[columnName].ToString() : string.Empty; } catch (Exception ex) { if (exceptionHandler != null) exceptionHandler.Invoke(rowKey, columnName, ex); }
            return retVal;
        }
        public static int GetDBInt(string rowKey, DataRow row, string columnName, PropertyExceptionHandler exceptionHandler = null) {
            int retVal = -1;
            try { retVal = row[columnName] != DBNull.Value ? Convert.ToInt32(row[columnName]) : -1; } catch (Exception ex) { if (exceptionHandler != null) exceptionHandler.Invoke(rowKey, columnName, ex); }
            return retVal;
        }
        public static int? GetDBIntNullable(string rowKey, DataRow row, string columnName, PropertyExceptionHandler exceptionHandler = null) {
            int? retVal = null;
            try { retVal = row[columnName] != DBNull.Value ? Convert.ToInt32(row[columnName]) : (int?)null; } catch (Exception ex) { if (exceptionHandler != null) exceptionHandler.Invoke(rowKey, columnName, ex); }
            return retVal;
        }
        public static DateTime GetDBDateTime(string rowKey, DataRow row, string columnName, PropertyExceptionHandler exceptionHandler = null) {
            DateTime dt = DateTime.MinValue;
            try { dt = row[columnName] != DBNull.Value ? Convert.ToDateTime(row[columnName]) : DateTime.MinValue; } catch (Exception ex) { if (exceptionHandler != null) exceptionHandler.Invoke(rowKey, columnName, ex); }
            return dt;
        }
        public static DateTime? GetDBDateTimeNullable(string rowKey, DataRow row, string columnName, PropertyExceptionHandler exceptionHandler = null) {
            DateTime? dt = null;
            try { dt = row[columnName] != DBNull.Value ? Convert.ToDateTime(row[columnName]) : (DateTime?)null; } catch (Exception ex) { if (exceptionHandler != null) exceptionHandler.Invoke(rowKey, columnName, ex); }
            return dt;
        }

        public static DataTable GetUserDefinedTableTypeSchema(SqlConnection connection, string tableTypeName) {
            string sql = string.Format(UserDefinedTableTypeSchemaSQLFormatString, tableTypeName);
            return GetSqlData(connection, null, sql, CommandType.Text);
        }

        private static string GetUserDefinedTableTypeSchemaSQLFormatString() {
            StringBuilder sb = new StringBuilder();
            sb.Append(" DECLARE @TempTable AS {0} ");
            sb.Append(" SELECT * FROM @TempTable ");
            return sb.ToString();
        }
    }
}
