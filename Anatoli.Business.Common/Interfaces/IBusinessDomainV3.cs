using System;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Collections.Generic;
using Anatoli.Common.DataAccess.Models;
using Anatoli.Common.ViewModel;

namespace Anatoli.Common.Business.Interfaces
{
    public interface IBusinessDomainV3<TSource>
        where TSource : BaseModel, new()
    {
        Expression<Func<TSource, TResult>> GetAllSelector<TResult>() where TResult : class, new();
        void SetConditionForFetchingData();
        Task<List<TResult>> GetAllAsync<TResult>() where TResult : class, new();
        Task<List<TResult>> GetAllAsync<TResult>(Expression<Func<TSource, bool>> predicate, Expression<Func<TSource, TResult>> selector);
        Task<TResult> GetByIdAsync<TResult>(Guid id) where TResult : class, new();
        Task<List<TResult>> GetAllChangedAfterAsync<TResult>(DateTime selectedDate) where TResult : class, new();
        Task PublishAsync(List<TSource> data);
        Task PublishAsync<TResult>(List<TResult> data) where TResult : CBaseViewModel;
        Task PublishAsync(TSource data);
        Task PublishAsync<TResult>(TResult data) where TResult : CBaseViewModel;
        Task DeleteAsync(List<TSource> data);
        Task DeleteAsync<TResult>(List<TResult> data) where TResult : CBaseViewModel;
        Task CheckDeletedAsync<TResult>(List<TResult> data) where TResult : CBaseViewModel, new();
    }
}