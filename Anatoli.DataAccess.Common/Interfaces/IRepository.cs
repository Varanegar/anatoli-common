using System;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace Anatoli.Common.DataAccess.Interfaces
{
    public interface IRepository<T> : IBaseRepository<T> where T : class
    {
        TResult GetById<TResult>(Guid id);
        Task<TResult> GetByIdAsync<TResult>(Guid id);
        Task<TResult> GetByIdAsync<TResult>(Guid id, Expression<Func<T, TResult>> selector);
    }
}