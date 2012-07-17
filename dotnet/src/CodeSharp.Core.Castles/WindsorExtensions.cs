//Copyright (c) CodeSharp.  All rights reserved. - http://www.codesharp.cn/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

using Castle.Core;
using Castle.Core.Configuration;
using Castle.Core.Interceptor;
using Castle.Facilities.NHibernateIntegration;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using CodeSharp.Core.RepositoryFramework;
using CodeSharp.Core.Services;

namespace CodeSharp.Core.Castles
{
    /// <summary>Windsor注册扩展
    /// </summary>
    public static class WindsorExtensions
    {
        private static readonly string _slot = "____IgnoreWhenRegister";
        private static ILog _log { get { return DependencyResolver.Resolve<ILoggerFactory>().Create(typeof(WindsorExtensions)); } }

        #region LoggerFactory
        /// <summary>设置Log4NetLoggerFactory为日志工具
        /// <remarks>log4net需要在外部初始化</remarks>
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        public static IWindsorContainer Log4NetLoggerFactory(this IWindsorContainer container)
        {
            container.Register(Component.For<ILoggerFactory>()
                .ImplementedBy<Log4NetLoggerFactory>()
                .LifeStyle.Singleton);
            return container;
        }
        /// <summary>设置Log4NetLoggerFactory为日志工具
        /// <remarks>log4net需要在外部初始化</remarks>
        /// </summary>
        /// <param name="container"></param>
        /// <param name="runtimeProperties">额外的指定一些运行时信息，以便于日志记录</param>
        /// <param name="runtimePropertiesWhenError">在Error以及Fatal级别时使用</param>
        /// <returns></returns>
        public static IWindsorContainer Log4NetLoggerFactory(this IWindsorContainer container
            , Func<IDictionary<string, object>> runtimeProperties
            , Func<IDictionary<string, object>> runtimePropertiesWhenError)
        {
            //container.Kernel.ConfigurationStore.AddComponentConfiguration(typeof(Log4NetLoggerFactory).AssemblyQualifiedName
            //    , BuildConfig<Log4NetLoggerFactory>("properties", runtimeProperties));

            //var dict = runtimeProperties == null
            //    ? new Dictionary<string, string>()
            //    : runtimeProperties.ToDictionary(o => o.Key, o => (o.Value ?? "").ToString());

            container.Register(Component.For<ILoggerFactory>()
                .ImplementedBy<Log4NetLoggerFactory>()
                .Named(typeof(Log4NetLoggerFactory).AssemblyQualifiedName)
                .DependsOn(Property.ForKey("properties").Eq(runtimeProperties))
                .DependsOn(Property.ForKey("propertiesWhenError").Eq(runtimePropertiesWhenError))
                .LifeStyle.Singleton);

            return container;
        }
        #endregion

        //常规依赖注入

        #region 注册Repository
        /// <summary>注册符合约定的Repository
        /// <remarks>约定为TestRepository</remarks>
        /// </summary>
        /// <param name="container"></param>
        /// <param name="assemblies"></param>
        /// <returns></returns>
        public static IWindsorContainer RegisterRepositories(this IWindsorContainer container, params Assembly[] assemblies)
        {
            //typeof().GetInterfaces
            //if (assemblies != null)
            //    assemblies.ToList().ForEach(assembly =>
            //        container.Register(AllTypes
            //            .FromAssembly(assembly)
            //            .BasedOn(typeof(NHibernateRepositoryBase<,>))
            //            .WithService
            //            .FromInterface()
            //            .Configure(r => r.Forward(typeof(IRepository<,>)))));
            container.RegisterFromInterface(Util.IsRepository, assemblies);
            return container;
        }
        /// <summary>注册符合约定的Repository
        /// <remarks>约定为TestRepository</remarks>
        /// </summary>
        /// <param name="container"></param>
        /// <param name="types"></param>
        /// <returns></returns>
        public static IWindsorContainer RegisterRepositories(this IWindsorContainer container, params Type[] types)
        {
            container.RegisterFromInterface(Util.IsRepository, types);
            return container;
        }
        /// <summary>注册符合约定的Repository
        /// <remarks>约定为TestRepository</remarks>
        /// </summary>
        /// <param name="container"></param>
        /// <param name="assemblies"></param>
        /// <param name="interceptors">拦截器</param>
        /// <returns></returns>
        public static IWindsorContainer RegisterRepositories(this IWindsorContainer container, Assembly[] assemblies, params Type[] interceptors)
        {
            container.RegisterFromInterface(Util.IsRepository, assemblies, interceptors);
            return container;
        }
        /// <summary>注册符合约定的Repository
        /// <remarks>约定为TestRepository</remarks>
        /// </summary>
        /// <param name="container"></param>
        /// <param name="types"></param>
        /// <param name="interceptors">拦截器</param>
        /// <returns></returns>
        public static IWindsorContainer RegisterRepositories(this IWindsorContainer container, Type[] types, params Type[] interceptors)
        {
            container.RegisterFromInterface(Util.IsRepository, types, interceptors);
            return container;
        }
        #endregion

