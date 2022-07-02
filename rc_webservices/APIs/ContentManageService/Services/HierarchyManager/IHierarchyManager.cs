using System.Linq.Expressions;

namespace ContentManageService.Services.HierarchyManager
{
    public interface IHierarchyManager<TEntity> where TEntity : class
    {
        IEnumerable<TEntity> GetChildElements<TId, TEntity>(IQueryable<TEntity> objects, TId id, Expression<Func<TEntity, TId>> idSelector);
    }
}
