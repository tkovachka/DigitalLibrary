using LibraryApplication.Domain.Domain;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;

namespace LibraryApplication.Repository.Interface
{
    public interface IRepository<T> where T : BaseEntity
    {
        Task<IDbContextTransaction> BeginTransactionAsync();
        T Insert(T entity);
        T Update(T entity);
        T Delete(T entity);
        E? Get<E>(Expression<Func<T, E>> selector,
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null);
        IEnumerable<T> InsertAll(IEnumerable<T> entities, bool saveChanges = true);
        IEnumerable<E> GetAll<E>(Expression<Func<T, E>> selector,
            Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
            bool asNoTracking = false);
        int DeleteAll(bool saveChanges = true);
    }
}