        #region 注册Service
        /// <summary>注册符合约定的Service
        /// <remarks>约定为TestService</remarks>
        /// </summary>
        /// <param name="container"></param>
        /// <param name="assemblies"></param>
        /// <returns></returns>
        public static IWindsorContainer RegisterServices(this IWindsorContainer container, params Assembly[] assemblies)
        {
            container.RegisterFromInterface(Util.IsService, assemblies);
            return container;
        }
        /// <summary>注册符合约定的Services
        /// </summary>
        /// <param name="container"></param>
        /// <param name="types"></param>
        /// <returns></returns>
        public static IWindsorContainer RegisterServices(this IWindsorContainer container, params Type[] types)
        {
            container.RegisterFromInterface(Util.IsService, types);
            return container;
        }
        /// <summary>注册符合约定的Services
        /// </summary>
        /// <param name="container"></param>
        /// <param name="assemblies"></param>
        /// <param name="interceptors">拦截器</param>
        /// <returns></returns>
        public static IWindsorContainer RegisterServices(this IWindsorContainer container, Assembly[] assemblies, params Type[] interceptors)
        {
            container.RegisterFromInterface(Util.IsService, assemblies, interceptors);
            return container;
        }
        /// <summary>注册符合约定的Services
        /// </summary>
        /// <param name="container"></param>
        /// <param name="types"></param>
        /// <param name="interceptors">拦截器</param>
        /// <returns></returns>
        public static IWindsorContainer RegisterServices(this IWindsorContainer container, Type[] types, params Type[] interceptors)
        {
            container.RegisterFromInterface(Util.IsService, types, interceptors);
            return container;
        }
        #endregion

        #region 注册Component
        /// <summary>
        /// 注册符合约定的Component
        /// <remarks>约定为Component属性</remarks>
        /// </summary>
        /// <param name="container"></param>
        /// <param name="assemblies"></param>
        /// <returns></returns>
        public static IWindsorContainer RegisterComponent(this IWindsorContainer container, params Assembly[] assemblies)
        {
            container.RegisterFromInterface(Util.IsComponent, assemblies);
            return container;
        }
        /// <summary>
        /// 注册符合约定的Component
        /// <remarks>约定为Component属性</remarks>
        /// </summary>
        /// <param name="container"></param>
        /// <param name="types"></param>
        /// <returns></returns> 
        public static IWindsorContainer RegisterComponent(this IWindsorContainer container, params Type[] types)
        {
            container.RegisterFromInterface(Util.IsComponent, types);
            return container;
        }
        /// <summary>
        /// 注册符合约定的Component
        /// <remarks>约定为Component属性</remarks>
        /// </summary>
        /// <param name="container"></param>
        /// <param name="assemblies"></param>
        /// <param name="interceptors">拦截器</param>
        /// <returns></returns>
        public static IWindsorContainer RegisterComponent(this IWindsorContainer container, Assembly[] assemblies, params Type[] interceptors)
        {
            container.RegisterFromInterface(Util.IsComponent, assemblies, interceptors);
            return container;
        }
        /// <summary>
        /// 注册符合约定的Component
        /// <remarks>约定为Component属性</remarks>
        /// </summary>
        /// <param name="container"></param>
        /// <param name="types"></param>
        /// <param name="interceptors">拦截器</param>
        /// <returns></returns>
        public static IWindsorContainer RegisterComponent(this IWindsorContainer container, Type[] types, params Type[] interceptors)
        {
            container.RegisterFromInterface(Util.IsComponent, types, interceptors);
            return container;
        }
        #endregion

