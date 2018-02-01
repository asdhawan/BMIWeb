using System;
using System.Data.Entity;
using System.Linq.Expressions;

namespace CommonEFUtils {
    public abstract class BaseReadOnlyDataObject<I, E, DC>
        where I : new()
        where E : new()
        where DC : DbContext, new() {
        protected static DC NewEntityContext() { return new DC(); }

        protected static I GetOneObject(E entity) { return BusinessLayerUtils.PopulateObject<I, E>(entity); }

        protected static Expression<Func<E, bool>> NoFilter = x => true;
    }
}
