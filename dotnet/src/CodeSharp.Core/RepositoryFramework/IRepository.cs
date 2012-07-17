//Copyright (c) CodeSharp.  All rights reserved. - http://www.codesharp.cn/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeSharp.Core.DomainBase;

namespace CodeSharp.Core.RepositoryFramework
{
    /// <summary>仓储接口
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TEntity"></typeparam>
    public interface IRepository<TKey, TEntity>
        where TEntity : IAggregateRoot
        where TKey : IComparable
    {
        void Add(TEntity item);
        void Remove(TEntity item);
        void Update(TEntity item);
        void SaveOrUpdate(TEntity item);
        TEntity FindBy(TKey key);
        IEnumerable<TEntity> FindAll();
        IEnumerable<TEntity> FindBy(params TKey[] keys);
    }
}