        #region 自动根据接口注册组件
        /// <summary>
        /// 自动根据接口注册组件
        /// </summary>
        /// <param name="container"></param>
        /// <param name="typePredicate"></param>
        /// <param name="assemblies"></param>
        /// <returns></returns>
        public static IWindsorContainer RegisterFromInterface(this IWindsorContainer container
            , Func<Type, bool> typePredicate
            , params Assembly[] assemblies)
        {
            return container.RegisterFromInterface(typePredicate, assemblies, new Type[] { });
        }
        /// <summary>
        /// 自动根据接口注册组件
        /// </summary>
        /// <param name="container"></param>
        /// <param name="typePredicate"></param>
        /// <param name="types"></param>
        /// <returns></returns>
        public static IWindsorContainer RegisterFromInterface(this IWindsorContainer container
            , Func<Type, bool> typePredicate
            , Type[] types)
        {
            return container.RegisterFromInterface(typePredicate, types, new Type[] { });
        }
        /// <summary>
        /// 自动根据接口注册组件
        /// </summary>
        /// <param name="container"></param>
        /// <param name="typePredicate">类型判断</param>
        /// <param name="assemblies"></param>
        /// <param name="interceptors">拦截器</param>
        /// <returns></returns>
        public static IWindsorContainer RegisterFromInterface(this IWindsorContainer container
            , Func<Type, bool> typePredicate
            , Assembly[] assemblies
            , params Type[] interceptors)
        {
            if (assemblies != null)
                assemblies.ToList().ForEach(assembly =>
                    container.RegisterFromInterface(typePredicate, assembly.GetExportedTypes(), interceptors));
            return container;
        }
        /// <summary>
        /// 自动根据接口注册组件
        /// </summary>
        /// <param name="container"></param>
        /// <param name="typePredicate"></param>
        /// <param name="types"></param>
        /// <param name="interceptors"></param>
        /// <returns></returns>
        public static IWindsorContainer RegisterFromInterface(this IWindsorContainer container
            , Func<Type, bool> typePredicate
            , Type[] types
            , params Type[] interceptors)
        {
            if (types == null) return container;

            types.ToList().ForEach(type =>
            {
                if (typePredicate(type))
                {
                    if (!IsIgnore())
                        GenerateParameters(type);
                    //生命周期
                    var life = ParseLifeStyle(type);

                    //实现注册
                    var typeKey = type.FullName;
                    if (!container.Kernel.HasComponent(typeKey))
                    {
                        var c = Component
                            .For(type)
                            .Named(typeKey)
                            .DynamicParameters((k, d) => GenerateDynamicParameters(type, d))
                            .Interceptors(interceptors);
                        container.Register(ParseRegistration(c, life));
                        _log.DebugFormat("将{0}实现注册为键{1}，{2}", type.FullName, typeKey, life);
                    }
                    //接口注册
                    (type.GetInterfaces() ?? new Type[] { }).ToList().ForEach(service =>
                    {
                        var key = service.FullName + "#" + type.FullName;

                        if (!container.Kernel.HasComponent(key))
                        {
                            var c = Component
                                .For(service)
                                .ImplementedBy(type)
                                .Named(key)
                                .DynamicParameters((k, d) => GenerateDynamicParameters(type, d))
                                .Interceptors(interceptors);
                            container.Register(ParseRegistration(c, life));
                            _log.DebugFormat("将类型{0}注册为{1}，{2}，键名={3}", type.FullName, service.FullName, life, key);
                        }
                    });
                }
            });
            return container;
        }
        #endregion

