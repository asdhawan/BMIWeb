using CommonUtils;
using System;
using System.Collections.Generic;
using System.Data.Entity;

namespace CommonEFUtils {
    public class BaseDbContext<BO, DA> : DbContext
        where BO : BaseObject<BO>
        where DA : new() {
        private string DbTableName { get; set; }
        public BaseDbContext(string dbTableName, string connectionStringName) : base("name=" + connectionStringName) { DbTableName = dbTableName; }
        public DbSet<BO> ObjectCollection { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder) { modelBuilder.Entity<BO>().ToTable(DbTableName); }

        public static DA NewDAInstance() { return new DA(); }

        public void Insert(BO obj) {
            try {
                ObjectCollection.Add(obj);
                SaveChanges();
            } catch (Exception ex) { ThrowException("Insert()", ex); }
        }
        public void Update(BO obj) {
            try {
                Entry(obj).State = EntityState.Modified;
                SaveChanges();
            } catch (Exception ex) { ThrowException("Update()", ex); }
        }
        public void Delete(BO obj) {
            try {
                ObjectCollection.Remove(obj);
                SaveChanges();
            } catch (Exception ex) { ThrowException("Delete()", ex); }
        }
        public void SaveCollection(Dictionary<BO, CommonEnums.DBChangeType> changesDictionary) {
            DbContextTransaction tran = null;
            try {
                Database.Connection.Open();
                tran = Database.BeginTransaction();
                foreach (KeyValuePair<BO, CommonEnums.DBChangeType> kvp in changesDictionary) {
                    if (kvp.Value == CommonEnums.DBChangeType.New)
                        Insert(kvp.Key);
                    else if (kvp.Value == CommonEnums.DBChangeType.Dirty)
                        Update(kvp.Key);
                    else if (kvp.Value == CommonEnums.DBChangeType.Deleted)
                        Delete(kvp.Key);
                }
                tran.Commit();
            } catch { tran.Rollback(); } finally { Database.Connection.Close(); }
        }

        private void ThrowException(string sourceMethod, Exception ex) {
            string sourceFile = typeof(BO).Name;
            string strMessage = string.Format("ConnectionString: {0} | File: {1} | Method: {2} | Database Error: {3} | {4} Stack Trace: {4}",
                Database.Connection.ConnectionString, sourceFile, sourceMethod, ex.Message, Environment.NewLine, ex.StackTrace);
            throw new Exception(strMessage);
        }
    }
}
