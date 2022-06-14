using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace VincallIntegration.Application
{
    public class CrudServices : CrudServices<DbContext>
    {
        public CrudServices(DbContext context) : base(context)
        {
            if (context == null)
                throw new ArgumentNullException("The DbContext class is null. Either you haven't registered GenericServices, or you are using the multi-DbContext version, in which case you need to use the CrudServices<TContext> and specify which DbContext to use.");
        }
    }

    public class CrudServices<TContext> :
        ICrudServices<TContext> where TContext : DbContext
    {
        private readonly TContext _context;

        public DbContext Context => _context;

        public CrudServices(TContext context)
        {
            _context = context;
        }

        
        public async Task<T> ReadSingleAsync<T>(params object[] keys) where T : class
        {
            T result;    
            result = await _context.Set<T>().FindAsync(keys).ConfigureAwait(false);     
            return result;
        }

        public async Task<T> ReadSingleAsync<T>(Expression<Func<T, bool>> whereExpression) where T : class
        {
            T result;            
            result = await _context.Set<T>().AsQueryable()
                .Where(whereExpression).SingleOrDefaultAsync().ConfigureAwait(false);   
            return result;
        }

        public IQueryable<T> ReadMany<T>(Expression<Func<T, bool>> whereExpression) where T : class
        {
            var result = _context.Set<T>().AsQueryable()
                .Where(whereExpression);
            return result;
        }

        public IQueryable<T> ReadManyNoTracked<T>(Expression<Func<T, bool>> whereExpression) where T : class
        {
            var result = _context.Set<T>().AsQueryable()
                .Where(whereExpression).AsNoTracking();
            return result;
        }

        public IQueryable<T> ReadManyNoTracked<T>() where T : class
        {           
             return _context.Set<T>().AsQueryable().AsNoTracking();
        }       

        public async Task<T> CreateAndSaveAsync<T>(T entity) where T : class
        {           
            _context.Add(entity);
            await _context.SaveChangesAsync().ConfigureAwait(false);           
            return entity;
        }

        public async Task UpdateAndSaveAsync<T>(T entity, string methodName = null) where T : class
        {          
           
            if (!_context.Entry(entity).IsKeySet)
                throw new InvalidOperationException($"The primary key was not set on the entity class {typeof(T).Name}. For an update we expect the key(s) to be set (otherwise it does a create).");
            if (_context.Entry(entity).State == EntityState.Detached)
                _context.Update(entity);
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<TEntity> UpdateAndSaveAsync<TEntity>(JsonPatchDocument<TEntity> patch, params object[] keys) where TEntity : class
        {
            return await LocalUpdateAndSaveAsync(patch, async () => await _context.FindAsync<TEntity>(keys).ConfigureAwait(false));
        }

        public async Task<TEntity> UpdateAndSaveAsync<TEntity>(JsonPatchDocument<TEntity> patch, Expression<Func<TEntity, bool>> whereExpression) where TEntity : class
        {
            return await LocalUpdateAndSaveAsync(patch, () => _context.Set<TEntity>().SingleOrDefaultAsync(whereExpression)).ConfigureAwait(false);
        }
       
        private async Task<TEntity> LocalUpdateAndSaveAsync<TEntity>(JsonPatchDocument<TEntity> patch, Func<Task<TEntity>> getEntity)
            where TEntity : class
        {           

            var entity = await getEntity().ConfigureAwait(false);

            patch.ApplyTo(entity, x => { }) ;            

            return entity;
        }

        public async Task DeleteAndSaveAsync<TEntity>(params object[] keys) where TEntity : class
        {           
            var entity = await this.ReadSingleAsync<TEntity>(keys);
            if (entity == null)
            {
                throw new InvalidOperationException($"Sorry, I could not find the {typeof(TEntity).Name} you wanted to delete.");               
            }
            _context.Remove(entity);
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<IEnumerable<T>> DeleteAllThenInsertAndSaveAsync<T>(IEnumerable<T> entities) where T : class
        {
            var sources = ReadManyNoTracked<T>();

            _context.RemoveRange(sources);

            await _context.AddRangeAsync(entities);

            await _context.SaveChangesAsync().ConfigureAwait(false);

            return this.ReadMany<T>(ex => true);
        }
    }

    public static class GenericPaging
    {
        public static IQueryable<T> Page<T>(
            this IQueryable<T> query,
            int pageNumZeroStart, int pageSize)
        {

            if (pageSize == 0)
                throw new ArgumentOutOfRangeException
                    (nameof(pageSize), "pageSize cannot be zero.");

            if (pageNumZeroStart != 0)
                query = query
                    .Skip(pageNumZeroStart * pageSize);

            return query.Take(pageSize);
        }


        public static List<T> Page<T>(
            this List<T> query,
            int pageNumZeroStart, int pageSize)
        {

            if (pageSize == 0)
                throw new ArgumentOutOfRangeException
                    (nameof(pageSize), "pageSize cannot be zero.");

            if (pageNumZeroStart != 0)
                query = query
                    .Skip(pageNumZeroStart * pageSize).Take(pageSize).ToList();

            return query;
        }
    }
}
