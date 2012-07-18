//Copyright (c) CodeSharp.  All rights reserved. - http://www.codesharp.cn/

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.Windsor.Configuration.Interpreters;
using Castle.Windsor.Installer;
using CodeSharp.Core.Services;
using castleConfig = Castle.Windsor.Installer.Configuration;

namespace CodeSharp.Core.Castles
{
    /// <summary>框架配置扩展
    /// </summary>
    public static class ConfigurationExtensions
    {
        /// <summary>配置默认的Castle容器
        /// <remarks>默认使用XmlInterpreter初始化</remarks>
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static Configuration Castle(this Configuration configuration)
        {
            return ConfigurationExtensions.Castle(configuration, o => { });
        }
        /// <summary>配置默认的Castle容器
        /// <remarks>默认使用XmlInterpreter初始化</remarks>
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="func">执行额外的配置</param>
        /// <returns></returns>
        public static Configuration Castle(this Configuration configuration, Action<WindsorResolver> func)
        {
            var container = new WindsorContainer();
            //从配置文件初始化
            container.Install(new ConfigurationInstaller(new XmlInterpreter()));
            
            var resolver = new WindsorResolver(container);
            //设置解释器实例
            configuration.Resolver(resolver);

            //强制注册Log4Net日志工厂+增加注入运行时变量 20111130
            resolver.Container.Log4NetLoggerFactory(
                () => configuration.RuntimeProperties(Configuration.RuntimeProperties_Environment)
                , () => configuration.RuntimeProperties(Configuration.RuntimeProperties_Environment_Error));
            //强制设置Common.Logging使用Log4Net实现且使用外部配置的log4net实例 EXTERNAL
            Common.Logging.LogManager.Adapter = new Common.Logging.Log4Net.Log4NetLoggerFactoryAdapter(new NameValueCollection() { { "configType", "EXTERNAL" } });

            //记录启动信息
            var log = DependencyResolver
                .Resolve<ILoggerFactory>()
                .Create(typeof(ConfigurationExtensions));
            log.Info("不启用NamingSubSystem=NamingSubsystemForDefaultComponent，Castle升级至3.0后无需此子系统");
            log.Info("强制使用Log4Net日志工厂");
            log.Info("强制设置Common.Logging使用Common.Logging.Log4Net实现");
            log.Debug("来自Configuration的在日志组件未初始化前的调试信息：\n" + configuration.PopMessages());

            //额外的容器配置
            func(resolver);

            return configuration;
        }
    }
}