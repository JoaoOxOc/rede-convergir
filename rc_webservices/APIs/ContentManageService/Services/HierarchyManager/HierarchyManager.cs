using System.Linq.Expressions;
using System.Reflection;

namespace ContentManageService.Services.HierarchyManager
{
    public class HierarchyManager<TEntity> : IHierarchyManager<TEntity> where TEntity : class
    {
        public IEnumerable<TEntity> GetChildElements<TId, TEntity>(IQueryable<TEntity> objects, TId id, Expression<Func<TEntity, TId>> idSelector)
        {
            var paramExpr = Expression.Parameter(typeof(TEntity));
            var idEqExpr = Expression.Equal(Expression.Invoke(idSelector, paramExpr), Expression.Constant(id));
            //var createdPropExpr = Expression.Property(paramExpr, "CreatedDate");
            //var gtExpr = Expression.GreaterThan(createdPropExpr, Expression.Constant(date));
            //var andExpr = Expression.And(idEqExpr, gtExpr);

            var condExpr = Expression.Lambda<Func<TEntity, bool>>(paramExpr);//(andExpr, paramExpr)

            return objects.Where(condExpr);
        }
    }
}
