using System;
using LinqKit;
using System.Linq;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Linq.Expressions;
using AutoMapper.QueryableExtensions;
using Anatoli.Common.DataAccess.Models;
using Anatoli.Common.DataAccess.Interfaces;

namespace Anatoli.Common.DataAccess.Repositories
{
    public abstract class AnatoliRepository<T> : BaseAnatoliRepository<T>, IDisposable, IRepository<T> where T : BaseModel, new()
    {
        #region Ctors
        public AnatoliRepository(DbContext dbContext) : base(dbContext)
        {
        }

        public AnatoliRepository(DbContext dbContext, OwnerInfo ownerInfo) : this(dbContext)
        {
            OwnerInfo = ownerInfo;
        }

        #endregion
        #region Methods
        public TResult GetById<TResult>(Guid id)
        {
            return DbSet.Where(p => p.Id == id).ProjectTo<TResult>().FirstOrDefault();
        }

        public async Task<TResult> GetByIdAsync<TResult>(Guid id)
        {
            return await DbSet.Where(p => p.Id == id).ProjectTo<TResult>().FirstOrDefaultAsync();
        }

        public async Task<TResult> GetByIdAsync<TResult>(Guid id, Expression<Func<T, TResult>> selector)
        {
            return await DbSet.Where(p => p.Id == id).Select(selector).FirstOrDefaultAsync();
        }

        protected override Expression<Func<T, bool>> CalcExtraPredict(Expression<Func<T, bool>> predict = null)
        {
            if (predict == null)
                predict = p => true;
            if (OwnerInfo != null)
                predict = PredicateBuilder.And(predict, p => p.ApplicationOwnerId == OwnerInfo.ApplicationOwnerKey && p.DataOwnerId == OwnerInfo.DataOwnerKey && p.IsRemoved == (OwnerInfo.RemovedData ? p.IsRemoved : false));
            if (ExtraPredicate != null)
                predict = PredicateBuilder.And(predict, ExtraPredicate);
            return predict;
        }
        #endregion
    }
}