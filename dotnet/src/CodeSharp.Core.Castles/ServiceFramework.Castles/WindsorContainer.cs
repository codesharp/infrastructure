using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Reflection;

using CodeSharp.ServiceFramework.Interfaces;
using Castle.Windsor;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel;

namespace CodeSharp.ServiceFramework.Castles
{
    /// <summary>
    /// Windsor实现的服务容器
    /// </summary>
    public class WindsorContainer : IContainer
    {
        internal IWindsorContainer _container;
        private DefaultContainer _default;
        private ILog _log;
        private ILog log
        {
            get
            {
                if (this._log == null)
                    this._log = (this._container.Kernel.HasComponent(typeof(ILoggerFactory))
                        ? this._container.Resolve<ILoggerFactory>()
                        : this._default.Resolve<ILoggerFactory>()).Create(typeof(WindsorContainer));
                return this._log;
            }
        }
        private Type _interceptor;
        private static readonly string _defaultComponent = "defaultComponent";

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="container"></param>
        /// <param name="interceptor">设置远程拦截器</param>
        public WindsorContainer(IWindsorContainer container, Type interceptor)
        {
            this._default = new DefaultContainer();
            this._container = container;
            this._container.Register(Component.For<RemoteServiceInterceptor>().ImplementedBy(interceptor));
        }

        #region IContainer Members
        /// <summary>
        /// 获取类型实例，找不到则使用默认实现
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public object Resolve(Type type)
        {
            try
            {
                var obj = this._container.Resolve(type);
                if (obj == null)
                    throw new Exception("未能从容器中获取类型" + type);
                return obj;
            }
            catch (Exception e)
            {
                var i = this._default.Resolve(type);
#if DEBUG
                //this.log.Debug(string.Format("未能从Windsor容器中获得组件{0}，返回默认实现{1}", type, i), e);
#endif
                return i;
            }
        }
        /// <summary>
        /// 获取类型实例，找不到则使用默认实现
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Resolve<T>() where T : class
        {
            return (T)this.Resolve(typeof(T));
        }
        /// <summary>
        /// 注册类型
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="classType"></param>
        public void Register(Type serviceType, Type classType)
        {
            this._container.Register(Component.For(serviceType).ImplementedBy(classType));
        }
        /// <summary>
        /// 注册实现
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="instance"></param>
        public void Register(Type serviceType, object instance)
        {
            this._container.Register(Component.For(serviceType).Instance(instance));
        }
        /// <summary>
        /// 注册远程服务类型
        /// </summary>
        /// <param name="serviceTypes"></param>
        public void RegisterRemoteServices(params Type[] serviceTypes)
        {
            if (serviceTypes == null) return;
            serviceTypes.ToList().ForEach(type => this.AddRemoteType(type));
        }
        /// <summary>
        /// 移除远程服务类型
        /// </summary>
        /// <param name="serviceTypes"></param>
        public void ClearRemoteServices(params Type[] serviceTypes)
        {
            if (serviceTypes == null) return;
            serviceTypes.ToList().ForEach(type => this.RemoveRemoteType(type));
        }
        #endregion

        private void AddRemoteType(Type type)
        {
            if (type == null)
            {
                //this.log.Debug("向容器注册远程服务时，提供了一个Null的类型，请检查您的程序中是否含有该服务的本地代理");
                return;
            }
            var key = this.GenerateKey(type);
            //var localKey = this.GenerateLocalKey(type);
            ////HACK:由于容器原因，此处耦合于Taobao.Infrastructure.Castle中的NamingSubSystem
            ////激活
            //if (this._container.Kernel.HasComponent(key))
            //{
            //    var handler = this._container.Kernel.GetHandler(key);
            //    handler.ComponentModel.Configuration.Attributes[_defaultComponent] = "true";
            //    this._container.Kernel.RaiseHandlersChanged();
            //    return;
            //}
            ////注册到容器中并注册远程服务拦截器
            //this._container.Register(Component
            //    .For(type)
            //    .Named(key)
            //    .LifeStyle.Singleton
            //    .Configuration(Attrib.ForName(_defaultComponent).Eq("true"))
            //    .Interceptors(typeof(RemoteServiceInterceptor)));
            //TODO：动态依赖过于繁琐，暂不实现
            //.DynamicParameters((k, d) => GenerateDynamicParameters(type, d)));

            //by wsky 20120720
            //Castle 3.x make it easy
            this._container.Register(Component
                .For(type)
                .Named(key + "_" + Guid.NewGuid())
                .LifeStyle.Singleton
                .Interceptors(typeof(RemoteServiceInterceptor))
                .IsDefault());

            this.log.InfoFormat("注册了键名为{0}的远程服务，类型为{1}", key, type.FullName);
        }
        private void RemoveRemoteType(Type type)
        {
            if (type == null) return;
            var key = this.GenerateKey(type);
            if (!this._container.Kernel.HasComponent(key))
                return;

            //var handler = this._container.Kernel.GetHandler(key).ComponentModel;
            //handler.ComponentModel.Configuration.Attributes[_defaultComponent] = "false";
            //this._container.Kernel.RaiseHandlersChanged();
            //this.log.DebugFormat("从容器中移除键名为{0}的远程服务组件={1}", key, this._container.Kernel.RemoveComponent(key));

            //by wsky 20120720
            //Castle 3.x make it easy
            this._container.Register(Component
                .For(type)
                .Named(key + "_" + Guid.NewGuid())
                .Instance(null)
                .IsDefault());
        }
        private string GenerateKey(Type type)
        {
            return "____Remote_" + type.FullName;
        }
        private string GenerateLocalKey(Type type)
        {
            return type.FullName;
        }
    }
}