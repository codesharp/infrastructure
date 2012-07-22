//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Castle.MicroKernel.Registration;
using CodeSharp.Core.Castles;
using CodeSharp.Core.Services;

namespace CodeSharp.Framework.Castles
{
    /// <summary>基于Castle完成容器化架构的配置初始化框架
    /// </summary>
    public class SystemConfigWithCastle : SystemConfig
    {
        //该类实际职责为装饰扩展此实例
        private SystemConfig _config;
        private bool _isBusinessFoundation;

        public override bool IsDependencyResolverEnable
        {
            get
            {
                return this._config.IsDependencyResolverEnable;
            }
            set
            {
                this._config.IsDependencyResolverEnable = value;
            }
        }
        public SystemConfigWithCastle(SystemConfig config)
            : base(config)
        {
            //HACK:该装饰器实现需要注意和原始对象的一致性问题
            this._config = config;
            this._config.IsDependencyResolverEnable = true;
            //启用castle
            this._config.FoundationConfig.Castle(o => { PrepareBaseDependency(this, o); });
        }

        #region new/override
        /// <summary>
        /// 从配置程序集中按规则读取并在应用配置文件目录下生成额外的配置文件
        /// 使用默认的文件变量
        /// </summary>
        /// <param name="fileName">文件名称，如：log4net.config</param>
        /// <returns></returns>
        public new SystemConfigWithCastle File(string fileName)
        {
            this._config.File(fileName);
            return this;
        }
        /// <summary>
        /// 从配置程序集中按规则读取并在应用配置文件目录下生成额外的配置文件
        /// </summary>
        /// <param name="fileName">文件名称，如：log4net.config</param>
        /// <param name="fileParameters">定义要在文本中替换的文件变量</param>
        /// <returns></returns>
        public new SystemConfigWithCastle File(string fileName, IDictionary<string, string> fileParameters)
        {
            this._config.File(fileName, fileParameters);
            return this;
        }
        /// <summary>
        /// 启用系统配置表功能
        /// <remarks>除非要显示指定刷新间隔，一般不需要单独调用此声明</remarks>
        /// </summary>
        /// <param name="interval">与上一次刷新时间对比，刷新时间最小间隔，默认60s</param>
        /// <returns></returns>
        public new SystemConfigWithCastle EnableSysConfig(TimeSpan interval)
        {
            this._config.EnableSysConfig(interval);
            return this;
        }
        /// <summary>
        /// 读取基础配置表数据
        /// <remarks>该表包含了大部分程序运行的基本配置信息</remarks>
        /// </summary>
        /// <returns></returns>
        public new SystemConfigWithCastle ReadCommonProperties()
        {
            this._config.ReadCommonProperties();
            return this;
        }
        /// <summary>
        /// 添加全局缓存
        /// <remarks>添加后将对其进行简易管理，非WeakReference</remarks>
        /// </summary>
        /// <param name="key">缓存的键</param>
        /// <param name="cache">缓存内容</param>
        /// <returns></returns>
        public new SystemConfigWithCastle NonWeakReferenceCache(string key, NonWeakReferenceCacheManager.ICache cache)
        {
            this._config.NonWeakReferenceCache(key, cache);
            return this;
        }
        /// <summary>
        /// 读取配置表并填充至基础配置实例中
        /// <remarks>填充进配置实例的配置项可被用于支持依赖注入</remarks>
        /// </summary>
        /// <param name="key">配置表的表名</param>
        /// <returns></returns>
        public new SystemConfigWithCastle ReadProperties(string key)
        {
            this._config.ReadProperties(key);
            return this;
        }
        /// <summary>
        /// 读取配置表并填充至基础配置实例中
        /// <remarks>填充进配置实例的配置项可被用于支持依赖注入</remarks>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="isIndependent">是否读取为独立配置表，独立配置表不会被填充到基础配置实例中</param>
        /// <returns></returns>
        public new SystemConfigWithCastle ReadProperties(string key, bool isIndependent)
        {
            this._config.ReadProperties(key, isIndependent);
            return this;
        }
        /// <summary>
        /// 设置默认zh-CN的时区/culture
        /// </summary>
        /// <returns></returns>
        public new SystemConfigWithCastle Globalization()
        {
            this._config.Globalization();
            return this;
        }
        #endregion

