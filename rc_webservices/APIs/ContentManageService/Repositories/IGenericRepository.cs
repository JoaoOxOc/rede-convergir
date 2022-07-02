using CommonLibrary.Enums;
using System.Linq.Expressions;

namespace ContentManageService.Repositories
{
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        void Add(TEntity entity);

        void Add(List<TEntity> entities);

        TEntity GetById(object id);

        List<TEntity> Get(int? pageNum = null, int? size = null, Expression<Func<TEntity, bool>> filter = null, Expression<Func<TEntity, object>> sort = null, SortDirection sortDirection = SortDirection.Ascending, string includeProperties = "", bool noTracking = false);

        List<TEntity> Get(int? pageNum = null, int? size = null, Expression<Func<TEntity, bool>> filter = null, string orderBy = null, SortDirection sortDirection = SortDirection.Ascending, string includeProperties = "", bool noTracking = false);

        void Update(TEntity entityToUpdate);

        void Update(List<TEntity> entitiesToUpdate);

        void Delete(object id);

        void Delete(TEntity entityToDelete);

        int Count(Expression<Func<TEntity, bool>> filter = null);
    }
}
