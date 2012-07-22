//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Remoting.Messaging;

using NHibernate;
using NHibernate.Criterion;
using Castle.Services.Transaction;
using Castle.Facilities.NHibernateIntegration;
using CodeSharp.Core.DomainBase;
using CodeSharp.Core.Services;
using ILoggerFactory = CodeSharp.Core.Services.ILoggerFactory;
namespace CodeSharp.Core.Castles
{
    /// <summary>基于NHibernate提供的仓储
    /// </summary>
    /// <remarks>
    /// 取消using (var session = this.GetSession())
    /// 由SessionMananger自行管理
    /// </remarks>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TEntity"></typeparam>
    [Transactional]
    public class NHibernateRepositoryBase<TKey, TEntity> : RepositoryFramework.RepositoryBase<TKey, TEntity>
        where TKey : IComparable
        where TEntity : class, IAggregateRoot
    {
        private string _alias;
        private string _tempAlias;
        private readonly ISessionManager _sessionManager;
        /// <summary>当前日志记录器
        /// </summary>
        protected ILog _log;

        /// <summary>临时切换数据库
        /// <remarks>请使用using语法</remarks>
        /// </summary>
        /// <param name="tempAlias">指定临时工厂别名</param>
        protected SwitchAlias TempAlias(string tempAlias)
        {
            return new SwitchAlias(this, tempAlias);
        }

        public NHibernateRepositoryBase()
            : this(DependencyResolver.Resolve<ISessionManager>(), DependencyResolver.Resolve<ILoggerFactory>()) { }
        public NHibernateRepositoryBase(ISessionManager sessionManager) 
            : this(sessionManager, DependencyResolver.Resolve<ILoggerFactory>()) { }
        public NHibernateRepositoryBase(ISessionManager sessionManager, ILoggerFactory loggerFactory) 
            : this(sessionManager, loggerFactory, string.Empty) { }
        public NHibernateRepositoryBase(ISessionManager sessionManager
            , ILoggerFactory loggerFactory
            , string alias)
        {
            this._sessionManager = sessionManager;
            this._log = loggerFactory.Create(this.GetType());
            this._alias = alias;
        }

        //RepositoryBase

        /// <summary>根据主键查找
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public override TEntity FindBy(TKey key)
        {
            return this.GetSession().Get<TEntity>(key);
        }
        /// <summary>查找全部
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<TEntity> FindAll()
        {
            return this.GetSession().CreateCriteria<TEntity>().List<TEntity>();
        }
        /// <summary>根据多个主键查找
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public override IEnumerable<TEntity> FindBy(params TKey[] keys)
        {
            return this.GetSession().CreateCriteria<TEntity>().Add(Expression.In("ID", keys)).List<TEntity>();
        }

        [Transaction(TransactionMode.Requires)]
        protected override void PersistNewItem(TEntity item)
        {
            this.GetSession().Save(item);
        }
        [Transaction(TransactionMode.Requires)]
        protected override void PersistUpdateItem(TEntity item)
        {
            this.GetSession().Update(item);
        }
        [Transaction(TransactionMode.Requires)]
        protected override void PersistDeleteItem(TEntity item)
        {
            this.GetSession().Delete(item);
        }
        [Transaction(TransactionMode.Requires)]
        protected override void PersistSaveOrUpdateItem(TEntity item)
        {
            this.GetSession().SaveOrUpdate(item);
        }

        //protected

        /// <summary>查找全部
        /// </summary>
        /// <param name="detachCriteria">游离查询条件</param>
        /// <returns></returns>
        protected IEnumerable<TEntity> FindAll(DetachedCriteria detachCriteria)
        {
            return detachCriteria == null
                ? new List<TEntity>()
                : detachCriteria.GetExecutableCriteria(this.GetSession()).List<TEntity>();
        }
        /// <summary>分页查找全部
        /// </summary>
        /// <param name="detachCriteria">游离查询条件</param>
        /// <param name="pageIndex">无需分页则设置空</param>
        /// <param name="pageSize">无需分页则设置空</param>
        /// <param name="total"></param>
        /// <returns></returns>
        protected IEnumerable<TEntity> FindAll(DetachedCriteria detachCriteria, int? pageIndex, int? pageSize, out long total)
        {
            total = 0;

            if (detachCriteria == null)
                return new List<TEntity>();

            var session = this.GetSession();
            var query = CriteriaTransformer.Clone(detachCriteria.GetExecutableCriteria(session));
            var counter = CriteriaTransformer.Clone(detachCriteria.GetExecutableCriteria(session));
            //由于order的存在会导致聚合查询语法错误，因此需要清理此处的order，对于count(*)而言没有意义的orderby
            counter.ClearOrders();
            if (pageIndex.HasValue && pageSize.HasValue)
                query.SetFirstResult((pageIndex.Value - 1) * pageSize.Value).SetMaxResults(pageSize.Value);

            var result = query.List<TEntity>();

            //总数
            total = pageIndex.HasValue && pageSize.HasValue
                ? Convert.ToInt64(counter.SetProjection(Projections.RowCount()).UniqueResult())
                : result.Count;

            return result;
        }
        /// <summary>查找全部
        /// </summary>
        /// <param name="criterions"></param>
        /// <returns></returns>
        protected IEnumerable<TEntity> FindAll(params ICriterion[] criterions)
        {
            return this.FindAll(new Order[] { }, criterions);
        }
        /// <summary>查找全部
        /// </summary>
        /// <param name="order"></param>
        /// <param name="criterions"></param>
        /// <returns></returns>
        protected IEnumerable<TEntity> FindAll(Order order, params ICriterion[] criterions)
        {
            return this.FindAll(new Order[] { order }, criterions);
        }
        /// <summary>查找全部
        /// </summary>
        /// <param name="orders"></param>
        /// <param name="criterions"></param>
        /// <returns></returns>
        protected IEnumerable<TEntity> FindAll(Order[] orders, params ICriterion[] criterions)
        {
            return this.FindAll(null, null, orders, criterions);
        }
        /// <summary>分页查找全部
        /// </summary>
        /// <param name="pageIndex">页码 索引从1开始</param>
        /// <param name="pageSize"></param>
        /// <param name="orders"></param>
        /// <param name="criterions"></param>
        /// <returns></returns>
        protected IEnumerable<TEntity> FindAll(int? pageIndex, int? pageSize, Order[] orders, params ICriterion[] criterions)
        {
            ICriteria query = this.GetSession().CreateCriteria<TEntity>();
            if (orders != null)
                orders.ToList().ForEach(o => query.AddOrder(o));
            if (criterions != null)
                criterions.ToList().ForEach(o => query.Add(o));
            if (pageIndex.HasValue && pageSize.HasValue)
                query.SetFirstResult((pageIndex.Value - 1) * pageSize.Value).SetMaxResults(pageSize.Value);
            return query.List<TEntity>();
        }
        /// <summary>分页查找全部
        /// </summary>
        /// <param name="pageIndex">页码 索引从1开始</param>
        /// <param name="pageSize"></param>
        /// <param name="orders"></param>
        /// <param name="criterions"></param>
        /// <param name="totalCount">返回记录总数</param>
        /// <returns></returns>
        protected IEnumerable<TEntity> FindAll(int? pageIndex, int? pageSize, Order[] orders, ICriterion[] criterions, out long totalCount)
        {
            //计算总数query
            ICriteria counterQuery = this.GetSession().CreateCriteria<TEntity>();
            if (criterions != null)
                criterions.ToList().ForEach(o => counterQuery.Add(o));

            //分页query
            ICriteria query = CriteriaTransformer.Clone(counterQuery);

            //totalCount 
            totalCount = Convert.ToInt64(counterQuery.SetProjection(Projections.RowCount()).UniqueResult());

            if (orders != null)
                orders.ToList().ForEach(o => query.AddOrder(o));
            if (pageIndex.HasValue && pageSize.HasValue)
                query.SetFirstResult((pageIndex.Value - 1) * pageSize.Value).SetMaxResults(pageSize.Value);
            return query.List<TEntity>();
        }
        /// <summary>查找单个
        /// </summary>
        /// <param name="criterions"></param>
        /// <returns></returns>
        protected TEntity FindOne(params ICriterion[] criterions)
        {
            return this.FindOne(new Order[] { }, criterions);
        }
        /// <summary>查找单个
        /// </summary>
        /// <param name="order"></param>
        /// <param name="criterions"></param>
        /// <returns></returns>
        protected TEntity FindOne(Order order, params ICriterion[] criterions)
        {
            return this.FindOne(new Order[] { order }, criterions);
        }
        /// <summary>查找单个
        /// </summary>
        /// <param name="orders"></param>
        /// <param name="criterions"></param>
        /// <returns></returns>
        protected TEntity FindOne(Order[] orders, params ICriterion[] criterions)
        {
            var list = this.FindAll(orders, criterions);
            return list.Count() == 0 ? null : list.ElementAt(0);
        }
        /// <summary>获取session
        /// </summary>
        /// <returns></returns>
        protected ISession GetSession()
        {
            //UnmanagedSession
            if (this.GetUnmanagedSession() != null)
            {
                //this._log.Debug("检测到UnmanagedSession，在释放之前将使用该Session");
                return this.GetUnmanagedSession();
            }
            //temp
            if (!string.IsNullOrEmpty(this._tempAlias))
                return this._sessionManager.OpenSession(this._tempAlias);
            //预先指定
            if (!string.IsNullOrEmpty(this._alias))
                return this._sessionManager.OpenSession(this._alias);
            //默认取第一个工厂定义
            return this._sessionManager.OpenSession();
        }
        /// <summary>提供Stateless模式的session
        /// <remarks>
        /// 可用于提升批量处理和超大集合的批处理性能以及常规CRUD性能
        /// 提供DataReader形式的流式读取记录的方式
        /// 需要注意的是该模式的会话仅为性能优化设计，没有实现nh一级缓存、工作单元等特性，是面向command的设计
        /// 建议场景：
        /// 1.超大集合过滤查询有可能导致OOM的场景
        /// 2.批量插入/更新数据（无需Batch_Size设置）
        /// 上述场景可以获得极佳的性能
        /// 常规DDD业务不建议使用
        /// 请使用using语法在使用后关闭连接
        /// </remarks>
        /// </summary>
        /// <returns></returns>
        protected IStatelessSession GetStatelessSession()
        {
            //预先指定
            if (!string.IsNullOrEmpty(this._alias))
                return this._sessionManager.OpenStatelessSession(this._alias);
            //默认取第一个工厂定义
            return this._sessionManager.OpenStatelessSession();
        }
        //获取上下文中声明的不在_sessionManager中托管的ISession
        private ISession GetUnmanagedSession()
        {
            return CallContext.GetData(NHibernateRepositoryUtility.UnmanagedSessionKey) as ISession;
        }

        /// <summary>用于提供临时切换数据库的支持
        /// </summary>
        protected class SwitchAlias : IDisposable
        {
            private NHibernateRepositoryBase<TKey, TEntity> _repository;
            /// <summary>用于提供临时切换数据库的支持
            /// </summary>
            /// <param name="repository"></param>
            /// <param name="tempAlias"></param>
            public SwitchAlias(NHibernateRepositoryBase<TKey, TEntity> repository, string tempAlias)
            {
                this._repository = repository;
                this._repository._tempAlias = tempAlias;
                this._repository._log.DebugFormat("临时切换Alias为{0}", tempAlias);
            }

            #region IDisposable Members

            public void Dispose()
            {
                this._repository._tempAlias = string.Empty;
            }

            #endregion
        }
    }
}