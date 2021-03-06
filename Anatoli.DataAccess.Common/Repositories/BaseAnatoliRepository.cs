﻿using System;
using LinqKit;
using System.Linq;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Linq.Expressions;
using EntityFramework.Caching;
using System.Collections.Generic;
using EntityFramework.Extensions;
using System.Data.Entity.Validation;
using AutoMapper.QueryableExtensions;
using Anatoli.Common.DataAccess.Models;
using System.Data.Entity.Infrastructure;
using Anatoli.Common.DataAccess.Interfaces;
using System.IO;
using LinqToExcel;
using Anatoli.Common.DynamicLinq;

namespace Anatoli.Common.DataAccess.Repositories
{
    public abstract class BaseAnatoliRepository<T> : IDisposable, IBaseRepository<T> where T : class
    {
        #region Properties
        public OwnerInfo OwnerInfo
        {
            get;
            set;
        }

        public DbContext DbContext
        {
            get;
            set;
        }

        protected DbSet<T> DbSet
        {
            get;
            set;
        }

        public Expression<Func<T, bool>> ExtraPredicate
        {
            get;
            set;
        }

        #endregion
        #region Ctors
        public BaseAnatoliRepository(DbContext dbContext)
        {
            if (dbContext == null)
                throw new ArgumentNullException("Null DbContext");
            DbContext = dbContext;
            DbSet = DbContext.Set<T>();
            DbContext.Database.Log = Console.Write;
        }

        public BaseAnatoliRepository(DbContext dbContext, OwnerInfo ownerInfo) : this(dbContext)
        {
            OwnerInfo = ownerInfo;
        }

        #endregion
        #region Methods
        public virtual IQueryable<T> GetQuery()
        {
            return DbSet;
        }

        public virtual T GetById(Guid id)
        {
            return DbSet.Find(id);
        }

        public virtual async Task<T> GetByIdAsync(Guid id)
        {
            return await DbSet.FindAsync(id);
        }

        public virtual List<T> GetAll()
        {
            return DbSet.AsExpandable().Where(CalcExtraPredict()).AsNoTracking().ToList();
        }

        public virtual async Task<List<T>> GetAllAsync()
        {
            return await DbSet.AsExpandable().Where(CalcExtraPredict()).AsNoTracking().ToListAsync();
        }

        public virtual async Task<List<TResult>> GetAllAsync<TResult>(Expression<Func<T, TResult>> selector)
        {
            var query = GetQuery().AsExpandable().Where(CalcExtraPredict()).AsNoTracking();
            if (selector != null)
                return await query.Select(selector).ToListAsync();
            return await query.ProjectTo<TResult>().ToListAsync();
        }

        public virtual async Task<T> FindAsync(Expression<Func<T, bool>> predicate)
        {
            try
            {
                return await DbSet.AsExpandable().SingleOrDefaultAsync(CalcExtraPredict(predicate));
            }
            catch (Exception)
            {
                return null;
            }
        }

        public virtual async Task<TResult> FindAsync<TResult>(Expression<Func<T, bool>> predicate, Expression<Func<T, TResult>> selector)
        {
            if (selector != null)
                return await DbSet.AsExpandable().Where(CalcExtraPredict(predicate)).Select(selector).SingleOrDefaultAsync();
            return await DbSet.AsExpandable().Where(CalcExtraPredict(predicate)).ProjectTo<TResult>().SingleOrDefaultAsync();
        }

        public virtual async Task<List<T>> FindAllAsync(Expression<Func<T, bool>> predicate)
        {
            return await DbSet.AsExpandable().Where(CalcExtraPredict(predicate)).ToListAsync();
        }

        public virtual async Task<List<TResult>> FindAllAsync<TResult>(Expression<Func<T, bool>> predicate, Expression<Func<T, TResult>> selector)
        {
            if (selector != null)
                return await DbSet.AsExpandable().Where(CalcExtraPredict(predicate)).Select(selector).ToListAsync();
            return await DbSet.AsExpandable().Where(CalcExtraPredict(predicate)).ProjectTo<TResult>().ToListAsync();
        }

        public virtual IEnumerable<T> GetFromCached(Expression<Func<T, bool>> predicate, int cacheTimeOut = 300)
        {
            return DbSet.AsExpandable().Where(CalcExtraPredict(predicate)).FromCache(CachePolicy.WithDurationExpiration(TimeSpan.FromSeconds(300)));
        }

        public virtual async Task<IEnumerable<T>> GetFromCachedAsync(Expression<Func<T, bool>> predicate, int cacheTimeOut = 300)
        {
            return await DbSet.AsExpandable().Where(CalcExtraPredict(predicate)).FromCacheAsync(CachePolicy.WithDurationExpiration(TimeSpan.FromSeconds(300)), tags: new List<string> { typeof(T).ToString() });
        }

        public virtual async Task<IEnumerable<TResult>> GetFromCachedAsync<TResult>(Expression<Func<T, bool>> predicate, Expression<Func<T, TResult>> selector, int cacheTimeOut = 300) where TResult : class
        {
            if (selector != null)
                return await DbSet.AsExpandable().Where(CalcExtraPredict(predicate)).Select(selector).FromCacheAsync(CachePolicy.WithDurationExpiration(TimeSpan.FromSeconds(300)), tags: new List<string> { typeof(T).ToString() });
            return await DbSet.AsExpandable().Where(CalcExtraPredict(predicate)).ProjectTo<TResult>().FromCacheAsync(CachePolicy.WithDurationExpiration(TimeSpan.FromSeconds(300)), tags: new List<string> { typeof(T).ToString() });
        }

