using System;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Anatoli.Common.ViewModel;
using System.Collections.Generic;
using Anatoli.Common.DataAccess.Models;

namespace Anatoli.Common.Business.Interfaces
{
    public interface IBusinessDomainV2<TMainSource, TMainSourceView>
        where TMainSource : BaseModel, new ()where TMainSourceView : CBaseViewModel, new ()
    {
        Guid ApplicationOwnerKey
        {
            get;
        }

        Guid DataOwnerKey
        {
            get;
        }

        Guid DataOwnerCenterKey
        {
            get;
        }

        Task<List<TMainSourceView>> GetAllAsync(Expression<Func<TMainSource, bool>> predicate);
        Task<List<TMainSourceView>> GetAllAsync();
        Task<TMainSourceView> GetByIdAsync(Guid id);
        Task<List<TMainSourceView>> GetAllChangedAfterAsync(DateTime selectedDate);
        Task PublishAsync(List<TMainSource> data);
        Task PublishAsync(TMainSource data);
        Task DeleteAsync(List<TMainSource> data);
        Task DeleteAsync(List<TMainSourceView> data);
        Task CheckDeletedAsync(List<TMainSourceView> data);
    }
}