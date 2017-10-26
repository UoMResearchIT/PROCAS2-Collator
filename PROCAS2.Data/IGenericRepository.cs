using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace PROCAS2.Data
{
    public interface IGenericRepository<TEntity>
    {
        IEnumerable<TEntity> Get(
             Expression<Func<TEntity, bool>> filter = null,
             Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
             string includeProperties = "");

        IQueryable<TEntity> GetAll();

        TEntity GetByID(object id);


        void Insert(TEntity entity);

        void Delete(object id);


        void Delete(TEntity entityToDelete);


        void Update(TEntity entityToUpdate);
    }
}