        /// <summary>额外的容器初始化
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        public SystemConfigWithCastle Resolve(Action<WindsorResolver> func)
        {
            func(DependencyResolver.Resolver as WindsorResolver);
            return this;
        }
        /// <summary>自定义异常体系，若需要定义系统的已知未知异常等请调用此方法进行定义
        /// </summary>
        /// <param name="exceptionSystem">异常体系的实例</param>
        /// <returns></returns>
        public new SystemConfigWithCastle ExceptionSystem(IExceptionSystem exceptionSystem)
        {
            (DependencyResolver.Resolver as WindsorResolver).Container
                .Register(Component
                .For<IExceptionSystem>()
                .Instance(exceptionSystem)
                .IsDefault());
            return this;
        }
        /// <summary>自定义异常体系，若需要定义系统的已知未知异常等请调用此方法进行定义
        /// </summary>
        /// <typeparam name="T">异常体系的实现类型</typeparam>
        /// <returns></returns>
        public SystemConfigWithCastle ExceptionSystem<T>() where T : IExceptionSystem
        {
            (DependencyResolver.Resolver as WindsorResolver).Container
                .Register(Component
                .For<IExceptionSystem>()
                .ImplementedBy<T>()
                .IsDefault());
            return this;
        }
        /// <summary>对业务依赖按约定进行注册
        /// <remarks>
        /// 默认的约定包含：service/dao/repository/factory/component
        /// </remarks>
        /// </summary>
        /// <param name="assemblies"></param>
        /// <returns></returns>
        public SystemConfigWithCastle BusinessDependency(params Assembly[] assemblies)
        {
            this._isBusinessFoundation = true;
            return assemblies == null ? this : this.Resolve(o =>
            {
                var windsor = o.Container;
                //DDD实现模式支持
                windsor.RegisterRepositories(assemblies);
                windsor.RegisterServices(assemblies, typeof(ServiceInterceptor));
                //自定义类型注册
                windsor.RegisterComponent(assemblies, typeof(ServiceInterceptor));
                windsor.RegisterFromInterface(IsFactory, assemblies);
                windsor.RegisterFromInterface(IsDao, assemblies);
                windsor.RegisterFromInterface(IsSpecial, assemblies);
            });
        }

        internal void CheckBusinessFoundation()
        {
            if (!this._isBusinessFoundation)
                throw new InvalidOperationException("由于该特性依赖基础业务/功能模块，请先调用BusinessFoundation()");
        }

        private static bool IsDao(Type type)
        {
            return type != null
                && (type.Name.EndsWith("Dao", StringComparison.OrdinalIgnoreCase)
                || type.Name.EndsWith("Dal", StringComparison.OrdinalIgnoreCase))
                && !type.IsAbstract
                && !type.IsInterface;
        }
        private static bool IsFactory(Type type)
        {
            return type != null
                && type.Name.EndsWith("Factory", StringComparison.OrdinalIgnoreCase)
                && !type.IsAbstract
                && !type.IsInterface;
        }
        private static bool IsSpecial(Type type)
        {
            return type != null
                && (type.Name.EndsWith("Rule", StringComparison.OrdinalIgnoreCase)
                || type.Name.EndsWith("Connection", StringComparison.OrdinalIgnoreCase))
                && !type.IsAbstract
                && !type.IsInterface;
        }
        private static void PrepareBaseDependency(SystemConfigWithCastle config, WindsorResolver resolver)
        {
            //基础服务使用全局容器
            var windsor = resolver.Container;
            //注册工厂支持
            //windsor.AddFacility<Castle.Facilities.FactorySupport.FactorySupportFacility>();
            //注册基本拦截器
            windsor.Register(Component.For<ServiceInterceptor>().LifeStyle.Transient);
            //不可使用此方式会引起对象正常依赖行为
            //windsor.RegisterFromInterface(typeof(ServiceInterceptor));
            //注册Configure中完成无容器的默认依赖
            config.ExceptionSystem<DefaultExceptionSystem>();
        }
    }
}