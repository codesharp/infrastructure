//Copyright (c) CodeSharp.  All rights reserved. - http://www.codesharp.cn/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeSharp.Core.DomainBase;

namespace CodeSharp.Core.RepositoryFramework
{
    /// <summary>仓储抽象基类
    /// </summary>
    /// <typeparam name="T"></typeparam> 
    public abstract class RepositoryBase<TKey, TEntity> : IRepository<TKey, TEntity>
        where TKey : IComparable
        where TEntity : IAggregateRoot
    {
        #region IRepository<T>
        /// <summary>创建
        /// </summary>
        /// <param name="item"></param>
        public void Add(TEntity item)
        {
            this.PersistNewItem(item);
        }
        /// <summary>移除
        /// </summary>
        /// <param name="item"></param>
        public void Remove(TEntity item)
        {
            this.PersistDeleteItem(item);
        }
        /// <summary>更新
        /// </summary>
        /// <param name="item"></param>
        public void Update(TEntity item)
        {
            this.PersistUpdateItem(item);
        }
        /// <summary>创建或更新
        /// </summary>
        /// <param name="item"></param>
        public void SaveOrUpdate(TEntity item)
        {
            this.PersistSaveOrUpdateItem(item);
        }
        /// <summary>根据标识查找
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public abstract TEntity FindBy(TKey key);
        /// <summary>返回全部
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerable<TEntity> FindAll();
        /// <summary>根据标识查找
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public abstract IEnumerable<TEntity> FindBy(params TKey[] keys);
        #endregion

        protected abstract void PersistNewItem(TEntity item);
        protected abstract void PersistUpdateItem(TEntity item);
        protected abstract void PersistDeleteItem(TEntity item);
        protected abstract void PersistSaveOrUpdateItem(TEntity item);
    }
}
