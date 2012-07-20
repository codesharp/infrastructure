using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

using NServiceBus.Config;
using NServiceBus.Config.ConfigurationSource;
using NServiceBus.Unicast;
using NServiceBus.Unicast.Transport.Msmq;

namespace CodeSharp.ServiceFramework.NServiceBuses
{
    /// <summary>
    /// 配置声明
    /// </summary>
    public class ConfigSource : IConfigurationSource
    {
        private string _inputQueue;
        private string _errorQueue;
        private string _endpointMapping;
        private int _maxRetries;
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="inputQueue">用于接收消息的队列名</param>
        /// <param name="errorQueue">用于放置错误消息的队列名</param>
        /// <param name="maxRetries">最大重试次数</param>
        public ConfigSource(string inputQueue, string errorQueue, int maxRetries)
        {
            this._inputQueue = inputQueue;
            //HACK:unicast模式下的分发队列，目前无需使用
            //this._endpointMapping = mappingQueue;
            this._errorQueue = errorQueue;
            this._maxRetries = maxRetries;
        }

        #region IConfigurationSource Members
        public T GetConfiguration<T>() where T : class
        {
            if (typeof(T) == typeof(MsmqTransportConfig))
                return this.GenerateMsmqTransportConfig() as T;
            if (typeof(T) == typeof(UnicastBusConfig))
                return this.GenerateUnicastBusConfig() as T;
            return ConfigurationManager.GetSection(typeof(T).Name) as T;
        }
        #endregion

        private MsmqTransportConfig GenerateMsmqTransportConfig()
        {
            return new MsmqTransportConfig
            {
                InputQueue = this._inputQueue,
                ErrorQueue = this._errorQueue,
                NumberOfWorkerThreads = 1,//HACK:社区版本只能使用1个工作线程
                MaxRetries = this._maxRetries
            };
        }
        private UnicastBusConfig GenerateUnicastBusConfig()
        {
            var config = new UnicastBusConfig();
            config.MessageEndpointMappings.Add(new MessageEndpointMapping()
            {
                Messages = "CodeSharp.ServiceFramework.Castles",
                Endpoint = this._inputQueue
            });
            return config;
        }
    }
}