        public virtual async Task<DataSourceResult> Filter<TResult>(DataSourceRequest request, Expression<Func<T, bool>> predicate, Expression<Func<T, TResult>> selector)
        {
            IQueryable<TResult> query;
            if (selector != null)
                query = DbSet.AsExpandable().Where(CalcExtraPredict(predicate)).Select(selector);
            else
                query = DbSet.AsExpandable().Where(CalcExtraPredict(predicate)).ProjectTo<TResult>();

            return await query.ToDataSourceResult(request);
            //Task.Factory.StartNew(() => query.ToDataSourceResult(request));
        }

        protected virtual Expression<Func<T, bool>> CalcExtraPredict(Expression<Func<T, bool>> predict = null)
        {
            if (predict == null)
                predict = p => true;
            if (ExtraPredicate != null)
                predict = PredicateBuilder.And(predict, ExtraPredicate);
            return predict;
        }

        public virtual void Add(T entity)
        {
            var dbEntityEntry = DbContext.Entry(entity);
            if (dbEntityEntry.State != EntityState.Detached)
                dbEntityEntry.State = EntityState.Added;
            else
                DbSet.Add(entity);
        }

        public virtual async Task<T> AddAsync(T entity)
        {
            var dbEntityEntry = DbContext.Entry(entity);
            if (dbEntityEntry.State == EntityState.Detached)
                dbEntityEntry.State = EntityState.Added;
            else
                DbSet.Add(entity);
            var factory = Task<T>.Factory;
            return await factory.StartNew(() => entity);
        }

        public virtual async Task<List<T>> AddRangeAsync(List<T> model)
        {
            var dbEntityEntry = DbContext.Entry(model);
            if (dbEntityEntry.State == EntityState.Detached)
                dbEntityEntry.State = EntityState.Added;
            else
                DbSet.AddRange(model);

            return await Task<List<T>>.Factory.StartNew(() => model);
        }

        public virtual async Task<List<T>> ImportExcelToDB(string filePath, bool deleteFile = false)
        {
            try
            {
                var data = await new ExcelQueryFactory(filePath).Worksheet<T>("Sheet1").ToListAsync();

                var model = await AddRangeAsync(data);

                await SaveChangesAsync();

                //deleting excel file from folder  
                if (deleteFile && File.Exists(filePath))
                    File.Delete(filePath);

                return model;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public virtual void Update(T entity)
        {
            DbEntityEntry dbEntityEntry = DbContext.Entry(entity);
            if (dbEntityEntry.State == EntityState.Detached)
                DbSet.Attach(entity);
            dbEntityEntry.State = EntityState.Modified;
        }

        public virtual async Task<T> UpdateAsync(T entity)
        {
            DbEntityEntry dbEntityEntry = DbContext.Entry(entity);
            if (dbEntityEntry.State == EntityState.Detached)
                DbSet.Attach(entity);
            dbEntityEntry.State = EntityState.Modified;
            var factory = Task<T>.Factory;
            return await factory.StartNew(() => entity);
        }

        public virtual void UpdateBatch(Expression<Func<T, bool>> predicate, T entity)
        {
            DbSet.Where(predicate).Update(t => entity);
        }

        public virtual async Task UpdateBatchAsync(Expression<Func<T, bool>> predicate, T entity)
        {
            await DbSet.Where(predicate).UpdateAsync(t => entity);
        }

        public virtual void Delete(T entity)
        {
            DbEntityEntry dbEntityEntry = DbContext.Entry(entity);
            if (dbEntityEntry.State != EntityState.Deleted)
                dbEntityEntry.State = EntityState.Deleted;
            else
                DbSet.Attach(entity);
            DbSet.Remove(entity);
        }

        public virtual async Task DeleteAsync(T entity)
        {
            await Task.Factory.StartNew(() => Delete(entity));
        }

        public virtual async Task DeleteAsync(Guid id)
        {
            var entity = GetByIdAsync(id);
            if (entity == null)
                return; // not found; assume already deleted.
            await DeleteAsync(entity.Result);
        }

        public virtual void DeleteRange(List<T> entities)
        {
            entities.ForEach(entity =>
            {
                DbEntityEntry dbEntityEntry = DbContext.Entry(entity);
                if (dbEntityEntry.State != EntityState.Deleted)
                    dbEntityEntry.State = EntityState.Deleted;
                else
                    DbSet.Attach(entity);
            }

            );
            DbSet.RemoveRange(entities);
        }

        public virtual async Task DeleteRangeAsync(List<T> entities)
        {
            await Task.Factory.StartNew(() => DeleteRange(entities));
        }

        public virtual void DeleteBatch(Expression<Func<T, bool>> predicate)
        {
            DbSet.Where(predicate).Delete();
        }

        public virtual async Task DeleteBatchAsync(Expression<Func<T, bool>> predicate)
        {
            await DbSet.Where(predicate).DeleteAsync();
        }

        public virtual void EntryModified(T entity)
        {
            DbContext.Entry(entity).State = EntityState.Modified;
        }

        public virtual void SaveChanges()
        {
            try
            {
                DbContext.SaveChanges();
                ExpireCache();
            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:", eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"", ve.PropertyName, ve.ErrorMessage);
                    }
                }

                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public virtual async Task SaveChangesAsync()
        {
            try
            {
                await DbContext.SaveChangesAsync();
                ExpireCache();
            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:", eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"", ve.PropertyName, ve.ErrorMessage);
                    }
                }

                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public virtual int Count()
        {
            return DbSet.Count();
        }

        public virtual async Task<int> CountAsync()
        {
            return await DbSet.CountAsync();
        }

        private void ExpireCache()
        {
            var cacheKey = typeof(T).ToString();
            CacheManager.Current.Expire(cacheKey);
        }

        #endregion
        public void Dispose()
        {
            DbContext.Dispose();
        }
    }
}