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
    /// <summary>Windsorע����չ
    /// </summary>
    public static class WindsorExtensions
    {
        private static readonly string _slot = "____IgnoreWhenRegister";
        private static ILog _log { get { return DependencyResolver.Resolve<ILoggerFactory>().Create(typeof(WindsorExtensions)); } }

        #region LoggerFactory
        /// <summary>����Log4NetLoggerFactoryΪ��־����
        /// <remarks>log4net��Ҫ���ⲿ��ʼ��</remarks>
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
        /// <summary>����Log4NetLoggerFactoryΪ��־����
        /// <remarks>log4net��Ҫ���ⲿ��ʼ��</remarks>
        /// </summary>
        /// <param name="container"></param>
        /// <param name="runtimeProperties">�����ָ��һЩ����ʱ��Ϣ���Ա�����־��¼</param>
        /// <param name="runtimePropertiesWhenError">��Error�Լ�Fatal����ʱʹ��</param>
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

        //��������ע��

        #region ע��Repository
        /// <summary>ע�����Լ����Repository
        /// <remarks>Լ��ΪTestRepository</remarks>
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
        /// <summary>ע�����Լ����Repository
        /// <remarks>Լ��ΪTestRepository</remarks>
        /// </summary>
        /// <param name="container"></param>
        /// <param name="types"></param>
        /// <returns></returns>
        public static IWindsorContainer RegisterRepositories(this IWindsorContainer container, params Type[] types)
        {
            container.RegisterFromInterface(Util.IsRepository, types);
            return container;
        }
        /// <summary>ע�����Լ����Repository
        /// <remarks>Լ��ΪTestRepository</remarks>
        /// </summary>
        /// <param name="container"></param>
        /// <param name="assemblies"></param>
        /// <param name="interceptors">������</param>
        /// <returns></returns>
        public static IWindsorContainer RegisterRepositories(this IWindsorContainer container, Assembly[] assemblies, params Type[] interceptors)
        {
            container.RegisterFromInterface(Util.IsRepository, assemblies, interceptors);
            return container;
        }
        /// <summary>ע�����Լ����Repository
        /// <remarks>Լ��ΪTestRepository</remarks>
        /// </summary>
        /// <param name="container"></param>
        /// <param name="types"></param>
        /// <param name="interceptors">������</param>
        /// <returns></returns>
        public static IWindsorContainer RegisterRepositories(this IWindsorContainer container, Type[] types, params Type[] interceptors)
        {
            container.RegisterFromInterface(Util.IsRepository, types, interceptors);
            return container;
        }
        #endregion

        #region ע��Service
        /// <summary>ע�����Լ����Service
        /// <remarks>Լ��ΪTestService</remarks>
        /// </summary>
        /// <param name="container"></param>
        /// <param name="assemblies"></param>
        /// <returns></returns>
        public static IWindsorContainer RegisterServices(this IWindsorContainer container, params Assembly[] assemblies)
        {
            container.RegisterFromInterface(Util.IsService, assemblies);
            return container;
        }
        /// <summary>ע�����Լ����Services
        /// </summary>
        /// <param name="container"></param>
        /// <param name="types"></param>
        /// <returns></returns>
        public static IWindsorContainer RegisterServices(this IWindsorContainer container, params Type[] types)
        {
            container.RegisterFromInterface(Util.IsService, types);
            return container;
        }
        /// <summary>ע�����Լ����Services
        /// </summary>
        /// <param name="container"></param>
        /// <param name="assemblies"></param>
        /// <param name="interceptors">������</param>
        /// <returns></returns>
        public static IWindsorContainer RegisterServices(this IWindsorContainer container, Assembly[] assemblies, params Type[] interceptors)
        {
            container.RegisterFromInterface(Util.IsService, assemblies, interceptors);
            return container;
        }
        /// <summary>ע�����Լ����Services
        /// </summary>
        /// <param name="container"></param>
        /// <param name="types"></param>
        /// <param name="interceptors">������</param>
        /// <returns></returns>
        public static IWindsorContainer RegisterServices(this IWindsorContainer container, Type[] types, params Type[] interceptors)
        {
            container.RegisterFromInterface(Util.IsService, types, interceptors);
            return container;
        }
        #endregion

        #region ע��Component
        /// <summary>
        /// ע�����Լ����Component
        /// <remarks>Լ��ΪComponent����</remarks>
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
        /// ע�����Լ����Component
        /// <remarks>Լ��ΪComponent����</remarks>
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
        /// ע�����Լ����Component
        /// <remarks>Լ��ΪComponent����</remarks>
        /// </summary>
        /// <param name="container"></param>
        /// <param name="assemblies"></param>
        /// <param name="interceptors">������</param>
        /// <returns></returns>
        public static IWindsorContainer RegisterComponent(this IWindsorContainer container, Assembly[] assemblies, params Type[] interceptors)
        {
            container.RegisterFromInterface(Util.IsComponent, assemblies, interceptors);
            return container;
        }
        /// <summary>
        /// ע�����Լ����Component
        /// <remarks>Լ��ΪComponent����</remarks>
        /// </summary>
        /// <param name="container"></param>
        /// <param name="types"></param>
        /// <param name="interceptors">������</param>
        /// <returns></returns>
        public static IWindsorContainer RegisterComponent(this IWindsorContainer container, Type[] types, params Type[] interceptors)
        {
            container.RegisterFromInterface(Util.IsComponent, types, interceptors);
            return container;
        }
        #endregion

        #region �Զ����ݽӿ�ע�����
        /// <summary>
        /// �Զ����ݽӿ�ע�����
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
        /// �Զ����ݽӿ�ע�����
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
        /// �Զ����ݽӿ�ע�����
        /// </summary>
        /// <param name="container"></param>
        /// <param name="typePredicate">�����ж�</param>
        /// <param name="assemblies"></param>
        /// <param name="interceptors">������</param>
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
        /// �Զ����ݽӿ�ע�����
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
                    //��������
                    var life = ParseLifeStyle(type);

                    //ʵ��ע��
                    var typeKey = type.FullName;
                    if (!container.Kernel.HasComponent(typeKey))
                    {
                        var c = Component
                            .For(type)
                            .Named(typeKey)
                            .DynamicParameters((k, d) => GenerateDynamicParameters(type, d))
                            .Interceptors(interceptors);
                        container.Register(ParseRegistration(c, life));
                        _log.DebugFormat("��{0}ʵ��ע��Ϊ��{1}��{2}", type.FullName, typeKey, life);
                    }
                    //�ӿ�ע��
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
                            _log.DebugFormat("������{0}ע��Ϊ{1}��{2}������={3}", type.FullName, service.FullName, life, key);
                        }
                    });
                }
            });
            return container;
        }
        #endregion

        #region ����ע����չ �������������ṩ��ע�᷽������Ҫ����typePredicate���������������з������ֲ�
        /// <summary>
        /// �Զ����ݽӿ�ע�����
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
        /// ���ע��ʱ���Բ����ڵ�����������
        /// <remarks>Ŀǰֻ֧�ּ�������ľ�̬������</remarks>
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        public static IWindsorContainer IgnoreWhenRegister(this IWindsorContainer container)
        {
            Thread.SetData(Thread.GetNamedDataSlot(_slot), true);
            return container;
        }
        /// <summary>
        /// �Ƿ���Ծ�̬�������
        /// </summary>
        /// <returns></returns>
        private static bool IsIgnore()
        {
            return Convert.ToBoolean(Thread.GetData(Thread.GetNamedDataSlot(_slot)));
        }
        #endregion

        #region �������þ�̬���� Ŀǰ֧�����ͣ��ַ��� ���·���Ŀǰ�����ھ�̬���
        /// <summary>
        /// �����ַ�����������
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static Parameter[] GenerateParameters(Type type)
        {
            //�ַ�����������
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
                    _log.DebugFormat("{0}������{1}δ���㣬��������properties.config", constructor.DeclaringType.FullName, p.Name);
                    return false;
                }
                return true;
            });
            parameters.AddRange(list.Select(p => Parameter.ForKey(p.Name).Eq(config.GetSetting(p.Name))));
            return true;
        }
        #endregion

        #region ���ɶ�̬����
        private static void GenerateDynamicParameters(Type type, IDictionary dic)
        {
            //�ַ�����������
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
            //����������������������
            var props = parameters.CreateChild(name);
            //����dictionary����
            var dict = props.CreateChild("dictionary");

            if (configs != null)
                foreach (var c in configs)
                    dict.CreateChild("item", (c.Value ?? "").ToString()).Attribute("key", c.Key);

            return componentConfig;
        }
    }
}