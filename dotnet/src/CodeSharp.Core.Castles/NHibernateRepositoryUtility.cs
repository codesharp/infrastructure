//Copyright (c) CodeSharp.  All rights reserved. - http://www.codesharp.cn/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Remoting.Messaging;

using NHibernate;
using CodeSharp.Core.Services;
using ILoggerFactory = CodeSharp.Core.Services.ILoggerFactory;

namespace CodeSharp.Core.Castles
{
    /// <summary>
    /// 为NHibernate仓储提供附加功能
    /// <remarks></remarks>
    /// </summary>
    public static class NHibernateRepositoryUtility
    {
        internal static readonly string UnmanagedSessionKey = "____UnmanagedSession";

        /// <summary>
        /// 使用默认的ISessionFactory声明未托管的ISession
        /// <remarks>主要为用于解决多线程环境session问题或session管理拆解</remarks>
        /// </summary>
        /// <returns></returns>
        public static UnmanagedSessionRegion UnmanagedSession()
        { 
            return new UnmanagedSessionRegion();
        }
        /// <summary>
        /// 使用默认的ISessionFactory声明未托管的ISession，并允许指定alias
        /// <remarks>
        /// 主要为用于解决多线程环境session问题或session管理拆解
        /// 这是一个强耦合的实现（castle/nhibernate），并非用于应对常规业务场景
        /// </remarks>
        /// </summary>
        /// <param name="alias">指定SessionFactory别名</param>
        /// <returns></returns>
        public static UnmanagedSessionRegion UnmanagedSession(string alias)
        {
            return new UnmanagedSessionRegion(alias);
        }
        /// <summary>
        /// 声明未托管的ISession使用区域
        /// <remarks>
        /// 该声明session会被区域内的NHibernateRepositoryBase以及子类共享
        /// 目前支持flush但不支持事务声明，请注意区域内的事务处理
        /// 应避免区域内主动关闭session
        /// </remarks>
        /// </summary>
        public class UnmanagedSessionRegion : IDisposable
        {
            private ILog _log;
            private ISession _prev;
            private ISession _session;
            internal UnmanagedSessionRegion() : this(string.Empty) { }
            internal UnmanagedSessionRegion(string alias)
            {
                this._log = DependencyResolver
                    .Resolve<ILoggerFactory>()
                    .Create(typeof(UnmanagedSessionRegion));

                this._prev = CallContext.GetData(UnmanagedSessionKey) as ISession;
                this._session = this.CreateSession(alias);
                CallContext.SetData(UnmanagedSessionKey, this._session);
                this._log.Debug("声明了未托管的ISession使用区域，alias=" + alias);
            }
            /// <summary>
            /// 执行Session.Flush
            /// </summary>
            public void Flush()
            {
                this._session.Flush();
            }

            private ISession CreateSession(string alias)
            {
                if (string.IsNullOrEmpty(alias))
                    return DependencyResolver.Resolve<ISessionFactory>().OpenSession();

                //HACK:耦合了Castle.Facilities.NHibernateIntegration
                return DependencyResolver.Resolve<Castle.Facilities.NHibernateIntegration.ISessionFactoryResolver>()
                    .GetSessionFactory(alias)
                    .OpenSession();
            }

            #region IDisposable Members

            public void Dispose()
            {
                try
                {
                    (CallContext.GetData(UnmanagedSessionKey) as ISession).Close();
                    this._log.Debug("关闭ISession");
                }
                catch (Exception e)
                {
                    this._log.Error("释放UnmanagedSessionRegion时异常", e);
                }
                CallContext.SetData(UnmanagedSessionKey, this._prev);
                this._log.Debug("释放ISession使用区域");
            }

            #endregion
        }
    }
}