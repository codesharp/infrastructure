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
using Castle.Core.Internal;

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
        private IDictionary<Type, RemoteHandlerSelector> _type2selectorHandler = new Dictionary<Type, RemoteHandlerSelector>();

        public WindsorContainer(IWindsorContainer container) : this(container, typeof(RemoteServiceInterceptor)) { }
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
            this._container.Register(Component.For<RemovedInterceptor>());
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
                if (this.log.IsDebugEnabled)
                    this.log.Debug("向容器注册远程服务时，提供了一个Null的类型，请检查您的程序中是否含有该服务的本地代理");
                return;
            }

            //var key = this.GenerateKey(type);
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
            //Castle 3.x make it easy?
            var keyRemote = this.GenerateKey(type);
            var keyRemoved = keyRemote + "_removed";
            if (this._container.Kernel.HasComponent(keyRemote))
            {
                _type2selectorHandler[type].Enable = true;

                if (this.log.IsDebugEnabled)
                    this.log.DebugFormat("激活远程服务{0}", type.FullName);
                return;
            }
            var h = new RemoteHandlerSelector(this.log, type, keyRemote, keyRemoved);
            _type2selectorHandler.Add(type, h);
            this._container.Kernel.AddHandlerSelector(h);
            this._container.Register(Component.For(type).Named(keyRemote).LifeStyle.Singleton.Interceptors(typeof(RemoteServiceInterceptor)));
            this._container.Register(Component.For(type).Named(keyRemoved).LifeStyle.Singleton.Interceptors(typeof(RemovedInterceptor)));
            this.log.InfoFormat("注册了键名为{0}的远程服务，类型为{1}", keyRemote, type.FullName);
            if (this.log.IsDebugEnabled)
                this.log.DebugFormat("为远程服务{0}注册{1}用于支持卸载", type.FullName, keyRemoved);
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
            _type2selectorHandler[type].Enable = false;
            if (this.log.IsDebugEnabled)
                this.log.DebugFormat("卸载远程服务{0}", type.FullName);
        }
        private string GenerateKey(Type type)
        {
            return "____Remote_" + type.FullName;
        }
        private string GenerateLocalKey(Type type)
        {
            return type.FullName;
        }

        //强制依赖失败的拦截器，用于替代实现组件移除/卸载
        public class RemovedInterceptor : Castle.DynamicProxy.IInterceptor
        {
            public RemovedInterceptor(object serviceUnavailable)
            {
                throw new InvalidOperationException("服务已经被卸载");
            }
            public void Intercept(Castle.DynamicProxy.IInvocation invocation)
            {
                throw new NotImplementedException();
            }
        }
        public class RemoteHandlerSelector : IHandlerSelector
        {
            private ILog _log;
            private Type _service;
            private string _keyRemote, _keyRemoved;
            public RemoteHandlerSelector(ILog log, Type service, string keyRemote, string keyRemoved)
            {
                this._log = log;
                this._service = service;
                this._keyRemote = keyRemote;
                this._keyRemoved = keyRemoved;
                
                this.Enable = true;
            }
            public bool Enable { get; set; }
            public bool HasOpinionAbout(string key, Type service)
            {
                return service == this._service;
            }
            public IHandler SelectHandler(string key, Type service, IHandler[] handlers)
            {
                if (this.Enable)
                    return handlers.First(o => o.ComponentModel.Name == this._keyRemote);
                //HACK:远程服务处于卸载状态时，若不存在非远程注册信息则返回用于替代卸载的组件
                if (handlers.All(o =>
                    o.ComponentModel.Name == this._keyRemote
                    || o.ComponentModel.Name == this._keyRemoved))
                    return handlers.First(o => o.ComponentModel.Name == this._keyRemoved);
                //存在其他注册信息则返回Null以取消该选择器的作用
                //if (this._log.IsDebugEnabled)
                //    this._log.DebugFormat("由于远程组件{0}已被卸载且存在该组件的本地实现，取消选择器", service.FullName);
                return null;
            }
        }
    }
}