        #region 新增注册扩展 由于上述早期提供的注册方法均需要满足typePredicate条件，故增加下列方法以弥补
        /// <summary>
        /// 自动根据接口注册组件
        /// </summary>
        /// <param name="container"></param>
        /// <param name="type"></param>
        /// <param name="interceptors"></param>
        /// <returns></returns>
        public static IWindsorContainer RegisterFromInterface(this IWindsorContainer container
            , Type type
            , params Type[] interceptors)
        {
            return container.RegisterFromInterface(o => true, new Type[] { type }, interceptors);
        }
        #endregion

        #region IgnoreWhenRegister
        /// <summary>
        /// 组件注册时忽略不存在的依赖参数项
        /// <remarks>目前只支持检查非组件的静态依赖项</remarks>
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        public static IWindsorContainer IgnoreWhenRegister(this IWindsorContainer container)
        {
            Thread.SetData(Thread.GetNamedDataSlot(_slot), true);
            return container;
        }
        /// <summary>
        /// 是否忽略静态依赖检查
        /// </summary>
        /// <returns></returns>
        private static bool IsIgnore()
        {
            return Convert.ToBoolean(Thread.GetData(Thread.GetNamedDataSlot(_slot)));
        }
        #endregion

        #region 生成配置静态依赖 目前支持类型：字符串 以下方法目前仅用于静态检查
        /// <summary>
        /// 生成字符串配置依赖
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static Parameter[] GenerateParameters(Type type)
        {
            //字符串配置依赖
            var parameters = new List<Parameter>();
            type.GetConstructors().ToList().ForEach(o => FillParameters(parameters, o));

            return parameters.ToArray();
        }
        private static bool FillParameters(List<Parameter> parameters, ConstructorInfo constructor)
        {
            var config = CodeSharp.Core.Configuration.Instance();
            var list = constructor.GetParameters().Where(p =>
            {
                if (p.ParameterType != typeof(string))
                    return false;
                try { config.GetSetting(p.Name); }
                catch (KeyNotFoundException)
                {
                    _log.DebugFormat("{0}的依赖{1}未满足，请检查您的properties.config", constructor.DeclaringType.FullName, p.Name);
                    return false;
                }
                return true;
            });
            parameters.AddRange(list.Select(p => Parameter.ForKey(p.Name).Eq(config.GetSetting(p.Name))));
            return true;
        }
        #endregion

        #region 生成动态依赖
        private static void GenerateDynamicParameters(Type type, IDictionary dic)
        {
            //字符串配置依赖
            var parameters = new List<Parameter>();
            type.GetConstructors().ToList().ForEach(o => FillDynamicParameters(dic, o));
        }
        private static bool FillDynamicParameters(IDictionary dic, ConstructorInfo constructor)
        {
            var config = CodeSharp.Core.Configuration.Instance();
            var list = constructor.GetParameters().Where(p =>
            {
                if (p.ParameterType != typeof(string))
                    return false;
                return config.GetSetting(p.Name, false) != null;
            });
            list.ToList().ForEach(p => dic[p.Name] = config.GetSetting(p.Name));
            return true;
        }
        #endregion

        private static LifeStyle ParseLifeStyle(Type type)
        {
            var attrs = type.GetCustomAttributes(typeof(ComponentAttribute), false);
            return attrs.Count() <= 0
                ? LifeStyle.Transient
                : (attrs[0] as ComponentAttribute).LifeStyle;
        }
        private static ComponentRegistration<T> ParseRegistration<T>(ComponentRegistration<T> c, LifeStyle lifeStyle) where T : class
        {
            if (lifeStyle == LifeStyle.Singleton)
                return c.LifeStyle.Singleton;
            return c.LifeStyle.Transient;
        }
        [Obsolete]
        private static IConfiguration BuildConfig<T>(string name, IDictionary<string, object> configs)
        {
            var componentConfig = new MutableConfiguration(typeof(T).AssemblyQualifiedName);
            var parameters = componentConfig.CreateChild("parameters");
            //构建构造器参数集合依赖
            var props = parameters.CreateChild(name);
            //创建dictionary对象
            var dict = props.CreateChild("dictionary");

            if (configs != null)
                foreach (var c in configs)
                    dict.CreateChild("item", (c.Value ?? "").ToString()).Attribute("key", c.Key);

            return componentConfig;
        }
    }
}