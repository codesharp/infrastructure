//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeSharp.Core.DomainBase;
using CodeSharp.Core.Services;

namespace CodeSharp.Core.RepositoryFramework
{
    /// <summary>仓储工厂
    /// </summary>
    public static class RepositoryFactory
    {
        /// <summary>获取指定的仓储接口实例
        /// </summary>
        /// <typeparam name="TRepository">仓储接口</typeparam>
        /// <typeparam name="TEntity">仓储接口对应的实体</typeparam>
        /// <returns></returns>
        public static TRepository GetRepository<TRepository, TKey, TEntity>()
            where TRepository : class, IRepository<TKey, TEntity>
            where TKey : IComparable
            where TEntity : IAggregateRoot
        {
            return DependencyResolver.Resolve<TRepository>();
        }
    }
}
