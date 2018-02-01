using System.Collections.Generic;
using System.Data.Entity;

namespace CommonEFUtils {
    public abstract class BaseReadOnlyCollectableDataObject<I, E, DC> : BaseReadOnlyDataObject<I, E, DC>
        where I : new()
        where E : new()
        where DC : DbContext, new() {
        protected static List<I> GetCollection(IEnumerable<E> entitySet) { return BusinessLayerUtils.BuildCollectionFromEntitySet<I, E>(entitySet); }
        protected static List<I> GetCollection(IEnumerable<E> entitySet, List<string> DBColumns) { return BusinessLayerUtils.BuildCollectionFromEntitySet<I, E>(entitySet, DBColumns); }
    }
}
