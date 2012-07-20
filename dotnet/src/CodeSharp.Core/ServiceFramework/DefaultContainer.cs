using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeSharp.ServiceFramework.Interfaces;

namespace CodeSharp.ServiceFramework
{
    /// <summary>
    /// 默认容器实现
    /// </summary>
    public class DefaultContainer : Interfaces.IContainer
    {
        private IDictionary<Type, object> _pool;
        /// <summary>
        /// 初始化
        /// </summary>
        public DefaultContainer()
        {
            this._pool = new Dictionary<Type, object>();
            //注册默认实现
            this._pool.Add(typeof(ILoggerFactory), new DefaultLoggerFactory());
            this._pool.Add(typeof(ISerializer), new DefaultJSONSerializer());
            this._pool.Add(typeof(IAuthentication), new DefaultAuthentication());
            this._pool.Add(typeof(IAsyncHandle), new DefaultAsyncHandle());
            this._pool.Add(typeof(IServiceDescriptionRender), new DefaultServiceDescriptionRender());
            this._pool.Add(typeof(IRemoteHandle), new Remoting.RemotingHandle(this.Resolve<ILoggerFactory>()));
            this._pool.Add(typeof(ILoadBalancingHelper), new DefaultLoadBalancingHelper(this.Resolve<ILoggerFactory>()));
        }

        #region IContainer Members
        public T Resolve<T>() where T : class
        {
            return (T)this.Resolve(typeof(T));
        }
        public object Resolve(Type type)
        {
            return this._pool.ContainsKey(type) ? this._pool[type] : null;
        }
        /// <summary>
        /// 向容器中注册类型，若存在相同serviceType则覆盖
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="classType"></param>
        public void Register(Type serviceType, Type classType)
        {
            this.Register(serviceType, Activator.CreateInstance(classType));
        }
        /// <summary>
        /// 向容器中注册对象实例，若存在相同serviceType则覆盖
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="instance"></param>
        public void Register(Type serviceType, object instance)
        {
            if (!this._pool.ContainsKey(serviceType))
                this._pool.Add(serviceType, instance);
            else
                this._pool[serviceType] = instance;
        }
        public void RegisterRemoteServices(params Type[] serviceTypes)
        {
            //不支持
        }
        public void ClearRemoteServices(params Type[] serviceTypes)
        {
            //不支持
        }
        #endregion
    }
}
