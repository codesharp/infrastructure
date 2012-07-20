using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using log4net.Appender;
using NServiceBus;

using CodeSharp.ServiceFramework.NServiceBuses;
using CodeSharp.ServiceFramework.Castles;
using Castle.MicroKernel.Registration;
using CodeSharp.ServiceFramework.Interfaces;

namespace CodeSharp.ServiceFramework.NServiceBuses
{
    /// <summary>
    /// 为ServiceFramework增加NServiceBuses扩展
    /// </summary>
    public static class NServiceBusExtensions
    {
        /// <summary>
        /// 使用NServiceBus作为ESB实现
        /// </summary>
        /// <param name="config"></param>
        /// <param name="isWeb">是否宿主在web环境</param>
        /// <param name="inputQueue">用于接收消息的队列名</param>
        /// <param name="errorQueue">用于放置错误消息的队列名</param>
        /// <param name="maxRetries">最大重试次数</param>
        /// <returns></returns>
        public static ConfigurationWithCastle NServiceBus(this ConfigurationWithCastle config
            , bool isWeb
            , string inputQueue
            , string errorQueue
            , int maxRetries)
        {
            return NServiceBusExtensions.NServiceBus(config, isWeb, inputQueue, errorQueue, maxRetries, true);
        }
        /// <summary>
        /// 使用NServiceBus作为ESB实现
        /// </summary>
        /// <param name="config"></param>
        /// <param name="isWeb">是否宿主在web环境</param>
        /// <param name="inputQueue">用于接收消息的队列名</param>
        /// <param name="errorQueue">用于放置错误消息的队列名</param>
        /// <param name="maxRetries">最大重试次数</param>
        /// <param name="isTransactional">是否使用事务性队列，默认启用，设为true时maxRetries才有效，但需要注意DTC带来的问题，如：意外的事务合并等</param>
        /// <returns></returns>
        public static ConfigurationWithCastle NServiceBus(this ConfigurationWithCastle config
            , bool isWeb
            , string inputQueue
            , string errorQueue
            , int maxRetries
            , bool isTransactional)
        {
            //初始化bus
            var bus = GetConfigure(isWeb)
                .CustomConfigurationSource(new ConfigSource(inputQueue, errorQueue, maxRetries))
                .CastleWindsorBuilder(config.WindsorContainer)
                .Log4Net()
                .XmlSerializer()//消息xml序列化
                .MsmqTransport()//使用MSMQ
                    .IsTransactional(isTransactional)//是否事务性队列
                    .PurgeOnStartup(false)//是否启动后清除队列中原有消息
                .UnicastBus()
                .ImpersonateSender(false)
                .LoadMessageHandlers()
                .CreateBus()
                .Start();
            //注册异步处理实现
            config.WindsorContainer.Register(Component.For<IAsyncHandle>().ImplementedBy<AsyncHandle>());
            return config;
        }

        private static NServiceBus.Configure GetConfigure(bool isWeb)
        {
            //HACK:Configure.WithWeb()和Configure.Instance并非同一个实例？
            if (isWeb)
                Configure.WithWeb();
            else
                Configure.With();
            //TODO:调整扫描方式，无需全部扫描
            return Configure.Instance;
        }
    }
}