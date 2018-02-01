using CommonUtils;
using System.Collections.Generic;
using System.Data.Entity;

namespace CommonEFUtils {
    public abstract class BaseCollectableDataObject<I, E, DC> : BaseDataObject<I, E, DC>
        where I : BaseObject<I>, new()
        where E : class, new()
        where DC : DbContext, new() {
        protected static void SaveCollection(Dictionary<I, CommonEnums.DBChangeType> changesDictionary, DC context, DbSet<E> dbSet, List<string> insertDBColumns = null, List<string> updateDBColumns = null, List<string> deleteDBColumns = null) {
            DBUtils.SaveCollection(changesDictionary, context, dbSet, insertDBColumns, updateDBColumns, deleteDBColumns);
        }
        protected static List<I> GetCollection(IEnumerable<E> entitySet, List<string> DBColumns = null) {
            return BusinessLayerUtils.BuildCollectionFromEntitySet<I, E>(entitySet, DBColumns);
        }
    }
}
