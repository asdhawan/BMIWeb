using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using CommonUtils;
using System.Data;
using System.Reflection;

namespace CommonEFUtils {
    public static class DBUtils {
        private enum SqlCommandType { Insert, Update, Delete };

        public delegate Dictionary<string, object> GetSqlParametersDelegate<BO>(BO businessObj) where BO : new();

        //Insert
        public static EO Insert<BO, EO>(BO obj, DbContext context, DbSet<EO> dbSet, List<string> dbColumns = null, bool bCommitOnChange = true)
            where BO : new()
            where EO : class, new() {
            EO entityObj = new EO();
            try {
                if (dbColumns == null)
                    entityObj = BusinessLayerUtils.PopulateEntity<EO, BO>(obj);
                else
                    entityObj = BusinessLayerUtils.PopulateEntity<EO, BO>(obj, dbColumns);
                dbSet.Add(entityObj);
                if (bCommitOnChange)
                    context.SaveChanges();
            } catch (Exception ex) { ThrowException<BO>(context, "Insert()", ex); }
            return entityObj;
        }

        //Update
        public static void Update<BO, EO>(BO obj, DbContext context, DbSet<EO> dbSet, List<string> dbColumns = null, bool bCommitOnChange = true)
            where BO : new()
            where EO : class, new() {
            EO entityObj = new EO();
            try {
                if (dbColumns == null)
                    entityObj = BusinessLayerUtils.PopulateEntity<EO, BO>(obj);
                else
                    entityObj = BusinessLayerUtils.PopulateEntity<EO, BO>(obj, dbColumns);
                context.Entry<EO>(entityObj).State = EntityState.Modified;
                if (bCommitOnChange)
                    context.SaveChanges();
            } catch (Exception ex) { ThrowException<BO>(context, "Update()", ex); }
        }

        //delete
        public static void Delete<BO, EO>(BO obj, DbContext context, DbSet<EO> dbSet, List<string> dbColumns = null, bool bCommitOnChange = true)
            where BO : new()
            where EO : class, new() {
            bool validateOnSaveEnabled = context.Configuration.ValidateOnSaveEnabled;
            EO entityObj = new EO();
            try {
                if (dbColumns == null)
                    entityObj = BusinessLayerUtils.PopulateEntity<EO, BO>(obj);
                else
                    entityObj = BusinessLayerUtils.PopulateEntity<EO, BO>(obj, dbColumns);

                context.Configuration.ValidateOnSaveEnabled = false;
                context.Entry<EO>(entityObj).State = EntityState.Deleted;
                if (bCommitOnChange)
                    context.SaveChanges();
            } catch (Exception ex) { ThrowException<BO>(context, "Delete()", ex); } finally { context.Configuration.ValidateOnSaveEnabled = validateOnSaveEnabled; }
        }

        //save collection
        public static void SaveCollection<BO, EO>(
            Dictionary<BO, CommonEnums.DBChangeType> changesDictionary,
            DbContext context,
            DbSet<EO> dbSet,
            List<string> insertDBColumns = null,
            List<string> updateDBColumns = null,
            List<string> deleteDBColumns = null)
            where BO : BaseObject<BO>, new()
            where EO : class, new() {
            try {
                foreach (KeyValuePair<BO, CommonEnums.DBChangeType> kvp in changesDictionary) {
                    if (kvp.Value == CommonEnums.DBChangeType.New)
                        Insert<BO, EO>(kvp.Key, context, dbSet, insertDBColumns, false);
                    else if (kvp.Value == CommonEnums.DBChangeType.Dirty)
                        Update<BO, EO>(kvp.Key, context, dbSet, updateDBColumns, false);
                    else if (kvp.Value == CommonEnums.DBChangeType.Deleted)
                        Delete<BO, EO>(kvp.Key, context, dbSet, deleteDBColumns, false);
                }
                context.SaveChanges();
            } catch (Exception ex) { ThrowException<BO>(context, "SaveCollection()", ex); }
        }

