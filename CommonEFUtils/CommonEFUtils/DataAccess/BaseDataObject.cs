using System.Collections.Generic;
using System.Data.Entity;

namespace CommonEFUtils {
    public abstract class BaseDataObject<I, E, DC> : BaseReadOnlyDataObject<I, E, DC>
        where I : new()
        where E : class, new()
        where DC : DbContext, new() {
        protected static E Insert(I obj, DC context, DbSet<E> dbSet, List<string> dbColumns = null) { return DBUtils.Insert<I, E>(obj, context, dbSet, dbColumns); }
        protected static void Update(I obj, DC context, DbSet<E> dbSet) { DBUtils.Update<I, E>(obj, context, dbSet); }
        protected static void Update(I obj, DC context, DbSet<E> dbSet, List<string> dbColumns) { DBUtils.Update<I, E>(obj, context, dbSet, dbColumns); }
        protected static void Delete(I obj, DC context, DbSet<E> dbSet) { DBUtils.Delete<I, E>(obj, context, dbSet); }
    }
}
