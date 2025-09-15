using LibraryApplication.Domain.Domain;
using LibraryApplication.Repository.Data;
using LibraryApplication.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;

namespace LibraryApplication.Repository.Implementation
{
    public class Repository<T> : IRepository<T> where T : BaseEntity
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<T> entites;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
            this.entites = _context.Set<T>();
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync();
        }

        public T Insert(T entity)
        {
            _context.Add(entity);
            _context.SaveChanges();
            return entity;
        }

        public T Update(T entity)
        {
            _context.Update(entity);
            _context.SaveChanges();
            return entity;
        }

        public T Delete(T entity)
        {
            _context.Remove(entity);
            _context.SaveChanges();
            return entity;
        }

        public E? Get<E>(Expression<Func<T, E>> selector,
            Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null)
        {
            IQueryable<T> query = entites;
            if (include != null)
            {
                query = include(query);
            }

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            if (orderBy != null)
            {
                return orderBy(query).Select(selector).FirstOrDefault();
            }

            return query.Select(selector).FirstOrDefault();
        }

        public IEnumerable<T> InsertAll(IEnumerable<T> entities, bool saveChanges = true)
        {
            if (entities == null || !entities.Any())
                return Enumerable.Empty<T>();

            _context.AddRange(entities);
            if (saveChanges)
                _context.SaveChanges();

            return entities;
        }

        public IEnumerable<E> GetAll<E>(Expression<Func<T, E>> selector,
            Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
            bool asNoTracking = false)
        {
            IQueryable<T> query = entites;

            if (include != null)
            {
                query = include(query);
            }

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }

            if (orderBy != null)
            {
                return orderBy(query).Select(selector).AsEnumerable();
            }

            return query.Select(selector).AsEnumerable();
        }

        public int DeleteAll(bool saveChanges = true)
        {
            if (!entites.Any()) return 0;

            _context.RemoveRange(entites);
            if (saveChanges)
                return _context.SaveChanges();

            return entites.Count();
        }
    }
}