        //save collection (with SQL)
        public static void SaveCollectionSQL<BO>(
            Dictionary<BO, CommonEnums.DBChangeType> changesDictionary,
            DbContext context,
            string insertSQL = null,
            GetSqlParametersDelegate<BO> insertParamsGetter = null,
            string updateSQL = null,
            GetSqlParametersDelegate<BO> updateParamsGetter = null,
            string deleteSQL = null,
            GetSqlParametersDelegate<BO> deleteParamsGetter = null)
            where BO : BaseObject<BO>, new() {
            DbContextTransaction tran = null;
            try {
                tran = context.Database.BeginTransaction();
                foreach (KeyValuePair<BO, CommonEnums.DBChangeType> kvp in changesDictionary) {
                    if (kvp.Value == CommonEnums.DBChangeType.New)
                        ExecuteSqlCommand<BO>(
                            SqlCommandType.Insert,
                            context,
                            kvp.Key,
                            insertSQL,
                            insertParamsGetter);
                    else if (kvp.Value == CommonEnums.DBChangeType.Dirty)
                        ExecuteSqlCommand<BO>(
                            SqlCommandType.Update,
                            context,
                            kvp.Key,
                            updateSQL,
                            updateParamsGetter);
                    else if (kvp.Value == CommonEnums.DBChangeType.Deleted)
                        ExecuteSqlCommand<BO>(
                            SqlCommandType.Delete,
                            context,
                            kvp.Key,
                            deleteSQL,
                            deleteParamsGetter);
                }
                tran.Commit();
            } catch (Exception ex) { tran.Rollback(); ThrowException<BO>(context, "SaveCollectionSQL()", ex); }
        }
        public static DataTable ToDataTable<EO>(this IEnumerable<EO> data, DataTable table = null) where EO : class, new() {
            IEnumerable<PropertyInfo> propertyInfos = typeof(EO).GetProperties().Where(x => Attribute.IsDefined(x, typeof(DbColumnAttribute)));
            if (table == null) {
                table = new DataTable();
                foreach (PropertyInfo prop in propertyInfos) {
                    table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
                }
            }
            table.Clear();
            foreach (EO item in data) {
                DataRow row = table.NewRow();
                foreach (PropertyInfo prop in propertyInfos) {
                    DbColumnAttribute attributeInfo = prop.GetCustomAttribute<DbColumnAttribute>();
                    string columnName = !string.IsNullOrEmpty(attributeInfo.ColumnName) ? attributeInfo.ColumnName : prop.Name;
                    row[columnName] = prop.GetValue(item) ?? DBNull.Value;
                }

                table.Rows.Add(row);
            }
            return table;
        }

        private static int ExecuteSqlCommand<I>(SqlCommandType commandType, DbContext context, I item, string sqlString, GetSqlParametersDelegate<I> paramsGetter)
            where I : new() {
            int iRetVal = 0;
            SqlParameter[] paramsArray = GetSqlParamsArray(paramsGetter(item));
            try { iRetVal = context.Database.ExecuteSqlCommand(sqlString, paramsArray); } catch (Exception ex) { ThrowException<I>(context, "ExecuteSqlCommand() Command Type: " + commandType.ToString(), ex); }
            return iRetVal;
        }

        private static SqlParameter[] GetSqlParamsArray(Dictionary<string, object> paramsDict) {
            List<SqlParameter> sqlParams = new List<SqlParameter>();
            sqlParams.AddRange(paramsDict.Select(x => new SqlParameter(x.Key, Utils.NullEmptyCheck(x.Value) ? (object)DBNull.Value : x.Value)));
            return sqlParams.ToArray();
        }

        private static void ThrowException<BO>(DbContext context, string sourceMethod, Exception ex) {
            string sourceFile = typeof(BO).Name;
            string exceptionType = ex.GetType().ToString();
            string strMessage = string.Format("Exception Type: {0} | ConnectionString: {1} | File: {2} | Method: {3} | Database Error: {4} | {5} Stack Trace: {6} | Inner Exception(s) (if any): ",
                exceptionType, context.Database.Connection.ConnectionString, sourceFile, sourceMethod, ex.Message, Environment.NewLine, ex.StackTrace);
            Exception theInnerException = ex.InnerException;
            while (theInnerException != null) {
                strMessage += theInnerException.Message;
                theInnerException = theInnerException.InnerException;
            }
            throw new Exception(strMessage);
        }
    }
}
