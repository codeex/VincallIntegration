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
    public interface ICrudServices
    {
        DbContext Context { get; }
       
        Task<T> ReadSingleAsync<T>(params object[] keys) where T : class;

        Task<T> ReadSingleAsync<T>(Expression<Func<T, bool>> whereExpression) where T : class;

        IQueryable<T> ReadManyNoTracked<T>() where T : class;

        IQueryable<T> ReadManyNoTracked<T>(Expression<Func<T, bool>> whereExpression) where T : class;

        IQueryable<T> ReadMany<T>(Expression<Func<T, bool>> whereExpression) where T : class;
        Task<T> CreateAndSaveAsync<T>(T entity) where T : class;

        
        Task UpdateAndSaveAsync<T>(T entity, string methodName = null) where T : class;

        
        Task<TEntity> UpdateAndSaveAsync<TEntity>(JsonPatchDocument<TEntity> patch, params object[] keys) where TEntity : class;

        Task<TEntity> UpdateAndSaveAsync<TEntity>(JsonPatchDocument<TEntity> patch, Expression<Func<TEntity, bool>> whereExpression) where TEntity : class;


        Task DeleteAndSaveAsync<TEntity>(params object[] keys) where TEntity : class;

        Task<IEnumerable<T>> DeleteAllThenInsertAndSaveAsync<T>(IEnumerable<T> entities) where T : class;
    }
}
